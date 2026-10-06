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
        <el-button
          v-if="selectedRows.length"
          type="danger"
          :icon="Delete"
          @click="handleBulkDelete"
        >
          ลบที่เลือก ({{ selectedRows.length }})
        </el-button>
      </template>
    </PageHeader>

    <!-- Filters -->
    <el-card shadow="never" style="margin-bottom: 16px" data-tour="txn-filter">
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
        <el-select v-model="filterMember" placeholder="สมาชิก" clearable @change="onFilterChange">
          <el-option
            v-for="m in memberStore.activeMembers"
            :key="m.id"
            :label="m.name"
            :value="m.id"
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
    <el-card shadow="never" data-tour="txn-table">
      <el-table
        v-loading="transactionStore.loading"
        :data="transactionStore.transactions"
        stripe
        height="600"
        row-key="id"
        @selection-change="handleSelectionChange"
      >
        <el-table-column type="selection" width="48" />
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
            <TransactionDetailPopover :row="row" />
          </template>
        </el-table-column>
        <el-table-column label="สมาชิก" width="120">
          <template #default="{ row }">
            <span v-if="row.memberName" class="member-cell">
              <span class="member-dot" :style="{ background: row.memberColor }" />
              {{ row.memberName }}
            </span>
            <span v-else class="member-cell-empty">-</span>
          </template>
        </el-table-column>
        <el-table-column label="หมายเหตุ" prop="note" min-width="160" class-name="note-cell" />
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

        <template #append>
          <div class="footer-badges">
            <span class="footer-badges-label">ยอดรวมทั้งหมด</span>
            <span class="footer-badge income">รายรับ +฿{{ formatAmount(allTimeIncome) }}</span>
            <span class="footer-badge expense">รายจ่าย -฿{{ formatAmount(allTimeExpense) }}</span>
            <span class="footer-badge" :class="allTimeBalance >= 0 ? 'income' : 'expense'">
              คงเหลือ {{ allTimeBalance >= 0 ? '+' : '-' }}฿{{ formatAmount(Math.abs(allTimeBalance)) }}
            </span>
          </div>
        </template>
      </el-table>

      <div class="pagination-wrap">
        <el-pagination
          v-model:current-page="currentPage"
          :page-size="pageSize"
          :page-sizes="[5, 10, 25]"
          :total="transactionStore.totalCount"
          layout="total, sizes, prev, pager, next"
          @current-change="onPageChange"
          @size-change="onPageSizeChange"
        />
      </div>
    </el-card>

    <ImportProgressOverlay
      :visible="bulkDeleteVisible"
      title="กำลังลบรายการ"
      hint="กรุณาอย่าปิดหรือเปลี่ยนหน้าจนกว่าจะเสร็จ"
      :steps="['กำลังลบรายการ']"
      :current-step="0"
      :done="bulkDeleteDone"
      :total="bulkDeleteTotal"
      :finished="bulkDeleteFinished"
      unit-label="รายการ"
      finished-title="ลบข้อมูลเสร็จแล้ว"
      :finished-message="`ลบ ${bulkDeleteTotal} รายการเรียบร้อย`"
      @close="onBulkDeleteClose"
    />
  </div>
</template>

<script setup lang="ts">
import { formatThaiDate as formatDate } from '@/utils/formatters';
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { Plus, Download, Edit, Delete, Search } from '@element-plus/icons-vue';
import PageHeader from '@/components/admin/PageHeader.vue';
import TransactionDetailPopover from '@/components/budget/TransactionDetailPopover.vue';
import ImportProgressOverlay from '@/components/common/ImportProgressOverlay.vue';
import { useTransactionStore } from '@/stores/transactionStore';
import { useCategoryStore } from '@/stores/categoryStore';
import { useMemberStore } from '@/stores/memberStore';
import { useReportStore } from '@/stores/reportStore';
import { reportService } from '@/services/reportService';
import { transactionService } from '@/services/transactionService';
import { useNotify } from '@/composables/useNotify';
import type { Transaction, TransactionType } from '@/types';

const route = useRoute();
const router = useRouter();
const transactionStore = useTransactionStore();
const categoryStore = useCategoryStore();
const memberStore = useMemberStore();
const reportStore = useReportStore();
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
const pageSize = ref(10);
const exporting = ref(false);
const selectedRows = ref<Transaction[]>([]);
const bulkDeleteVisible = ref(false);
const bulkDeleteDone = ref(0);
const bulkDeleteTotal = ref(0);
const bulkDeleteFinished = ref(false);

