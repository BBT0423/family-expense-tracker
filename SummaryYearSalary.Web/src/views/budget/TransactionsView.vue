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

    <!-- Filters -->
    <el-card shadow="never" style="margin-bottom: 16px">
      <div class="filter-row">
        <el-date-picker
          v-model="selectedDate"
          type="month"
          format="MMM BBBB"
          value-format="YYYY-MM"
          placeholder="เดือน/ปี (ว่าง = ทุกเดือน)"
          clearable
          @change="onDateChange"
        />
        <el-select v-model="filterType" placeholder="ประเภท" clearable @change="onFilterChange">
          <el-option label="รายรับ" value="Income" />
          <el-option label="รายจ่าย" value="Expense" />
        </el-select>
        <el-select
          v-model="filterCategory"
          placeholder="หมวดหมู่"
          clearable
          @change="onFilterChange"
        >
          <el-option
            v-for="c in categoryStore.categories"
            :key="c.id"
            :label="`${c.icon} ${c.name}`"
            :value="c.id"
          />
        </el-select>
        <el-input
          v-model="searchTerm"
          placeholder="ค้นหาหมายเหตุ..."
          clearable
          @keyup.enter="onFilterChange"
          @clear="onFilterChange"
        >
          <template #prefix
            ><el-icon><Search /></el-icon
          ></template>
        </el-input>
      </div>
    </el-card>

    <!-- Table -->
    <el-card shadow="never">
      <el-table v-loading="transactionStore.loading" :data="transactionStore.transactions" stripe>
        <el-table-column label="วันที่" width="130" sortable>
          <template #default="{ row }">{{ formatDate(row.transactionDate) }}</template>
        </el-table-column>
        <el-table-column label="ประเภท" width="90">
          <template #default="{ row }">
            <el-tag :type="row.type === 'Income' ? 'success' : 'danger'" size="small">
              {{ row.type === 'Income' ? 'รายรับ' : 'รายจ่าย' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="หมวดหมู่">
          <template #default="{ row }">
            <span>{{ row.categoryIcon }} {{ row.categoryName }}</span>
            <span v-if="row.createdByName" class="created-by-tag"
              >(บันทึกโดย {{ row.createdByName }})</span
            >
          </template>
        </el-table-column>
        <el-table-column label="หมายเหตุ" prop="note" show-overflow-tooltip />
        <el-table-column label="จำนวนเงิน" align="right" width="140">
          <template #default="{ row }">
            <span :class="row.type === 'Income' ? 'income' : 'expense'" class="amount-text">
              {{ row.type === 'Income' ? '+' : '-' }}฿{{ formatAmount(row.amount) }}
            </span>
          </template>
        </el-table-column>
        <el-table-column label="" width="96" fixed="right" align="center">
          <template #default="{ row }">
            <div class="row-actions">
              <el-button
                text
                size="small"
                :icon="Edit"
                @click="router.push(`/pages/transactions/${row.id}/edit`)"
              />
              <el-button
                text
                size="small"
                :icon="Delete"
                type="danger"
                @click="handleDelete(row.id)"
              />
            </div>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrap">
        <el-pagination
          v-model:current-page="currentPage"
          :page-size="pageSize"
          :total="transactionStore.totalCount"
          layout="total, prev, pager, next"
          @current-change="onPageChange"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { formatThaiDate as formatDate } from '@/utils/formatters';
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { Plus, Download, Edit, Delete, Search } from '@element-plus/icons-vue';
import PageHeader from '@/components/admin/PageHeader.vue';
import { useTransactionStore } from '@/stores/transactionStore';
import { useCategoryStore } from '@/stores/categoryStore';
import { reportService } from '@/services/reportService';
import { useNotify } from '@/composables/useNotify';
import type { TransactionType } from '@/types';

const router = useRouter();
const transactionStore = useTransactionStore();
const categoryStore = useCategoryStore();
const { toast, confirm } = useNotify();

const now = new Date();
const selectedDate = ref(`${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`);
const currentMonth = ref(now.getMonth() + 1);
const currentYear = ref(now.getFullYear());
const filterType = ref<TransactionType | undefined>();
const filterCategory = ref<number | undefined>();
const searchTerm = ref('');
const currentPage = ref(1);
const pageSize = 20;
const exporting = ref(false);

function formatAmount(v: number) {
  return v.toLocaleString('th-TH', { minimumFractionDigits: 2 });
}

function getFilter() {
  return {
    month: currentMonth.value,
    year: currentYear.value,
    type: filterType.value,
    categoryId: filterCategory.value,
    searchTerm: searchTerm.value || undefined,
    pageNumber: currentPage.value,
    pageSize,
  };
}

async function onDateChange(val: string | null) {
  if (val) {
    const [y, m] = val.split('-').map(Number);
    currentYear.value = y;
    currentMonth.value = m;
  } else {
    // Cleared — 0/0 means "no month/year filter" to the backend, i.e. all time.
    currentYear.value = 0;
    currentMonth.value = 0;
  }
  currentPage.value = 1;
  await transactionStore.fetchTransactions(getFilter());
}

async function onFilterChange() {
  currentPage.value = 1;
  await transactionStore.fetchTransactions(getFilter());
}

async function onPageChange(page: number) {
  currentPage.value = page;
  await transactionStore.fetchTransactions(getFilter());
}

async function handleDelete(id: number) {
  const ok = await confirm('ต้องการลบรายการนี้หรือไม่?', { dangerConfirm: true });
  if (!ok) return;
  const result = await transactionStore.deleteTransaction(id);
  if (result?.isSuccess) toast('ลบรายการสำเร็จ', 'success');
  else toast(result?.error ?? 'เกิดข้อผิดพลาด', 'error');
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

onMounted(async () => {
  await Promise.all([
    categoryStore.fetchCategories(),
    transactionStore.fetchTransactions(getFilter()),
  ]);
});
</script>

<style scoped>
.filter-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
  gap: 12px;
  align-items: center;
}
.filter-row > * {
  width: 100%;
}
.amount-text {
  font-weight: 600;
}
.income {
  color: #27ae60;
}
.expense {
  color: #e74c3c;
}
.pagination-wrap {
  display: flex;
  justify-content: flex-end;
  margin-top: 16px;
}
.created-by-tag {
  display: block;
  font-size: 0.75rem;
  color: var(--text-muted);
}
</style>
