<template>
  <div>
    <PageHeader>
      <template #actions-left>
        <el-button :icon="Download" :loading="exporting" @click="handleExport"
          >Export Excel</el-button
        >
      </template>
    </PageHeader>

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
      <el-card class="summary-card" shadow="never">
        <div class="card-label">รายรับ</div>
        <div class="card-amount income">฿{{ fmt(reportStore.monthlySummary?.totalIncome) }}</div>
      </el-card>
      <el-card class="summary-card" shadow="never">
        <div class="card-label">รายจ่าย</div>
        <div class="card-amount expense">฿{{ fmt(reportStore.monthlySummary?.totalExpense) }}</div>
      </el-card>
      <el-card class="summary-card" shadow="never">
        <div class="card-label">คงเหลือ</div>
        <div
          class="card-amount"
          :class="(reportStore.monthlySummary?.balance ?? 0) >= 0 ? 'income' : 'expense'"
        >
          ฿{{ fmt(reportStore.monthlySummary?.balance) }}
        </div>
      </el-card>
    </div>

    <!-- Charts row -->
    <div class="charts-row">
      <el-card shadow="never">
        <template #header><span class="card-title">รายรับ-รายจ่าย 12 เดือน</span></template>
        <v-chart v-if="yearlyOption" :option="yearlyOption" style="height: 260px" autoresize />
        <el-empty v-else description="ไม่มีข้อมูล" :image-size="60" />
      </el-card>
      <el-card shadow="never">
        <template #header><span class="card-title">รายจ่ายตามหมวดหมู่</span></template>
        <v-chart v-if="pieOption" :option="pieOption" style="height: 260px" autoresize />
        <el-empty v-else description="ไม่มีข้อมูล" :image-size="60" />
      </el-card>
    </div>

    <!-- Member breakdown -->
    <el-card shadow="never" style="margin-top: 16px">
      <template #header><span class="card-title">รายจ่ายแต่ละสมาชิก</span></template>
      <div v-for="m in reportStore.memberSummary?.members" :key="m.memberId" class="member-row">
        <div class="member-info">
          <span class="member-dot" :style="{ background: m.color }" />
          <span>{{ m.name }}</span>
        </div>
        <el-progress
          :percentage="m.percentage"
          :stroke-width="10"
          :color="m.color"
          style="flex: 1; margin: 0 16px"
        />
        <span class="expense">฿{{ fmt(m.totalExpense) }}</span>
      </div>
      <el-empty
        v-if="!reportStore.memberSummary?.members?.length"
        description="ไม่มีข้อมูล"
        :image-size="60"
      />
    </el-card>

    <!-- Category breakdown table -->
    <el-card shadow="never" style="margin-top: 16px">
      <template #header><span class="card-title">รายจ่ายตามหมวดหมู่</span></template>
      <el-table :data="reportStore.categorySummary?.categories ?? []" size="small">
        <el-table-column label="หมวดหมู่">
          <template #default="{ row }">{{ row.icon }} {{ row.name }}</template>
        </el-table-column>
        <el-table-column label="จำนวนรายการ" prop="count" align="center" width="120" />
        <el-table-column label="สัดส่วน" align="center" width="100">
          <template #default="{ row }">{{ row.percentage }}%</template>
        </el-table-column>
        <el-table-column label="จำนวนเงิน" align="right" width="140">
          <template #default="{ row }">
            <span class="expense">฿{{ fmt(row.total) }}</span>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { Download } from '@element-plus/icons-vue';
import { use } from 'echarts/core';
import { BarChart, PieChart } from 'echarts/charts';
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components';
import { CanvasRenderer } from 'echarts/renderers';
import VChart from 'vue-echarts';
import PageHeader from '@/components/admin/PageHeader.vue';
import { useReportStore } from '@/stores/reportStore';
import { reportService } from '@/services/reportService';
import { useNotify } from '@/composables/useNotify';

use([BarChart, PieChart, GridComponent, TooltipComponent, LegendComponent, CanvasRenderer]);

const reportStore = useReportStore();
const { toast } = useNotify();

const now = new Date();
const selectedDate = ref(`${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`);
const currentMonth = ref(now.getMonth() + 1);
const currentYear = ref(now.getFullYear());
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

function fmt(v?: number | null) {
  return (v ?? 0).toLocaleString('th-TH', { minimumFractionDigits: 2 });
}

const yearlyOption = computed(() => {
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

const pieOption = computed(() => {
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

async function onDateChange(val: string) {
  const [y, m] = val.split('-').map(Number);
  currentYear.value = y;
  currentMonth.value = m;
  await reportStore.fetchAll(currentMonth.value, currentYear.value);
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

onMounted(() => reportStore.fetchAll(currentMonth.value, currentYear.value));
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
}
.card-label {
  font-size: 0.85rem;
  color: var(--text-light, #888);
  margin-bottom: 4px;
}
.card-amount {
  font-size: 1.5rem;
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
}
.card-title {
  font-weight: 600;
}
.member-row {
  display: flex;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid var(--border, #eee);
}
.member-info {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 120px;
  flex-shrink: 0;
}
.member-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  display: inline-block;
  flex-shrink: 0;
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
