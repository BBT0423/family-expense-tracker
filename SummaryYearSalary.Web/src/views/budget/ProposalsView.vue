<template>
  <div>
    <PageHeader />

    <el-card v-loading="store.loading" shadow="never">
      <el-empty
        v-if="!store.loading && store.incoming.length === 0"
        description="ไม่มีรายการรอยืนยัน"
      />

      <div v-else class="proposal-list">
        <div v-for="p in store.incoming" :key="p.id" class="proposal-item">
          <div class="proposal-head">
            <div>
              <div class="proposal-from">{{ p.proposedByName }} ลงรายการให้คุณ</div>
              <div class="proposal-meta">{{ formatThaiDate(p.transactionDate) }}</div>
            </div>
            <div class="proposal-amount" :class="p.type === 'Income' ? 'income' : 'expense'">
              {{ p.type === 'Income' ? '+' : '-' }}{{ formatNumber(p.amount, 2) }} ฿
            </div>
          </div>

          <div v-if="p.note" class="proposal-note">{{ p.note }}</div>

          <div class="proposal-fields">
            <el-select v-model="draft(p.id).categoryId" placeholder="หมวดหมู่">
              <el-option
                v-for="c in categoriesFor(p.type)"
                :key="c.id"
                :label="`${c.icon} ${c.name}`"
                :value="c.id"
              />
            </el-select>
          </div>

          <div class="proposal-actions">
            <el-button :loading="busyId === p.id" @click="handleReject(p)">ปฏิเสธ</el-button>
            <el-button type="primary" :loading="busyId === p.id" @click="handleAccept(p)">
              ยอมรับ
            </el-button>
          </div>
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref, watch } from 'vue';
import PageHeader from '@/components/admin/PageHeader.vue';
import { useProposalStore } from '@/stores/proposalStore';
import { useCategoryStore } from '@/stores/categoryStore';
import { useNotify } from '@/composables/useNotify';
import { formatNumber, formatThaiDate } from '@/utils/formatters';
import { toProblemDetails } from '@/utils/utilities';
import type { TransactionProposal, TransactionType } from '@/types';

const store = useProposalStore();
const categoryStore = useCategoryStore();
const { toast, confirm } = useNotify();

const busyId = ref<number | null>(null);
const drafts = reactive<Record<number, { categoryId?: number }>>({});

function draft(id: number) {
  return (drafts[id] ??= {});
}

// Default the category to what the proposer picked; the owner may change it.
watch(
  () => store.incoming,
  (list) => {
    for (const p of list) {
      const d = draft(p.id);
      d.categoryId ??= p.categoryId;
    }
  },
  { immediate: true }
);

const categoriesFor = (type: TransactionType) => categoryStore.filterByType(type);

async function run(
  p: TransactionProposal,
  action: () => Promise<{ isSuccess?: boolean } | undefined>,
  ok: string
) {
  busyId.value = p.id;
  try {
    const res = await action();
    if (res?.isSuccess) toast(ok, 'success');
  } catch (e) {
    toast(toProblemDetails(e).detail, 'error');
  } finally {
    busyId.value = null;
  }
}

function handleAccept(p: TransactionProposal) {
  const d = draft(p.id);
  return run(p, () => store.accept(p.id, { categoryId: d.categoryId }), 'ยอมรับรายการแล้ว');
}

async function handleReject(p: TransactionProposal) {
  const yes = await confirm(`ปฏิเสธรายการของ ${p.proposedByName}? รายการจะถูกทิ้งและแจ้งผู้ลง`, {
    dangerConfirm: true,
  });
  if (yes) await run(p, () => store.reject(p.id), 'ปฏิเสธรายการแล้ว');
}

onMounted(() => {
  categoryStore.fetchCategories();
  store.fetchIncoming();
});
</script>

<style scoped>
.proposal-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.proposal-item {
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 16px;
}
.proposal-head {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
}
.proposal-from {
  font-weight: 600;
  color: var(--text);
}
.proposal-meta {
  font-size: 13px;
  color: var(--text-muted);
  margin-top: 2px;
}
.proposal-amount {
  font-size: 1.15rem;
  font-weight: 700;
  white-space: nowrap;
}
.proposal-amount.income {
  color: var(--el-color-success);
}
.proposal-amount.expense {
  color: var(--el-color-danger);
}
.proposal-note {
  margin-top: 8px;
  font-size: 14px;
  color: var(--text-muted);
}
.proposal-fields {
  display: grid;
  grid-template-columns: 1fr;
  gap: 12px;
  margin-top: 12px;
}
.proposal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 12px;
}
</style>
