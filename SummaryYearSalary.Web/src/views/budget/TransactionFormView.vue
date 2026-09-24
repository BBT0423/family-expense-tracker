<template>
  <div>
    <PageHeader />

    <el-card shadow="never">
      <el-form :model="form" label-position="top" @submit.prevent="handleSubmit">
        <!-- Income / Expense toggle -->
        <el-form-item label="ประเภท">
          <el-radio-group v-model="form.type" size="large" @change="form.categoryId = undefined">
            <el-radio-button value="Income" style="color: #27ae60">รายรับ</el-radio-button>
            <el-radio-button value="Expense" style="color: #e74c3c">รายจ่าย</el-radio-button>
          </el-radio-group>
        </el-form-item>

        <el-form-item label="วันที่" required>
          <el-date-picker
            v-model="form.transactionDate"
            type="date"
            format="DD/MMM/BBBB"
            value-format="YYYY-MM-DD"
            placeholder="เลือกวันที่"
            style="width: 100%"
            :shortcuts="dateShortcuts"
          />
        </el-form-item>

        <el-form-item label="จำนวนเงิน (฿)" required>
          <el-input
            v-model="amountDisplay"
            class="amount-input"
            placeholder="0.00"
            style="width: 100%; text-align: right"
            inputmode="decimal"
            @focus="onAmountFocus"
            @blur="onAmountBlur"
            @input="onAmountInput"
          >
            <template #prepend>฿</template>
          </el-input>
        </el-form-item>

        <el-form-item v-if="!isEdit" label="ลงให้คนอื่น">
          <el-switch v-model="forOther" active-text="ส่งให้เจ้าของยืนยัน" />
        </el-form-item>

        <el-form-item v-if="forOther" label="อีเมลผู้รับ" required>
          <el-select
            v-model="recipientEmails"
            multiple
            filterable
            allow-create
            default-first-option
            placeholder="เลือกหรือพิมพ์อีเมลผู้รับ (ใส่ได้หลายคน)"
            style="width: 100%"
          >
            <el-option
              v-for="u in userDirectoryStore.entries"
              :key="u.id"
              :label="`${u.displayName} (${u.email})`"
              :value="u.email"
            />
          </el-select>
          <div v-if="recipientHints.length" class="recipient-hint">
            <div
              v-for="hint in recipientHints"
              :key="hint.email"
              :class="hint.found ? 'recipient-ok' : ''"
            >
              {{ hint.email }}:
              {{
                hint.found ? `ส่งถึง ${hint.name}` : 'ไม่พบในระบบ (ยังส่งได้ ตรวจสอบอีเมลอีกครั้ง)'
              }}
            </div>
          </div>
          <div v-else class="recipient-hint">เจ้าของจะได้รับแจ้งเตือนให้ยืนยันรายการ</div>
        </el-form-item>

        <el-form-item label="หมวดหมู่" required>
          <el-select
            v-model="form.categoryId"
            placeholder="เลือกหมวดหมู่"
            style="width: 100%"
            filterable
          >
            <el-option
              v-for="c in activeCategories"
              :key="c.id"
              :label="`${c.icon} ${c.name}`"
              :value="c.id"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="หมายเหตุ">
          <el-input
            v-model="form.note"
            type="textarea"
            :rows="2"
            placeholder="หมายเหตุ (ไม่บังคับ)"
            maxlength="500"
            show-word-limit
          />
        </el-form-item>

        <div class="form-actions">
          <el-button @click="router.back()">ยกเลิก</el-button>
          <el-button type="primary" native-type="submit" :loading="saving">
            {{ submitLabel }}
          </el-button>
        </div>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import PageHeader from '@/components/admin/PageHeader.vue';
