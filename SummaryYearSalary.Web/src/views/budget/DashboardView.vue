<template>
  <div>
    <PageHeader>
      <template #actions-left>
        <el-button type="primary" :icon="Plus" @click="router.push('/pages/transactions/add')">
          เพิ่มรายการ
        </el-button>
      </template>
    </PageHeader>

    <!-- Month/Year selector -->
    <div class="filter-bar">
      <el-date-picker
        v-model="selectedDate"
        type="month"
        format="MM/YYYY"
        value-format="YYYY-MM"
        placeholder="เลือกเดือน"
        style="width: 160px"
        @change="onDateChange"
      />
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

    <!-- Charts row -->
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
        <v-chart v-if="pieChartOption" :option="pieChartOption" style="height: 260px" autoresize />
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
      <el-table
        v-loading="transactionStore.loading"
        :data="transactionStore.transactions.slice(0, 10)"
        size="small"
      >
        <el-table-column label="วันที่" width="100">
          <template #default="{ row }">{{ formatDate(row.transactionDate) }}</template>
        </el-table-column>
        <el-table-column label="หมวดหมู่">
          <template #default="{ row }">
            <span>{{ row.categoryIcon }} {{ row.categoryName }}</span>
          </template>
        </el-table-column>
        <el-table-column label="สมาชิก" prop="memberName" />
        <el-table-column label="หมายเหตุ" prop="note" show-overflow-tooltip />
        <el-table-column label="จำนวนเงิน" align="right" width="130">
          <template #default="{ row }">
            <span :class="row.type === 'Income' ? 'income' : 'expense'">
              {{ row.type === 'Income' ? '+' : '-' }}฿{{ formatAmount(row.amount) }}
            </span>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { Plus } from '@element-plus/icons-vue';
import { use } from 'echarts/core';
import { BarChart, PieChart } from 'echarts/charts';
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components';
import { CanvasRenderer } from 'echarts/renderers';
import VChart from 'vue-echarts';
import PageHeader from '@/components/admin/PageHeader.vue';
import { useReportStore } from '@/stores/reportStore';
import { useTransactionStore } from '@/stores/transactionStore';

use([BarChart, PieChart, GridComponent, TooltipComponent, LegendComponent, CanvasRenderer]);

const router = useRouter();
const reportStore = useReportStore();
const transactionStore = useTransactionStore();

const now = new Date();
const selectedDate = ref(`${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`);
const currentMonth = ref(now.getMonth() + 1);
const currentYear = ref(now.getFullYear());

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
        radius: ['40%', '70%'],
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

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('th-TH', {
    day: '2-digit',
    month: '2-digit',
    year: '2-digit',
  });
}

async function onDateChange(val: string) {
  const [y, m] = val.split('-').map(Number);
  currentYear.value = y;
  currentMonth.value = m;
  await loadData();
}

async function loadData() {
  await Promise.all([
    reportStore.fetchAll(currentMonth.value, currentYear.value),
    transactionStore.fetchTransactions({
      month: currentMonth.value,
      year: currentYear.value,
      pageSize: 10,
    }),
  ]);
}

onMounted(loadData);
</script>

<style scoped>
.filter-bar {
  margin-bottom: 16px;
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
  grid-template-columns: 1fr 1fr;
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
  .charts-row {
    grid-template-columns: 1fr;
  }
}
</style>