function handleSelectionChange(rows: Transaction[]) {
  selectedRows.value = rows;
}

function formatAmount(v: number) {
  return v.toLocaleString('th-TH', { minimumFractionDigits: 2 });
}

const allTimeIncome = computed(() => reportStore.allTimeSummary?.totalIncome ?? 0);
const allTimeExpense = computed(() => reportStore.allTimeSummary?.totalExpense ?? 0);
const allTimeBalance = computed(() => reportStore.allTimeSummary?.balance ?? 0);

function getFilter() {
  return {
    month: currentMonth.value,
    year: currentYear.value,
    type: filterType.value,
    categoryId: filterCategory.value,
    memberId: filterMember.value,
    searchTerm: searchTerm.value || undefined,
    pageNumber: currentPage.value,
    pageSize: pageSize.value,
  };
}

async function onPageSizeChange(size: number) {
  pageSize.value = size;
  currentPage.value = 1;
  await transactionStore.fetchTransactions(getFilter());
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
  await Promise.all([
    transactionStore.fetchTransactions(getFilter()),
    reportStore.fetchAllTimeSummary(filterMember.value),
  ]);
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

async function handleBulkDelete() {
  const ids = selectedRows.value.map((r) => r.id);
  if (!ids.length) return;
  const ok = await confirm(`ต้องการลบ ${ids.length} รายการที่เลือกหรือไม่?`, {
    dangerConfirm: true,
  });
  if (!ok) return;

  bulkDeleteDone.value = 0;
  bulkDeleteTotal.value = ids.length;
  bulkDeleteFinished.value = false;
  bulkDeleteVisible.value = true;

  // Fire deletes in parallel (not one-at-a-time) — same lesson learned from
  // the import auto-create perf fix: sequential round-trips to even a local
  // backend add up fast, parallel + per-item progress ticks stays both fast
  // and honestly synced to real completions.
  let failures = 0;
  await Promise.all(
    ids.map(async (id) => {
      try {
        const result = await transactionService.remove(id);
        if (!result.data?.isSuccess) failures++;
      } catch {
        failures++;
      } finally {
        bulkDeleteDone.value++;
      }
    })
  );

  bulkDeleteFinished.value = true;
  await transactionStore.fetchTransactions();
  selectedRows.value = [];

  if (failures > 0) {
    toast(`ลบสำเร็จ ${ids.length - failures}/${ids.length} รายการ`, 'error');
  } else {
    toast(`ลบสำเร็จ ${ids.length} รายการ`, 'success');
  }
}

function onBulkDeleteClose() {
  bulkDeleteVisible.value = false;
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
  // Deep-linked from the Dashboard's "เปิดในหน้ารายการ →" — pre-fill the
  // filters from the URL before the first fetch so the list opens already
  // scoped to that category/month/member instead of showing everything.
  const q = route.query;
  if (q.categoryId) filterCategory.value = Number(q.categoryId);
  if (q.memberId) filterMember.value = Number(q.memberId);
  if (q.month && q.year) {
    currentMonth.value = Number(q.month);
    currentYear.value = Number(q.year);
    selectedDate.value = `${currentYear.value}-${String(currentMonth.value).padStart(2, '0')}`;
  }

  await Promise.all([
    categoryStore.fetchCategories(),
    memberStore.fetchMembers(),
    transactionStore.fetchTransactions(getFilter()),
    reportStore.fetchAllTimeSummary(filterMember.value),
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
.member-cell {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}
.member-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}
.member-cell-empty {
  color: var(--text-muted);
}
:deep(.note-cell .cell) {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.footer-badges {
  position: sticky;
  bottom: 0;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 10px;
  padding: 12px 16px;
  background: var(--surface);
  border-top: 1px solid var(--border);
}
.footer-badges-label {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--text-muted);
  margin-right: 4px;
}
.footer-badge {
  display: inline-flex;
  align-items: center;
  padding: 5px 12px;
  border-radius: 999px;
  font-size: 0.85rem;
  font-weight: 700;
  white-space: nowrap;
}
.footer-badge.income {
  background: var(--success-light, #dcfce7);
  color: var(--success-dark, #0f6930);
}
.footer-badge.expense {
  background: var(--danger-light, #fee2e2);
  color: var(--danger-dark, #a31919);
}
</style>
