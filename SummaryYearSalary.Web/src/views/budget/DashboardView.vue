<template>
  <div>
    <PageHeader>
      <template #actions-left>
        <el-button type="primary" :icon="Plus" @click="router.push('/pages/transactions/add')">
          เพิ่มรายการ
        </el-button>
        <el-button :icon="Download" :loading="exporting" @click="handleExport">
          Export Excel
        </el-button>
      </template>
    </PageHeader>

    <!-- Month picker + member filter -->
    <div class="filter-bar" data-tour="dash-filter">
      <el-date-picker
        v-model="selectedDate"
        type="month"
        format="MMM BBBB"
        value-format="YYYY-MM"
        placeholder="เลือกเดือน"
        style="width: 160px"
        :disabled-date="isFutureMonth"
        :clearable="false"
      />
      <el-segmented v-model="segmentValue" :options="memberSegmentOptions" class="member-segmented" />
    </div>

    <!-- Summary cards (selected month, with month-over-month comparison) -->
    <div class="summary-grid-3" data-tour="dash-summary">
      <div class="summary-card wide fx-card">
        <div class="summary-card-accent" :style="{ background: currentBalance >= 0 ? 'var(--success)' : 'var(--danger)' }" />
        <div class="summary-card-label">คงเหลือเดือนนี้</div>
        <div class="summary-card-value" :class="currentBalance >= 0 ? 'income' : 'expense'">
          {{ currentBalance >= 0 ? '+' : '-' }}฿{{ formatAmount(Math.abs(displayBalance)) }}
        </div>
        <div class="summary-card-sub">
          {{ balanceDescription }}
          <template v-if="reportStore.previousMonthSummary">
            · เดือนก่อนคงเหลือ
            <b :class="prevBalance >= 0 ? 'income' : 'expense'">
              {{ prevBalance >= 0 ? '+' : '-' }}฿{{ formatAmount(Math.abs(prevBalance)) }}
            </b>
          </template>
        </div>
      </div>
      <div class="summary-card fx-card">
        <div class="summary-card-label">รายรับ</div>
        <div class="summary-card-value income">฿{{ formatAmount(displayIncome) }}</div>
        <div v-if="incomeChangePct !== null" class="summary-card-sub">
          <b :class="incomeChangePct >= 0 ? 'income' : 'expense'"
            >{{ incomeChangePct >= 0 ? '↑' : '↓' }} {{ Math.abs(incomeChangePct).toFixed(0) }}%</b
          >
          จาก {{ previousMonthLabel }} (฿{{ formatAmount(prevIncome) }})
        </div>
      </div>
      <div class="summary-card fx-card">
        <div class="summary-card-label">รายจ่าย</div>
        <div class="summary-card-value expense">฿{{ formatAmount(displayExpense) }}</div>
        <div v-if="expenseChangePct !== null" class="summary-card-sub">
          <b :class="expenseChangePct >= 0 ? 'expense' : 'income'"
            >{{ expenseChangePct >= 0 ? '↑' : '↓' }} {{ Math.abs(expenseChangePct).toFixed(0) }}%</b
          >
          จาก {{ previousMonthLabel }} (฿{{ formatAmount(prevExpense) }})
        </div>
      </div>
    </div>

    <!-- All-time totals — deliberately ignores the month/year picker above,
         so there's always a fixed lifetime reference point regardless of
         which month is currently being viewed. Still respects the member
         filter since that's an independent dimension from date. -->
    <div class="all-time-row">
      <span class="all-time-label">ทุกช่วงเวลา</span>
      <span
        >รายรับ
        <b class="income">฿{{ formatAmount(reportStore.allTimeSummary?.totalIncome ?? 0) }}</b></span
      >
      <span
        >รายจ่าย
        <b class="expense">฿{{ formatAmount(reportStore.allTimeSummary?.totalExpense ?? 0) }}</b></span
      >
      <span
        >คงเหลือ
        <b :class="(reportStore.allTimeSummary?.balance ?? 0) >= 0 ? 'income' : 'expense'"
          >฿{{ formatAmount(reportStore.allTimeSummary?.balance ?? 0) }}</b
        ></span
      >
    </div>

    <div class="charts-row">
      <!-- Yearly bar chart -->
      <el-card v-loading="reportStore.loading" class="chart-card" shadow="never" data-tour="dash-chart">
        <template #header>
          <div class="card-header-row">
            <span class="card-title">รายรับ-รายจ่าย 12 เดือน</span>
            <span class="chart-legend">
              <button class="legend-toggle" :style="{ opacity: showIncome ? 1 : 0.4 }" @click="toggleIncome">
                <i class="legend-dot income-dot"></i>รายรับ
              </button>
              <button class="legend-toggle" :style="{ opacity: showExpense ? 1 : 0.4 }" @click="toggleExpense">
                <i class="legend-dot expense-dot"></i>รายจ่าย
              </button>
            </span>
          </div>
        </template>
        <v-chart
          v-if="yearlyChartOption"
          ref="chartRef"
          :option="yearlyChartOption"
          style="height: 260px"
          autoresize
        />
        <el-empty v-else description="ไม่มีข้อมูล" :image-size="60" />
      </el-card>

      <!-- Category ranked list (replaces the old pie chart) -->
      <el-card v-loading="reportStore.loading" class="chart-card" shadow="never" data-tour="dash-category">
        <template #header>
          <div class="card-header-row">
            <span class="card-title">รายจ่ายตามหมวดหมู่</span>
            <el-button
              v-if="sortedCategories.length"
              text
              type="primary"
              @click="allCategoriesModalVisible = true"
            >
              ทั้งหมด {{ sortedCategories.length }} หมวด
            </el-button>
          </div>
        </template>
        <div v-if="topCategories.length" class="category-list">
          <div
            v-for="(c, idx) in topCategories"
            :key="c.categoryId"
            class="category-row"
            @click="handleCategoryRowClick(c)"
          >
            <div class="category-row-name">
              {{ c.icon }} {{ c.name }} <span class="category-row-count">· {{ c.count }} รายการ</span>
            </div>
            <div class="category-row-amount">฿{{ formatAmount(c.total) }}</div>
            <div class="category-row-track">
              <div
                class="category-row-fill"
                :style="{
                  width: categoriesMounted ? c.barWidth + '%' : '0%',
                  transitionDelay: idx * 50 + 'ms',
                }"
              />
            </div>
            <div class="category-row-pct">{{ c.percentage }}%</div>
          </div>
        </div>
        <el-empty v-else description="ไม่มีข้อมูล" :image-size="60" />
      </el-card>
    </div>

    <!-- Recent transactions -->
    <el-card shadow="never" style="margin-top: 16px">
      <template #header>
        <div class="card-header-row">
          <span class="card-title">รายการล่าสุด</span>
          <el-button text type="primary" @click="router.push('/pages/transactions')"
            >ดูทั้งหมด</el-button
          >
        </div>
      </template>
      <div class="table-scroll">
        <el-table
          v-loading="transactionStore.loading"
          :data="transactionStore.transactions.slice(0, 10)"
          size="small"
          style="min-width: 640px"
        >
          <el-table-column label="วันที่" width="120">
            <template #default="{ row }">{{ formatDate(row.transactionDate) }}</template>
          </el-table-column>
          <el-table-column label="หมวดหมู่">
            <template #default="{ row }">
              <TransactionDetailPopover :row="row" />
            </template>
          </el-table-column>
          <el-table-column label="หมายเหตุ" prop="note" min-width="150" class-name="note-cell" />
          <el-table-column label="สมาชิก" width="90">
            <template #default="{ row }">{{ row.memberName || '-' }}</template>
          </el-table-column>
          <el-table-column label="จำนวนเงิน" align="right" width="130">
            <template #default="{ row }">
              <span :class="row.type === 'Income' ? 'income' : 'expense'">
                {{ row.type === 'Income' ? '+' : '-' }}฿{{ formatAmount(row.amount) }}
              </span>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </el-card>

    <!-- Full category breakdown (opened from the "ทั้งหมด N หมวด" link above) -->
    <el-dialog v-model="allCategoriesModalVisible" title="รายจ่ายตามหมวดหมู่ (ทั้งหมด)" width="600px">
      <div class="table-scroll">
        <el-table
          :data="reportStore.categorySummary?.categories ?? []"
          size="small"
          style="min-width: 520px"
          class="clickable-rows"
          @row-click="handleCategoryRowClick"
        >
          <el-table-column label="หมวดหมู่">
            <template #default="{ row }">{{ row.icon }} {{ row.name }}</template>
          </el-table-column>
          <el-table-column label="จำนวนรายการ" prop="count" align="center" width="120" />
          <el-table-column label="สัดส่วน" width="160">
            <template #default="{ row }">
              <el-progress :percentage="row.percentage" :color="row.color" :stroke-width="8" />
            </template>
          </el-table-column>
          <el-table-column label="จำนวนเงิน" align="right" width="140">
            <template #default="{ row }">
              <span class="expense">฿{{ formatAmount(row.total) }}</span>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </el-dialog>

    <!-- Items within a single category -->
    <el-dialog
      v-model="categoryModalVisible"
      width="520px"
      top="120px"
      append-to-body
      :show-close="false"
      class="category-detail-dialog"
    >
      <template #header>
        <div class="cat-modal-header">
          <div class="cat-modal-heading">
            <div class="cat-modal-sub">{{ monthLabel }} · {{ memberName }}</div>
            <div class="cat-modal-title">
              {{ categoryModalCategory?.icon }} {{ categoryModalCategory?.name }}
            </div>
          </div>
          <button class="cat-modal-close" @click="categoryModalVisible = false">✕</button>
        </div>
      </template>
      <div class="cat-modal-stats">
        <div>
          <div class="cat-modal-stat-label">รวม</div>
          <div class="cat-modal-stat-value expense">
            ฿{{ formatAmount(categoryModalCategory?.total ?? 0) }}
          </div>
        </div>
        <div>
          <div class="cat-modal-stat-label">สัดส่วน</div>
          <div class="cat-modal-stat-value">{{ categoryModalCategory?.percentage ?? 0 }}%</div>
        </div>
        <div>
          <div class="cat-modal-stat-label">จำนวน</div>
          <div class="cat-modal-stat-value">{{ categoryModalCategory?.count ?? 0 }} รายการ</div>
        </div>
      </div>
      <div v-loading="categoryModalLoading" class="cat-modal-list">
        <div
          v-for="(item, idx) in categoryModalTransactions"
          :key="item.id"
          class="cat-modal-row"
          :style="{ '--i': idx }"
        >
          <span class="cat-modal-row-date">{{ formatDate(item.transactionDate) }}</span>
          <span>{{ item.note || '-' }}</span>
          <span>{{ item.memberName || '-' }}</span>
          <b class="expense">฿{{ formatAmount(item.amount) }}</b>
        </div>
        <div class="cat-modal-footer">
          <a class="cat-modal-link" @click="openInTransactions">เปิดในหน้ารายการ →</a>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { formatThaiDate as formatDate } from '@/utils/formatters';
