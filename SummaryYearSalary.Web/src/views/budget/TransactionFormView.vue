<template>
  <div>
    <PageHeader />

    <el-card shadow="never" data-tour="txn-form">
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

        <el-form-item label="สำหรับใคร" required>
          <el-select
            v-model="form.memberId"
            placeholder="เลือกสมาชิก"
            style="width: 100%"
            filterable
          >
            <el-option
              v-for="m in memberStore.activeMembers"
              :key="m.id"
              :label="m.name"
              :value="m.id"
            >
              <span class="member-option">
                <span class="member-dot" :style="{ background: m.color }" />
                {{ m.name }}
              </span>
            </el-option>
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
import { useMemberStore } from '@/stores/memberStore';
import { useTransactionStore } from '@/stores/transactionStore';
import { useNotify } from '@/composables/useNotify';
import type { TransactionType, CreateTransactionRequest } from '@/types';
import dayjs from 'dayjs';

const router = useRouter();
const route = useRoute();
const categoryStore = useCategoryStore();
const memberStore = useMemberStore();
const transactionStore = useTransactionStore();
const { toast } = useNotify();

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
  memberId?: number;
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

const activeCategories = computed(() => categoryStore.filterByType(form.value.type));
const submitLabel = computed(() => (isEdit.value ? 'บันทึกการแก้ไข' : 'เพิ่มรายการ'));

async function handleSubmit() {
  if (!form.value.categoryId || !form.value.memberId || form.value.amount <= 0) {
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
      memberId: form.value.memberId,
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
  await Promise.all([categoryStore.fetchCategories(), memberStore.fetchMembers()]);

  if (isEdit.value) {
    // Pre-fill from existing transaction in store (or refetch)
    const existing = transactionStore.transactions.find((t) => t.id === Number(route.params.id));
    if (existing) {
      form.value = {
        type: existing.type,
        transactionDate: dayjs(existing.transactionDate).format('YYYY-MM-DD'),
        amount: existing.amount,
        categoryId: existing.categoryId,
        memberId: existing.memberId,
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

:deep(.el-input-group__prepend) {
  background-color: var(--el-fill-color-light);
}

.amount-input :deep(.el-input__inner) {
  text-align: right;
}

.member-option {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}
.member-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  flex-shrink: 0;
}
</style>