import { useCategoryStore } from '@/stores/categoryStore';
import { useTransactionStore } from '@/stores/transactionStore';
import { useUserDirectoryStore } from '@/stores/userDirectoryStore';
import { proposalService } from '@/services/proposalService';
import { toProblemDetails } from '@/utils/utilities';
import { useNotify } from '@/composables/useNotify';
import type { TransactionType, CreateTransactionRequest } from '@/types';
import dayjs from 'dayjs';

const router = useRouter();
const route = useRoute();
const categoryStore = useCategoryStore();
const transactionStore = useTransactionStore();
const userDirectoryStore = useUserDirectoryStore();
const { toast } = useNotify();

// ---- Log for someone else (owner must accept) ----
const forOther = ref(false);
const recipientEmails = ref<string[]>([]);

// el-select's allow-create doesn't dedupe: typing/selecting the same email
// twice appends it twice, which renders as one garbled, overlapping tag.
watch(recipientEmails, (val) => {
  const unique = [...new Set(val.map((e) => e.trim()).filter(Boolean))];
  if (unique.length !== val.length || unique.some((e, i) => e !== val[i])) {
    recipientEmails.value = unique;
  }
});

// Matches each entered email against the directory purely client-side (no
// extra API round trip per keystroke) — just a hint, the real check happens
// server-side per recipient on submit.
const recipientHints = computed(() =>
  recipientEmails.value.map((email) => {
    const match = userDirectoryStore.entries.find(
      (u) => u.email.toLowerCase() === email.toLowerCase()
    );
    return { email, found: !!match, name: match?.displayName ?? '' };
  })
);

watch(forOther, () => {
  form.value.categoryId = undefined;
});

const isEdit = computed(() => !!route.params.id);
const saving = ref(false);

const dateShortcuts = [
  { text: 'วันนี้', value: new Date() },
  {
    text: 'เมื่อวาน',
    value: () => {
      const d = new Date();
      d.setDate(d.getDate() - 1);
      return d;
    },
  },
  {
    text: '7 วันที่แล้ว',
    value: () => {
      const d = new Date();
      d.setDate(d.getDate() - 7);
      return d;
    },
  },
];

// ---- Amount field ----
const amountDisplay = ref('0.00');

function toFormatted(val: number): string {
  if (!val || isNaN(val)) return '0.00';
  const parts = val.toFixed(2).split('.');
  parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');
  return parts.join('.');
}

function onAmountFocus() {
  // แสดง raw number ให้แก้ไขง่าย
  amountDisplay.value = form.value.amount > 0 ? String(form.value.amount) : '';
}

function onAmountInput(val: string) {
  // อนุญาตแค่ตัวเลขกับจุดทศนิยม
  amountDisplay.value = val.replace(/[^0-9.]/g, '');
}

function onAmountBlur() {
  const num = parseFloat(amountDisplay.value.replace(/,/g, '')) || 0;
  form.value.amount = num;
  amountDisplay.value = toFormatted(num);
}

const form = ref<{
  type: TransactionType;
  transactionDate: string;
  amount: number;
  categoryId?: number;
  note?: string;
}>({
  type: 'Expense',
  transactionDate: dayjs().format('YYYY-MM-DD'),
  amount: 0,
});

// sync กรณี edit mode เซต form.amount จากภายนอก
watch(
  () => form.value.amount,
  (val) => {
    amountDisplay.value = toFormatted(val);
  },
  { immediate: true }
);

// Someone else's account can only be given the built-in categories (yours are private).
const activeCategories = computed(() =>
  forOther.value
    ? categoryStore.filterByType(form.value.type).filter((c) => c.isDefault)
    : categoryStore.filterByType(form.value.type)
);
const submitLabel = computed(() => {
  if (isEdit.value) return 'บันทึกการแก้ไข';
  return forOther.value ? 'ส่งให้ยืนยัน' : 'เพิ่มรายการ';
});