import { ref, computed, watch, onMounted, onBeforeUnmount, nextTick } from 'vue';
import { useRouter } from 'vue-router';
import { Plus, Download } from '@element-plus/icons-vue';
import { use } from 'echarts/core';
import { BarChart, CustomChart } from 'echarts/charts';
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components';
import { CanvasRenderer } from 'echarts/renderers';
import VChart from 'vue-echarts';
import PageHeader from '@/components/admin/PageHeader.vue';
import TransactionDetailPopover from '@/components/budget/TransactionDetailPopover.vue';
import { useReportStore } from '@/stores/reportStore';
import { useTransactionStore } from '@/stores/transactionStore';
import { useMemberStore } from '@/stores/memberStore';
import { reportService } from '@/services/reportService';
import { transactionService } from '@/services/transactionService';
import { useNotify } from '@/composables/useNotify';
import { useAnimatedNumber } from '@/composables/useAnimatedNumber';
import type { CategoryBreakdown, Transaction } from '@/types';

// LegendComponent is registered (but never rendered — legend.show stays
// false) purely so ECharts accepts legend.selected, which is how the
// custom header buttons toggle each series' visibility.
use([BarChart, CustomChart, GridComponent, TooltipComponent, LegendComponent, CanvasRenderer]);

const router = useRouter();
const reportStore = useReportStore();
const transactionStore = useTransactionStore();
const memberStore = useMemberStore();
const { toast } = useNotify();

