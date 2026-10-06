<template>
  <el-popover trigger="hover" placement="right" :width="300" :show-after="150" @show="loadHistory">
    <template #reference>
      <span class="txn-detail-trigger">
        {{ row.categoryIcon }} {{ row.categoryName }}
        <span v-if="createdByLabel" class="created-by-tag">(บันทึกโดย {{ createdByLabel }})</span>
      </span>
    </template>
    <div class="txn-popover">
      <div class="txn-popover-head">
        <span class="txn-popover-title">{{ row.categoryIcon }} {{ row.categoryName }}</span>
        <el-tag :type="row.type === 'Income' ? 'success' : 'danger'" size="small">
          {{ row.type === 'Income' ? 'รายรับ' : 'รายจ่าย' }}
        </el-tag>
      </div>
      <div class="txn-popover-amount" :class="row.type === 'Income' ? 'income' : 'expense'">
        {{ row.type === 'Income' ? '+' : '-' }}฿{{ formatAmount(row.amount) }}
      </div>

      <div class="txn-popover-meta">
        <span class="label">บันทึกโดย</span>
        <span>{{ createdByLabel || 'คุณ' }} · {{ formatDateTime(row.createdAt) }}</span>
      </div>
      <div class="txn-popover-meta">
        <span class="label">ประวัติ</span>
        <span v-if="loading">กำลังโหลด...</span>
        <span v-else-if="!summary">ยังไม่เคยแก้ไข</span>
        <span v-else>{{ summary }}</span>
      </div>

      <div class="txn-popover-actions">
        <el-button
          size="small"
          type="primary"
          @click="router.push(`/pages/transactions/${row.id}/edit`)"
        >
          แก้ไข
        </el-button>
        <el-button size="small" @click="handleDuplicate">ทำซ้ำ</el-button>
        <el-button size="small" type="danger" text @click="handleDelete">ลบ</el-button>
      </div>
    </div>
  </el-popover>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { formatDateTime } from '@/utils/formatters';
import { useTransactionStore } from '@/stores/transactionStore';
import { transactionService } from '@/services/transactionService';
import { useNotify } from '@/composables/useNotify';
import type { Transaction, TransactionHistoryEntry } from '@/types';

const props = defineProps<{ row: Transaction }>();

const router = useRouter();
const transactionStore = useTransactionStore();
const { toast, confirm } = useNotify();

// TransactionsView populates createdByName (creator, when different from the
// signed-in owner); DashboardView's combined-household view populates
// ownerName instead — either one answers "who does this row belong to".
const createdByLabel = computed(() => props.row.createdByName || props.row.ownerName);

function formatAmount(v: number) {
  return v.toLocaleString('th-TH', { minimumFractionDigits: 2 });
}

// One popover instance per row (rendered inside a v-for), so plain local
// refs are enough — no cross-row cache map needed.
const history = ref<TransactionHistoryEntry[] | null>(null);
const loading = ref(false);

async function loadHistory() {
  if (history.value !== null || loading.value) return;
  loading.value = true;
  try {
    const { data } = await transactionService.getHistory(props.row.id);
    history.value = data?.isSuccess && data.value ? data.value : [];
  } catch {
    history.value = [];
  } finally {
    loading.value = false;
  }
}

// Show only the most recent edit "batch" (every field changed in the same
// save shares the same timestamp) as one line, e.g.
// "paa แก้จำนวนเงิน 400.00 → 420.00, หมายเหตุ เก่า → ใหม่".
const summary = computed(() => {
  const h = history.value;
  if (!h || h.length === 0) return '';
  const latestAt = h[0].changedAt;
  const latestBatch = h.filter((entry) => entry.changedAt === latestAt);
  const changes = latestBatch
    .map((entry) => `${entry.fieldName} ${entry.oldValue ?? '-'} → ${entry.newValue ?? '-'}`)
    .join(', ');
  return `${latestBatch[0].changedByName} แก้ไข ${changes}`;
});

async function handleDuplicate() {
  const result = await transactionStore.createTransaction({
    categoryId: props.row.categoryId,
    memberId: props.row.memberId,
    type: props.row.type,
    amount: props.row.amount,
    note: props.row.note,
    transactionDate: props.row.transactionDate,
  });
  if (result?.isSuccess) {
    toast('ทำซ้ำรายการสำเร็จ', 'success');
    await transactionStore.fetchTransactions();
  } else {
    toast(result?.error ?? 'เกิดข้อผิดพลาด', 'error');
  }
}

async function handleDelete() {
  const ok = await confirm('ต้องการลบรายการนี้หรือไม่?', { dangerConfirm: true });
  if (!ok) return;
  const result = await transactionStore.deleteTransaction(props.row.id);
  if (result?.isSuccess) toast('ลบรายการสำเร็จ', 'success');
  else toast(result?.error ?? 'เกิดข้อผิดพลาด', 'error');
}
</script>

<style scoped>
.txn-detail-trigger {
  cursor: help;
  text-decoration: underline dotted;
  text-underline-offset: 3px;
}
.created-by-tag {
  display: block;
  font-size: 0.75rem;
  color: var(--text-muted);
  text-decoration: none;
}
.txn-popover {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.txn-popover-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}
.txn-popover-title {
  font-weight: 600;
  font-size: 14px;
}
.txn-popover-amount {
  font-size: 22px;
  font-weight: 700;
}
.txn-popover-amount.income {
  color: #27ae60;
}
.txn-popover-amount.expense {
  color: #e74c3c;
}
.txn-popover-meta {
  display: flex;
  flex-direction: column;
  gap: 2px;
  font-size: 12.5px;
  padding-top: 6px;
  border-top: 1px solid var(--border);
}
.txn-popover-meta .label {
  color: var(--text-muted);
}
.txn-popover-actions {
  display: flex;
  gap: 8px;
  padding-top: 4px;
}
.txn-popover-actions .el-button {
  flex: 1;
  margin-left: 0;
}
</style>