async function submitProposal() {
  if (!form.value.categoryId || form.value.amount <= 0 || recipientEmails.value.length === 0) {
    toast('กรุณากรอกข้อมูลให้ครบถ้วน', 'warning');
    return;
  }
  saving.value = true;
  const succeeded: string[] = [];
  const failed: { email: string; reason: string }[] = [];
  try {
    for (const email of recipientEmails.value) {
      try {
        const { data } = await proposalService.create({
          recipientEmail: email,
          type: form.value.type,
          transactionDate: form.value.transactionDate,
          amount: form.value.amount,
          categoryId: form.value.categoryId,
          note: form.value.note,
        });
        if (data?.isSuccess) {
          succeeded.push(email);
        } else {
          failed.push({ email, reason: data?.error ?? 'เกิดข้อผิดพลาด' });
        }
      } catch (e) {
        failed.push({ email, reason: toProblemDetails(e).detail });
      }
    }

    // Only clear out the recipients that actually went through, so a mistyped
    // email among several can be fixed and resent without redoing the rest.
    recipientEmails.value = recipientEmails.value.filter((e) => !succeeded.includes(e));

    if (failed.length === 0) {
      toast(
        succeeded.length > 1
          ? `ส่งให้เจ้าของยืนยันแล้ว ${succeeded.length} คน`
          : 'ส่งให้เจ้าของยืนยันแล้ว',
        'success'
      );
      router.push('/pages/transactions');
    } else {
      const summary = failed.map((f) => `${f.email}: ${f.reason}`).join('\n');
      if (succeeded.length > 0) {
        toast(
          `ส่งสำเร็จ ${succeeded.length} คน แต่พลาด ${failed.length} คน — ${summary}`,
          'warning'
        );
      } else {
        toast(summary, 'error');
      }
    }
  } finally {
    saving.value = false;
  }
}

async function handleSubmit() {
  if (forOther.value && !isEdit.value) return submitProposal();

  if (!form.value.categoryId || form.value.amount <= 0) {
    toast('กรุณากรอกข้อมูลให้ครบถ้วน', 'warning');
    return;
  }

  saving.value = true;
  try {
    const payload: CreateTransactionRequest = {
      type: form.value.type,
      transactionDate: form.value.transactionDate,
      amount: form.value.amount,
      categoryId: form.value.categoryId!,
      note: form.value.note,
    };

    let result;
    if (isEdit.value) {
      result = await transactionStore.updateTransaction(Number(route.params.id), payload);
    } else {
      result = await transactionStore.createTransaction(payload);
    }

    if (result?.isSuccess) {
      toast(isEdit.value ? 'แก้ไขรายการสำเร็จ' : 'เพิ่มรายการสำเร็จ', 'success');
      router.push('/pages/transactions');
    } else {
      toast(result?.error ?? 'เกิดข้อผิดพลาด', 'error');
    }
  } finally {
    saving.value = false;
  }
}

onMounted(async () => {
  await Promise.all([categoryStore.fetchCategories(), userDirectoryStore.fetchDirectory()]);

  if (isEdit.value) {
    // Pre-fill from existing transaction in store (or refetch)
    const existing = transactionStore.transactions.find((t) => t.id === Number(route.params.id));
    if (existing) {
      form.value = {
        type: existing.type,
        transactionDate: dayjs(existing.transactionDate).format('YYYY-MM-DD'),
        amount: existing.amount,
        categoryId: existing.categoryId,
        note: existing.note,
      };
    }
  }
});
</script>

<style scoped>
.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 8px;
}
.recipient-ok {
  margin-top: 4px;
  font-size: 13px;
  color: var(--el-color-success);
}
.recipient-error {
  margin-top: 4px;
  font-size: 13px;
  color: var(--el-color-danger);
}
.recipient-hint {
  margin-top: 4px;
  font-size: 12px;
  color: var(--text-muted);
}

:deep(.el-input-group__prepend) {
  background-color: var(--el-fill-color-light);
}

.amount-input :deep(.el-input__inner) {
  text-align: right;
}
</style>