const now = new Date();
const currentMonth = ref(now.getMonth() + 1);
const currentYear = ref(now.getFullYear());
const selectedMemberId = ref<number | undefined>(undefined);
const exporting = ref(false);

const categoryModalVisible = ref(false);
const categoryModalCategory = ref<CategoryBreakdown | null>(null);
const categoryModalTransactions = ref<Transaction[]>([]);
const categoryModalLoading = ref(false);
const allCategoriesModalVisible = ref(false);

async function handleCategoryRowClick(row: CategoryBreakdown) {
  categoryModalCategory.value = row;
  categoryModalVisible.value = true;
  categoryModalLoading.value = true;
  try {
    const { data } = await transactionService.getList({
      month: currentMonth.value,
      year: currentYear.value,
      categoryId: row.categoryId,
      memberId: selectedMemberId.value,
      pageSize: 100,
    });
    categoryModalTransactions.value = data?.isSuccess && data.value ? data.value.items : [];
  } finally {
    categoryModalLoading.value = false;
  }
}

function openInTransactions() {
  const query: Record<string, string> = {
    categoryId: String(categoryModalCategory.value?.categoryId ?? ''),
    month: String(currentMonth.value),
    year: String(currentYear.value),
  };
  if (selectedMemberId.value) query.memberId = String(selectedMemberId.value);
  router.push({ path: '/pages/transactions', query });
}

