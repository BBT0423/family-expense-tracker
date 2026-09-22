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

    <!-- Month/Year selector -->
    <div class="filter-bar">
      <el-date-picker
        v-model="selectedDate"
        type="month"
        format="MMM BBBB"
        value-format="YYYY-MM"
        placeholder="เลือกเดือน"
        style="width: 160px"
        @change="onDateChange"
      />
      <ScopeSelector v-model="scopeUserId" @update:model-value="loadData" />
    </div>

    <!-- Summary cards -->
    <div class="summary-grid">
      <el-card class="summary-card income-card" shadow="never">
        <div class="card-label">รายรับ</div>
        <div class="card-amount income">
          ฿{{ formatAmount(reportStore.monthlySummary?.totalIncome) }}
        </div>
      </el-card>
      <el-card class="summary-card expense-card" shadow="never">
        <div class="card-label">รายจ่าย</div>
        <div class="card-amount expense">
          ฿{{ formatAmount(reportStore.monthlySummary?.totalExpense) }}
        </div>
      </el-card>
      <el-card class="summary-card balance-card" shadow="never">
        <div class="card-label">คงเหลือ</div>
        <div
          class="card-amount"
          :class="(reportStore.monthlySummary?.balance ?? 0) >= 0 ? 'income' : 'expense'"
        >
          ฿{{ formatAmount(reportStore.monthlySummary?.balance) }}
        </div>
      </el-card>
    </div>

    <el-tabs v-model="activeTab">
      <!-- Overview: charts + recent transactions -->
      <el-tab-pane label="ภาพรวม" name="overview">
        <div class="charts-row">
          <!-- Yearly bar chart -->
          <el-card class="chart-card" shadow="never">
            <template #header><span class="card-title">รายรับ-รายจ่าย 12 เดือน</span></template>
            <v-chart
              v-if="yearlyChartOption"
              :option="yearlyChartOption"
              style="height: 260px"
              autoresize
            />
            <el-empty v-else description="ไม่มีข้อมูล" :image-size="60" />
          </el-card>

          <!-- Category pie chart -->
          <el-card class="chart-card" shadow="never">
            <template #header><span class="card-title">รายจ่ายตามหมวดหมู่</span></template>
            <v-chart
              v-if="pieChartOption"
              :option="pieChartOption"
              style="height: 260px"
              autoresize
            />
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
                  <span>{{ row.categoryIcon }} {{ row.categoryName }}</span>
                  <span v-if="row.ownerName" class="owner-tag">({{ row.ownerName }})</span>
                </template>
              </el-table-column>
              <el-table-column label="หมายเหตุ" prop="note" show-overflow-tooltip />
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
      </el-tab-pane>

      <!-- Details: category breakdown table -->
      <el-tab-pane label="รายละเอียด" name="details">
        <el-card shadow="never">
          <template #header><span class="card-title">รายจ่ายตามหมวดหมู่</span></template>
          <div class="table-scroll">
            <el-table
              :data="reportStore.categorySummary?.categories ?? []"
              size="small"
              style="min-width: 520px"
            >
              <el-table-column label="หมวดหมู่">
                <template #default="{ row }">{{ row.icon }} {{ row.name }}</template>
              </el-table-column>
              <el-table-column label="จำนวนรายการ" prop="count" align="center" width="120" />
              <el-table-column label="สัดส่วน" align="center" width="100">
                <template #default="{ row }">{{ row.percentage }}%</template>
              </el-table-column>
              <el-table-column label="จำนวนเงิน" align="right" width="140">
                <template #default="{ row }">
                  <span class="expense">฿{{ formatAmount(row.total) }}</span>
                </template>
              </el-table-column>
            </el-table>
          </div>
        </el-card>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup lang="ts">
import { formatThaiDate as formatDate } from '@/utils/formatters';
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { Plus, Download } from '@element-plus/icons-vue';
import { use } from 'echarts/core';
import { BarChart, PieChart } from 'echarts/charts';
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components';
import { CanvasRenderer } from 'echarts/renderers';
import VChart from 'vue-echarts';
import PageHeader from '@/components/admin/PageHeader.vue';
import ScopeSelector from '@/components/common/ScopeSelector.vue';
import { useReportStore } from '@/stores/reportStore';
import { useTransactionStore } from '@/stores/transactionStore';
import { reportService } from '@/services/reportService';
import { useNotify } from '@/composables/useNotify';

