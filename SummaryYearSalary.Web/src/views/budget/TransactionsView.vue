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
          format="MM/YYYY"
          value-format="YYYY-MM"
          placeholder="เดือน/ปี"
          style="width: 150px"
          @change="onDateChange"
        />
        <el-select
          v-model="filterType"
          placeholder="ประเภท"
          clearable
          style="width: 120px"
          @change="onFilterChange"
        >
          <el-option label="รายรับ" value="Income" />
          <el-option label="รายจ่าย" value="Expense" />
        </el-select>
        <el-select
          v-model="filterCategory"
          placeholder="หมวดหมู่"
          clearable
          style="width: 160px"
          @change="onFilterChange"
        >
          <el-option
            v-for="c in categoryStore.categories"
            :key="c.id"
            :label="`${c.icon} ${c.name}`"
            :value="c.id"
          />
        </el-select>
        <el-select
          v-model="filterMember"
          placeholder="สมาชิก"
          clearable
          style="width: 140px"
          @change="onFilterChange"
        >
          <el-option v-for="m in memberStore.members" :key="m.id" :label="m.name" :value="m.id" />
        </el-select>
        <el-input
          v-model="searchTerm"
          placeholder="ค้นหาหมายเหตุ..."
          clearable
          style="width: 200px"
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
        <el-table-column label="วันที่" width="100" sortable>
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
          </template>
        </el-table-column>
        <el-table-column label="สมาชิก" prop="memberName" width="110" />
        <el-table-column label="หมายเหตุ" prop="note" show-overflow-tooltip />
        <el-table-column label="จำนวนเงิน" align="right" width="140">
          <template #default="{ row }">
            <span :class="row.type === 'Income' ? 'income' : 'expense'" class="amount-text">
              {{ row.type === 'Income' ? '+' : '-' }}฿{{ formatAmount(row.amount) }}
            </span>
          </template>
        </el-table-column>
        <el-table-column label="" width="100" fixed="right">
          <template #default="{ row }">
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
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { Plus, Download, Edit, Delete, Search } from '@element-plus/icons-vue';
import PageHeader from '@/components/admin/PageHeader.vue';
import { useTransactionStore } from '@/stores/transactionStore';
import { useMemberStore } from '@/stores/memberStore';
import { useCategoryStore } from '@/stores/categoryStore';
import { reportService } from '@/services/reportService';
import { useNotify } from '@/composables/useNotify';
import type { TransactionType } from '@/types';

const router = useRouter();
const transactionStore = useTransactionStore();
const memberStore = useMemberStore();
const categoryStore = useCategoryStore();
const { toast, confirm } = useNotify();

const now = new Date();
const selectedDate = ref(`${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`);
const currentMonth = ref(now.getMonth() + 1);
const currentYear = ref(now.getFullYear());
const filterType = ref<TransactionType | undefined>();
const filterCategory = ref<number | undefined>();
const filterMember = ref<number | undefined>();
const searchTerm = ref('');
const currentPage = ref(1);
const pageSize = 20;
const exporting = ref(false);

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('th-TH', {
    day: '2-digit',
    month: '2-digit',
    year: '2-digit',
  });
}

function formatAmount(v: number) {
  return v.toLocaleString('th-TH', { minimumFractionDigits: 2 });
}

function getFilter() {
  return {
    month: currentMonth.value,
    year: currentYear.value,
    type: filterType.value,
    categoryId: filterCategory.value,
    memberId: filterMember.value,
    searchTerm: searchTerm.value || undefined,
    pageNumber: currentPage.value,
    pageSize,
  };
}

async function onDateChange(val: string) {
  const [y, m] = val.split('-').map(Number);
  currentYear.value = y;
  currentMonth.value = m;
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
    memberStore.fetchMembers(),
    categoryStore.fetchCategories(),
    transactionStore.fetchTransactions(getFilter()),
  ]);
});
</script>

<style scoped>
.filter-row {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  align-items: center;
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
</style>