const thaiMonths = [
  'ม.ค.',
  'ก.พ.',
  'มี.ค.',
  'เม.ย.',
  'พ.ค.',
  'มิ.ย.',
  'ก.ค.',
  'ส.ค.',
  'ก.ย.',
  'ต.ค.',
  'พ.ย.',
  'ธ.ค.',
];
const thaiFullMonths = [
  'มกราคม',
  'กุมภาพันธ์',
  'มีนาคม',
  'เมษายน',
  'พฤษภาคม',
  'มิถุนายน',
  'กรกฎาคม',
  'สิงหาคม',
  'กันยายน',
  'ตุลาคม',
  'พฤศจิกายน',
  'ธันวาคม',
];

const monthLabel = computed(() => `${thaiMonths[currentMonth.value - 1]} ${currentYear.value + 543}`);
const previousMonthLabel = computed(() => {
  const m = currentMonth.value === 1 ? 12 : currentMonth.value - 1;
  return thaiMonths[m - 1];
});

const selectedDate = computed<string>({
  get: () => `${currentYear.value}-${String(currentMonth.value).padStart(2, '0')}`,
  set: (val) => {
    if (!val) return;
    const [y, m] = val.split('-').map(Number);
    currentYear.value = y;
    currentMonth.value = m;
    loadData();
  },
});

function isFutureMonth(date: Date) {
  const y = date.getFullYear();
  const m = date.getMonth() + 1;
  return y > now.getFullYear() || (y === now.getFullYear() && m > now.getMonth() + 1);
}

const ALL_MEMBERS = 0;
const memberSegmentOptions = computed(() => [
  { label: 'ทุกคน', value: ALL_MEMBERS },
  ...memberStore.activeMembers.map((m) => ({ label: m.name, value: m.id })),
]);
const memberName = computed(
  () => memberStore.activeMembers.find((m) => m.id === selectedMemberId.value)?.name ?? 'ทุกคน'
);
const segmentValue = computed<number>({
  get: () => selectedMemberId.value ?? ALL_MEMBERS,
  set: (v) => selectMember(v === ALL_MEMBERS ? undefined : v),
});

function selectMember(id: number | undefined) {
  selectedMemberId.value = id;
  loadData();
}

const currentIncome = computed(() => reportStore.monthlySummary?.totalIncome ?? 0);
const currentExpense = computed(() => reportStore.monthlySummary?.totalExpense ?? 0);
const currentBalance = computed(() => reportStore.monthlySummary?.balance ?? 0);
const prevIncome = computed(() => reportStore.previousMonthSummary?.totalIncome ?? 0);
const prevExpense = computed(() => reportStore.previousMonthSummary?.totalExpense ?? 0);
const prevBalance = computed(() => reportStore.previousMonthSummary?.balance ?? 0);

const displayBalance = useAnimatedNumber(currentBalance);
const displayIncome = useAnimatedNumber(currentIncome);
const displayExpense = useAnimatedNumber(currentExpense);

const balanceDescription = computed(() =>
  currentBalance.value >= 0 ? 'รายรับมากกว่ารายจ่าย' : 'รายจ่ายเกินรายรับ'
);