use([BarChart, PieChart, GridComponent, TooltipComponent, LegendComponent, CanvasRenderer]);

const router = useRouter();
const reportStore = useReportStore();
const transactionStore = useTransactionStore();
const { toast } = useNotify();

const activeTab = ref<'overview' | 'details'>('overview');

const now = new Date();
const selectedDate = ref(`${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`);
const currentMonth = ref(now.getMonth() + 1);
const currentYear = ref(now.getFullYear());
const scopeUserId = ref<string | undefined>(undefined);
const exporting = ref(false);

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

const yearlyChartOption = computed(() => {
  const data = reportStore.yearlySummary?.months;
  if (!data?.length) return null;
  return {
    tooltip: { trigger: 'axis' },
    legend: { data: ['รายรับ', 'รายจ่าย'] },
    xAxis: { type: 'category', data: thaiMonths },
    yAxis: { type: 'value', axisLabel: { formatter: (v: number) => `฿${v.toLocaleString()}` } },
    series: [
      {
        name: 'รายรับ',
        type: 'bar',
        data: data.map((m) => m.income),
        color: '#27ae60',
        barMaxWidth: 30,
      },
      {
        name: 'รายจ่าย',
        type: 'bar',
        data: data.map((m) => m.expense),
        color: '#e74c3c',
        barMaxWidth: 30,
      },
    ],
  };
});

const pieChartOption = computed(() => {
  const cats = reportStore.categorySummary?.categories;
  if (!cats?.length) return null;
  return {
    tooltip: { trigger: 'item', formatter: '{b}: ฿{c} ({d}%)' },
    legend: { orient: 'vertical', right: 10, top: 'center' },
    series: [
      {
        type: 'pie',
        // Shifted left + slightly smaller so the donut never extends under the
        // vertical right-side legend, regardless of card width (see DashboardView.vue
        // plan notes for the geometry this is based on).
        center: ['35%', '50%'],
        radius: ['38%', '65%'],
        data: cats.map((c) => ({
          name: `${c.icon} ${c.name}`,
          value: c.total,
          itemStyle: { color: c.color },
        })),
        label: { show: false },
      },
    ],
  };
});

function formatAmount(val?: number | null) {
  return (val ?? 0).toLocaleString('th-TH', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

async function onDateChange(val: string) {
  const [y, m] = val.split('-').map(Number);
  currentYear.value = y;
  currentMonth.value = m;
  await loadData();
}

async function loadData() {
  await Promise.all([
    reportStore.fetchAll(currentMonth.value, currentYear.value, scopeUserId.value),
    transactionStore.fetchTransactions({
      month: currentMonth.value,
      year: currentYear.value,
      pageSize: 10,
      userId: scopeUserId.value,
    }),
  ]);
}

async function handleExport() {
  exporting.value = true;
  try {
    const { data } = await reportService.exportMonthly(
      currentMonth.value,
      currentYear.value,
      scopeUserId.value
    );
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

onMounted(loadData);
</script>

<style scoped>
.filter-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.table-scroll {
  overflow-x: auto;
}

.owner-tag {
  color: var(--text-muted, #888);
  font-size: 0.85em;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-bottom: 16px;
}

.summary-card {
  text-align: center;
  padding: 8px 0;
}
.card-label {
  font-size: 0.85rem;
  color: var(--text-light, #888);
  margin-bottom: 6px;
}
.card-amount {
  font-size: 1.6rem;
  font-weight: 700;
}

.income {
  color: #27ae60;
}
.expense {
  color: #e74c3c;
}

.charts-row {
  display: grid;
  /* Each chart (donut + legend) needs real room — auto-fit drops to a single
     column once a column would go under 460px, regardless of viewport width
     or whether the sidebar is expanded/collapsed, instead of guessing one
     fixed breakpoint. */
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

@media (max-width: 768px) {
  .summary-grid {
    grid-template-columns: 1fr;
  }
}
</style>