function pctChange(current: number, previous: number): number | null {
  if (!previous) return null;
  return ((current - previous) / previous) * 100;
}

const incomeChangePct = computed(() => pctChange(currentIncome.value, prevIncome.value));
const expenseChangePct = computed(() => pctChange(currentExpense.value, prevExpense.value));

const sortedCategories = computed(() => {
  const cats = reportStore.categorySummary?.categories;
  if (!cats?.length) return [];
  return [...cats].sort((a, b) => b.total - a.total);
});

// Progress-bar widths are relative to the biggest category, not the overall
// total — matches the approved design (the top category always fills 100%).
const topCategories = computed(() => {
  const sorted = sortedCategories.value;
  const max = sorted[0]?.total || 1;
  return sorted.slice(0, 6).map((c) => ({ ...c, barWidth: (c.total / max) * 100 }));
});

// Bars start at 0 width and grow in on every data change (not just first
// mount) — toggling this flag off-then-on re-triggers the CSS transition.
const categoriesMounted = ref(false);
watch(
  () => reportStore.categorySummary,
  () => {
    categoriesMounted.value = false;
    nextTick(() => {
      categoriesMounted.value = true;
    });
  }
);

const showIncome = ref(true);
const showExpense = ref(true);
function toggleIncome() {
  if (showIncome.value && !showExpense.value) return; // can't hide the last visible series
  showIncome.value = !showIncome.value;
}
function toggleExpense() {
  if (showExpense.value && !showIncome.value) return;
  showExpense.value = !showExpense.value;
}

function cssVar(name: string, fallback: string) {
  if (typeof window === 'undefined') return fallback;
  const v = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return v || fallback;
}

const yearlyChartOption = computed(() => {
  const data = reportStore.yearlySummary?.months;
  if (!data?.length) return null;
  const successColor = cssVar('--success', '#16a34a');
  const dangerColor = cssVar('--danger', '#dc2626');
  const primaryColor = cssVar('--primary', '#0891b2');
  const selectedIdx = currentMonth.value - 1;
  const realCurrentIdx =
    currentYear.value === now.getFullYear() ? now.getMonth() : 11;

  return {
    animationDuration: 700,
    animationEasing: 'cubicOut',
    animationDelay: (idx: number) => idx * 45,
    animationDurationUpdate: 600,
    tooltip: {
      trigger: 'axis',
      backgroundColor: 'transparent',
      borderWidth: 0,
      padding: 0,
      // Without this, the floating tooltip div (appended near the cursor on
      // hover) can sit on top of the canvas and silently swallow the next
      // click if it lands where the tooltip just appeared — which is exactly
      // what made rapid clicks on different months intermittently "miss".
      extraCssText: 'box-shadow:none;pointer-events:none;',
      formatter: (params: { dataIndex: number; seriesType: string }[]) => {
        const bar = params.find((p) => p.seriesType === 'bar');
        if (!bar) return '';
        const idx = bar.dataIndex;
        const inc = data[idx]?.income ?? 0;
        const exp = data[idx]?.expense ?? 0;
        const net = inc - exp;
        const label = `${thaiFullMonths[idx]} ${currentYear.value + 543}`;
        const fmt = (v: number) => `฿${Math.round(v).toLocaleString()}`;
        return `<div style="background:#18181b;color:#fff;border-radius:10px;padding:10px 12px;font-size:12px;box-shadow:0 10px 24px rgba(0,0,0,.25);min-width:150px">
          <b style="font-size:12.5px">${label}</b>
          <div style="display:flex;justify-content:space-between;gap:14px;margin-top:6px"><span style="color:#86efac">รายรับ</span><span>${fmt(inc)}</span></div>
          <div style="display:flex;justify-content:space-between;gap:14px"><span style="color:#fca5a5">รายจ่าย</span><span>${fmt(exp)}</span></div>
          <div style="display:flex;justify-content:space-between;gap:14px;border-top:1px solid rgba(255,255,255,.15);margin-top:4px;padding-top:4px"><span style="color:#a1a1aa">คงเหลือ</span><b>${net < 0 ? '−' : '+'}${fmt(Math.abs(net))}</b></div>
        </div>`;
      },
    },
    xAxis: {
      type: 'category',
      data: thaiMonths,
      axisLabel: {
        // Force every month to render — ECharts' default 'auto' interval
        // hides labels it thinks will overlap, and its width estimate for
        // rich-text tokens (used below for the bold/colored selected label)
        // runs conservative enough to skip down to only every 3rd month.
        interval: 0,
        formatter: (value: string, idx: number) =>
          idx === selectedIdx ? `{sel|${value}}` : idx > realCurrentIdx ? `{future|${value}}` : `{normal|${value}}`,
        rich: {
          sel: { color: primaryColor, fontWeight: 700 },
          normal: { color: '#6c757d' },
          future: { color: '#c4c9cf' },
        },
      },
    },
    yAxis: { type: 'value', axisLabel: { formatter: (v: number) => `฿${v.toLocaleString()}` } },
    series: [
      {
        name: 'รายรับ',
        type: 'bar',
        data: data.map((m) => m.income),
        color: successColor,
        barMaxWidth: 30,
        legendHoverLink: false,
      },
      {
        name: 'รายจ่าย',
        type: 'bar',
        data: data.map((m) => m.expense),
        color: dangerColor,
        barMaxWidth: 30,
      },
      {
        // markArea's fractional category coordinates render unreliably, so the
        // selected-month band is drawn from the axis' real pixel geometry.
        name: '__selected-month',
        type: 'custom',
        silent: true,
        z: 0,
        tooltip: { show: false },
        data: [[selectedIdx, 0]],
        renderItem: (
          params: { coordSys: { x: number; y: number; width: number; height: number } },
          api: { coord: (v: number[]) => number[]; size: (v: number[]) => number[] },
        ) => {
          const bandWidth = api.size([1, 0])[0];
          const centerX = api.coord([selectedIdx, 0])[0];
          const { y, height } = params.coordSys;
          return {
            type: 'rect',
            shape: { x: centerX - bandWidth / 2, y, width: bandWidth, height },
            style: { fill: 'rgba(8,145,178,.09)' },
          };
        },
      },
    ],
    legend: { show: false, selected: { รายรับ: showIncome.value, รายจ่าย: showExpense.value } },
  };
});

// vue-echarts only fires its `click` event when the pointer lands on an
// actual rendered bar element — for a low-value month that's a tiny sliver,
// so clicking anywhere else in that month's column (as the original design
// allows) silently did nothing. Converting the raw pixel position through
// ECharts' own axis mapping instead makes the whole column clickable.
const chartRef = ref<InstanceType<typeof VChart> | null>(null);
// reportStore's fetch functions write their result into shared state as soon
// as each one resolves, with no awareness of which request was the latest —
// so clicking several months in quick succession can let a slower, older
// response land after a newer one and leave the UI showing stale data. This
// counter detects that: if another click came in while this one was still
// loading, it re-runs once more so the final state always converges on
// whatever month was clicked last, rather than whichever response arrived last.
let chartRequestId = 0;

async function selectMonthFromChart(monthIndex: number) {
  const clickedMonth = monthIndex + 1;
  const isFuture = currentYear.value === now.getFullYear() && clickedMonth > now.getMonth() + 1;
  if (isFuture || clickedMonth === currentMonth.value) return;
  currentMonth.value = clickedMonth;
  const myRequest = ++chartRequestId;
  await loadData();
  if (myRequest !== chartRequestId) {
    await loadData();
  }
}

function handleChartNativeClick(e: MouseEvent) {
  const chart = chartRef.value as unknown as {
    convertFromPixel: (finder: { gridIndex: number }, pixel: number[]) => number[] | null;
    root?: HTMLElement;
  } | null;
  const container = chart?.root ?? (e.currentTarget as HTMLElement);
  if (!chart || !container) return;
  const rect = container.getBoundingClientRect();
  const x = e.clientX - rect.left;
  const y = e.clientY - rect.top;
  const result = chart.convertFromPixel({ gridIndex: 0 }, [x, y]);
  const idx = result?.[0];
  if (idx == null || Number.isNaN(idx)) return;
  const rounded = Math.round(idx);
  if (rounded < 0 || rounded > 11) return;
  selectMonthFromChart(rounded);
}

function formatAmount(val?: number | null) {
  return (val ?? 0).toLocaleString('th-TH', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

async function loadData() {
  await Promise.all([
    reportStore.fetchAll(currentMonth.value, currentYear.value, selectedMemberId.value),
    reportStore.fetchAllTimeSummary(selectedMemberId.value),
    reportStore.fetchPreviousMonthSummary(currentMonth.value, currentYear.value, selectedMemberId.value),
    transactionStore.fetchTransactions({
      month: currentMonth.value,
      year: currentYear.value,
      pageSize: 10,
      memberId: selectedMemberId.value,
    }),
  ]);
}

async function handleExport() {
  exporting.value = true;
  try {
    const { data } = await reportService.exportMonthly(currentMonth.value, currentYear.value);
    const url = URL.createObjectURL(data);
    const a = document.createElement('a');
    a.href = url;
    a.download = `report-${currentYear.value}-${String(currentMonth.value).padStart(2, '0')}.xlsx`;
    a.click();
    URL.revokeObjectURL(url);
  } catch {
    toast('ไม่สามารถ export ได้', 'error');
  } finally {
    exporting.value = false;
  }
}

// The chart's DOM root only exists while `v-if="yearlyChartOption"` is true,
// so the listener is (re-)attached whenever chartRef appears and detached
// whenever it disappears, instead of assuming a single mount/unmount.
let chartRootWithListener: HTMLElement | null = null;
watch(chartRef, (instance) => {
  if (chartRootWithListener) {
    chartRootWithListener.removeEventListener('click', handleChartNativeClick);
    chartRootWithListener = null;
  }
  const root = (instance as unknown as { root?: HTMLElement } | null)?.root;
  if (root) {
    root.addEventListener('click', handleChartNativeClick);
    chartRootWithListener = root;
  }
});

onBeforeUnmount(() => {
  chartRootWithListener?.removeEventListener('click', handleChartNativeClick);
});

onMounted(() => {
  memberStore.fetchMembers();
  loadData();
});
</script>

<style scoped>
.filter-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

:deep(.member-segmented.el-segmented) {
  --el-segmented-bg-color: var(--surface);
  --el-segmented-item-selected-bg-color: #0891b2;
  --el-segmented-item-selected-color: #fff;
  --el-segmented-color: #52525b;
  border: 1px solid var(--border);
  border-radius: 8px;
  font-size: 12.5px;
  font-weight: 600;
}

.summary-grid-3 {
  display: grid;
  grid-template-columns: 1.25fr 1fr 1fr;
  gap: 16px;
  margin-bottom: 16px;
}
.summary-card {
  position: relative;
  overflow: hidden;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 14px;
  padding: 22px 24px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.fx-card {
  transition:
    transform 0.2s,
    box-shadow 0.2s;
}
.fx-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 24px rgba(16, 24, 40, 0.08);
}
.summary-card-accent {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  transition: background 0.4s;
}
.summary-card-label {
  font-size: 14px;
  font-weight: 600;
  color: #52525b;
}
.summary-card-value {
  font-size: 30px;
  font-weight: 800;
  line-height: 1.15;
  font-variant-numeric: tabular-nums;
  transition: color 0.4s;
}
.summary-card.wide .summary-card-value {
  font-size: 40px;
  letter-spacing: -0.02em;
  line-height: 1.1;
}
.summary-card-sub {
  font-size: 13.5px;
  color: #52525b;
}
@media (max-width: 900px) {
  .summary-grid-3 {
    grid-template-columns: 1fr;
  }
}

.all-time-row {
  display: flex;
  flex-wrap: wrap;
  gap: 28px;
  align-items: center;
  font-size: 13.5px;
  color: var(--text-muted);
  padding: 0 4px;
  margin-bottom: 16px;
}
.all-time-label {
  font-weight: 700;
  color: var(--text);
}

.table-scroll {
  overflow-x: auto;
}

:deep(.note-cell .cell) {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* Also used directly by the category-breakdown widgets below (ranked list +
   the "all categories" modal table) since scoped styles don't cross
   component boundaries. */
.income {
  color: var(--success);
}
.expense {
  color: var(--danger);
}

.charts-row {
  display: grid;
  /* Each chart card needs real room — auto-fit drops to a single column once
     a column would go under 460px, regardless of viewport width or whether
     the sidebar is expanded/collapsed, instead of guessing one fixed
     breakpoint. */
  grid-template-columns: repeat(auto-fit, minmax(460px, 1fr));
  gap: 16px;
  margin-bottom: 16px;
}

.card-title {
  font-weight: 600;
}
.card-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.chart-legend {
  display: flex;
  gap: 6px;
  font-size: 12.5px;
  color: var(--text-muted);
}
.legend-toggle {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px 8px;
  border-radius: 6px;
  border: none;
  background: none;
  font: inherit;
  font-size: 12.5px;
  color: #52525b;
  cursor: pointer;
  transition:
    opacity 0.2s,
    background 0.15s;
}
.legend-toggle:hover {
  background: #f1f5f7;
}
.legend-dot {
  width: 10px;
  height: 10px;
  border-radius: 3px;
  display: inline-block;
}
.income-dot {
  background: var(--success);
}
.expense-dot {
  background: var(--danger);
}

.category-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.category-row {
  display: grid;
  grid-template-columns: 1fr 92px;
  gap: 6px 12px;
  padding: 7px 8px;
  margin: 0 -8px;
  border-radius: 8px;
  cursor: pointer;
  transition:
    background 0.15s,
    transform 0.1s;
}
.category-row:hover {
  background: #f0f9fb;
}
.category-row:active {
  transform: scale(0.99);
}
.category-row-name {
  font-size: 13.5px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.category-row-count {
  color: var(--text-muted);
  font-size: 12px;
}
.category-row-amount {
  font-size: 13.5px;
  font-weight: 700;
  text-align: right;
  font-variant-numeric: tabular-nums;
}
.category-row-track {
  height: 6px;
  border-radius: 999px;
  background: #eef0f2;
  overflow: hidden;
}
.category-row-fill {
  height: 100%;
  width: 0%;
  background: #0891b2;
  border-radius: 999px;
  transition: width 0.6s cubic-bezier(0.2, 0.8, 0.2, 1);
}
.category-row-pct {
  font-size: 12px;
  color: var(--text-muted);
  text-align: right;
  line-height: 6px;
}

:deep(.clickable-rows .el-table__row) {
  cursor: pointer;
}

/* Category-detail modal (custom header/body — dialog itself has no default
   padding removed here since #header replaces el-dialog's own title area) */
.cat-modal-header {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  width: 100%;
}
.cat-modal-heading {
  flex: 1;
  min-width: 0;
}
.cat-modal-sub {
  font-size: 12.5px;
  color: var(--text-muted);
}
.cat-modal-title {
  font-size: 17px;
  font-weight: 800;
  margin-top: 2px;
}
.cat-modal-close {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: none;
  background: #f1f3f5;
  color: var(--text-muted);
  cursor: pointer;
  font-size: 14px;
  transition: background 0.15s;
}
.cat-modal-close:hover {
  background: #e9ecef;
}
.cat-modal-stats {
  display: flex;
  gap: 24px;
  margin: 0 -16px 12px;
  padding: 14px 16px;
  background: #fafbfc;
  border-top: 1px solid #f2f3f5;
  border-bottom: 1px solid #f2f3f5;
}
.cat-modal-stat-label {
  font-size: 12px;
  color: var(--text-muted);
}
.cat-modal-stat-value {
  font-size: 22px;
  font-weight: 800;
}
.cat-modal-list {
  max-height: 420px;
  overflow-y: auto;
}
.cat-modal-row {
  display: grid;
  grid-template-columns: 90px 1fr 60px 100px;
  padding: 10px 8px;
  font-size: 13.5px;
  border-bottom: 1px solid #f2f3f5;
  border-radius: 6px;
  animation: cat-row-in 0.3s ease-out both;
  animation-delay: calc(var(--i) * 55ms + 80ms);
  transition: background 0.15s;
}
.cat-modal-row:hover {
  background: #f6fbfc;
}
.cat-modal-row-date {
  color: var(--text-muted);
}
.cat-modal-footer {
  display: flex;
  justify-content: flex-end;
  padding-top: 12px;
}
.cat-modal-link {
  font-size: 13px;
  font-weight: 600;
  color: var(--primary);
  cursor: pointer;
}
.cat-modal-link:hover {
  text-decoration: underline;
}

@keyframes cat-row-in {
  from {
    opacity: 0;
    transform: translateY(6px);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
</style>
