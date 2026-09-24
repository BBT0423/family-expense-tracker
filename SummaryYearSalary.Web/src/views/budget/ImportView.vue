<template>
  <div>
    <PageHeader />

    <el-card shadow="never">
      <el-steps :active="step" finish-status="success" style="margin-bottom: 32px">
        <el-step title="อัปโหลดไฟล์" />
        <el-step title="แมปคอลัมน์" />
        <el-step title="ตรวจสอบข้อมูล" />
        <el-step title="นำเข้าสำเร็จ" />
      </el-steps>

      <!-- Step 1: Upload -->
      <template v-if="step === 0">
        <el-upload
          drag
          accept=".xlsx,.csv"
          :auto-upload="false"
          :limit="1"
          :on-change="onFileChange"
          :file-list="fileList"
        >
          <el-icon size="48" class="upload-icon"><UploadFilled /></el-icon>
          <div class="upload-text">คลิกหรือลากไฟล์มาวางที่นี่</div>
          <div class="upload-hint">รองรับไฟล์ .xlsx และ .csv</div>
        </el-upload>

        <el-divider>รูปแบบไฟล์ที่ต้องการ</el-divider>
        <el-table :data="templateRows" size="small" border>
          <el-table-column prop="col" label="คอลัมน์" width="80" />
          <el-table-column prop="name" label="ชื่อ" />
          <el-table-column prop="required" label="บังคับ" width="80" align="center">
            <template #default="{ row }">
              <el-tag :type="row.required ? 'danger' : 'info'" size="small">{{
                row.required ? 'ใช่' : 'ไม่'
              }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="example" label="ตัวอย่าง" />
        </el-table>

        <div class="step-actions">
          <el-button type="primary" :disabled="!parsedRows.length" @click="step = 1"
            >ถัดไป</el-button
          >
        </div>
      </template>

      <!-- Step 2: Mapping -->
      <template v-if="step === 1">
        <p class="step-desc">กรุณาระบุคอลัมน์ของไฟล์ที่ตรงกับข้อมูลแต่ละประเภท</p>
        <el-form label-position="top">
          <el-form-item
            v-for="f in mappingFields"
            :key="f.key"
            :label="f.label + (f.required ? ' *' : '')"
          >
            <el-select v-model="mapping[f.key]" placeholder="เลือกคอลัมน์" style="width: 100%">
              <el-option v-for="col in headers" :key="col" :label="col" :value="col" />
            </el-select>
          </el-form-item>
        </el-form>
        <div class="step-actions">
          <el-button @click="step = 0">ย้อนกลับ</el-button>
          <el-button type="primary" @click="buildPreview">ถัดไป</el-button>
        </div>
      </template>

      <!-- Step 3: Preview -->
      <template v-if="step === 2">
        <p class="step-desc">ตรวจสอบข้อมูลก่อนนำเข้า (เลื่อนดูได้ทั้งหมด)</p>
        <el-table :data="previewRows" size="small" border height="480">
          <el-table-column label="วันที่" prop="transactionDate" width="100" />
          <el-table-column label="ประเภท" width="80">
            <template #default="{ row }">
              <el-tag :type="row.type === 'Income' ? 'success' : 'danger'" size="small">
                {{ row.type === 'Income' ? 'รายรับ' : 'รายจ่าย' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="หมวดหมู่" prop="categoryName" />
          <el-table-column label="จำนวนเงิน" prop="amount" align="right" width="110" />
          <el-table-column label="หมายเหตุ" prop="note" show-overflow-tooltip />
          <el-table-column label="สถานะ" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="row.valid ? 'success' : 'danger'" size="small">
                {{ row.valid ? 'ถูกต้อง' : 'ข้อผิดพลาด' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="สาเหตุ" prop="reason" min-width="200" show-overflow-tooltip />
        </el-table>
        <p class="preview-summary">
          ทั้งหมด {{ previewRows.length }} รายการ — ถูกต้อง {{ validRows }} / ข้อผิดพลาด
          {{ previewRows.length - validRows }}
        </p>
        <div class="step-actions">
          <el-button @click="step = 1">ย้อนกลับ</el-button>
          <el-button type="primary" :disabled="validRows === 0" @click="handleImport">
            นำเข้า {{ validRows }} รายการ
          </el-button>
        </div>
      </template>

      <!-- Step 4: Queued — the import itself now runs in the background via
           importJobStore, so this step never blocks navigation. -->
      <template v-if="step === 3">
        <div class="queued-panel">
          <template v-if="importStatus === 'running'">
            <el-icon size="48" class="queued-icon running"><Loading /></el-icon>
            <p class="queued-title">
              กำลังนำเข้า {{ importCompleted }} รายการ ({{ importPercent }}%)
            </p>
            <el-progress :percentage="importPercent" style="max-width: 320px; margin: 0 auto" />
            <p class="queued-hint">ไปหน้าไหนก็ได้ระหว่างนี้ ระบบจะแจ้งเตือนเมื่อเสร็จ</p>
          </template>
          <template v-else-if="importStatus === 'done'">
            <el-icon size="48" class="queued-icon done"><CircleCheck /></el-icon>
            <p class="queued-title">นำเข้าสำเร็จ {{ importCompleted }} รายการ</p>
          </template>
          <template v-else>
            <el-icon size="48" class="queued-icon"><Upload /></el-icon>
            <p class="queued-title">เริ่มนำเข้าแล้ว — ดูสถานะได้ที่มุมขวาบน</p>
          </template>
          <div class="step-actions" style="justify-content: center">
            <el-button type="primary" @click="router.push('/pages/transactions')"
              >ดูรายการทั้งหมด</el-button
            >
            <el-button @click="resetImport">นำเข้าไฟล์อื่น</el-button>
          </div>
        </div>
      </template>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { UploadFilled, Loading, CircleCheck, Upload } from '@element-plus/icons-vue';
import * as XLSX from 'xlsx';
import PageHeader from '@/components/admin/PageHeader.vue';
import { useCategoryStore } from '@/stores/categoryStore';
import { useImportJobStore } from '@/stores/importJobStore';
import { useNotify } from '@/composables/useNotify';
import type { BulkImportItem, TransactionType } from '@/types';
import dayjs from 'dayjs';

const router = useRouter();
const categoryStore = useCategoryStore();
const importJobStore = useImportJobStore();
const {
  status: importStatus,
  percent: importPercent,
  completed: importCompleted,
} = storeToRefs(importJobStore);
const { toast, confirm } = useNotify();

const step = ref(0);
const fileList = ref<any[]>([]);
const parsedRows = ref<Record<string, string>[]>([]);
const headers = ref<string[]>([]);
const previewRows = ref<any[]>([]);

const mapping = ref<Record<string, string>>({
  transactionDate: '',
  type: '',
  categoryName: '',
  amount: '',
  note: '',
});

const mappingFields = [
  { key: 'transactionDate', label: 'วันที่ (DD/MM/YYYY ค.ศ. หรือ พ.ศ.)', required: true },
  { key: 'type', label: 'ประเภท (Income/Expense/รายรับ/รายจ่าย)', required: true },
  { key: 'categoryName', label: 'หมวดหมู่', required: true },
  { key: 'amount', label: 'จำนวนเงิน', required: true },
  { key: 'note', label: 'หมายเหตุ', required: false },
];

const templateRows = [
  { col: 'A', name: 'วันที่', required: true, example: '01/04/2026' },
  { col: 'B', name: 'ประเภท', required: true, example: 'รายจ่าย / Expense' },
  { col: 'C', name: 'หมวดหมู่', required: true, example: 'อาหาร/เครื่องดื่ม' },
  { col: 'D', name: 'จำนวนเงิน', required: true, example: '150.00' },
  { col: 'E', name: 'หมายเหตุ', required: false, example: 'ข้าวเที่ยง' },
];

const validRows = computed(() => previewRows.value.filter((r) => r.valid).length);

onMounted(() => {
  categoryStore.fetchCategories();
});

function onFileChange(file: any) {
  fileList.value = [file];
  const reader = new FileReader();
  reader.onload = (e) => {
    const data = e.target?.result;
    const wb = XLSX.read(data, { type: 'binary' });
    const ws = wb.Sheets[wb.SheetNames[0]];
    const json: Record<string, string>[] = XLSX.utils.sheet_to_json(ws, { raw: false });
    if (json.length > 0) {
      headers.value = Object.keys(json[0]);
      // Auto-map common column names
      headers.value.forEach((h) => {
        const lh = h.toLowerCase();
        if (lh.includes('วันที่') || lh.includes('date')) mapping.value.transactionDate = h;
        if (lh.includes('ประเภท') || lh.includes('type')) mapping.value.type = h;
        if (lh.includes('หมวด') || lh.includes('category')) mapping.value.categoryName = h;
        if (lh.includes('จำนวน') || lh.includes('amount')) mapping.value.amount = h;
        if (lh.includes('หมาย') || lh.includes('note')) mapping.value.note = h;
      });
    }
    parsedRows.value = json;
  };
  reader.readAsBinaryString(file.raw);
}

function buildPreview() {
  const categories = categoryStore.categories;

  previewRows.value = parsedRows.value.map((row) => {
    const dateStr = row[mapping.value.transactionDate] ?? '';
    const typeStr = (row[mapping.value.type] ?? '').toLowerCase();
    const categoryName = row[mapping.value.categoryName] ?? '';
    const amountStr = row[mapping.value.amount] ?? '';
    const note = row[mapping.value.note] ?? '';

    const type: TransactionType =
      typeStr.includes('income') || typeStr.includes('รายรับ') ? 'Income' : 'Expense';
    const amount = parseFloat(amountStr.replace(/,/g, ''));
    const category = categories.find((c) => c.name === categoryName);

    const rawParsed = dayjs(dateStr, ['DD/MM/YYYY', 'YYYY-MM-DD', 'MM/DD/YYYY']);
    // Excel from Thai users often carries Buddhist-era years (e.g. 16/09/2569)
    const parsed =
      rawParsed.isValid() && rawParsed.year() > 2400 ? rawParsed.subtract(543, 'year') : rawParsed;

    const reasons: string[] = [];
    if (!category) {
      reasons.push(`ไม่พบหมวดหมู่ "${categoryName}" ในระบบ`);
    } else if (category.type !== type) {
      const catTypeLabel = category.type === 'Income' ? 'รายรับ' : 'รายจ่าย';
      const rowTypeLabel = type === 'Income' ? 'รายรับ' : 'รายจ่าย';
      reasons.push(
        `หมวดหมู่ "${categoryName}" เป็น${catTypeLabel} แต่ระบุประเภทเป็น${rowTypeLabel}`
      );
    }
    if (isNaN(amount) || amount <= 0) {
      reasons.push(`จำนวนเงินไม่ถูกต้อง: "${amountStr}"`);
    }
    if (!parsed.isValid()) {
      reasons.push(`วันที่ไม่ถูกต้อง: "${dateStr}"`);
    }
    const valid = reasons.length === 0;

    return {
      transactionDate: parsed.isValid() ? parsed.format('YYYY-MM-DD') : dateStr,
      type,
      categoryName,
      amount,
      note,
      valid,
      reason: reasons.join(', '),
      categoryId: category?.id,
    };
  });

  step.value = 2;
}

async function handleImport() {
  const ok = await confirm(
    `ยืนยันนำเข้า ${validRows.value} รายการ? ไม่สามารถยกเลิกได้หลังนำเข้าแล้ว`,
    {
      confirmText: 'นำเข้า',
    }
  );
  if (!ok) return;

  await categoryStore.fetchCategories();

  const items: BulkImportItem[] = previewRows.value
    .filter((r) => r.valid)
    .map((r) => ({
      categoryId: r.categoryId,
      type: r.type,
      amount: r.amount,
      note: r.note || undefined,
      transactionDate: r.transactionDate,
    }));

  // Fire-and-forget: the job runs inside the store, independent of this
  // component, so the user is free to navigate away immediately.
  importJobStore.startImport(items);
  toast('เริ่มนำเข้าแล้ว ไปหน้าไหนก็ได้ ระบบจะแจ้งเตือนเมื่อเสร็จ', 'info');
  step.value = 3;
}

function resetImport() {
  step.value = 0;
  fileList.value = [];
  parsedRows.value = [];
  previewRows.value = [];
  headers.value = [];
  mapping.value = {
    transactionDate: '',
    type: '',
    categoryName: '',
    amount: '',
    note: '',
  };
}
</script>

<style scoped>
.upload-icon {
  color: var(--primary, #4a90d9);
  margin-bottom: 8px;
}
.upload-text {
  font-size: 1rem;
  font-weight: 500;
  margin-bottom: 4px;
}
.upload-hint {
  font-size: 0.85rem;
  color: var(--text-light, #888);
}
.step-desc {
  color: var(--text-light, #666);
  margin-bottom: 16px;
}
.step-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 24px;
}
.preview-summary {
  margin-top: 12px;
  font-size: 0.9rem;
  color: var(--text-light, #666);
}
.queued-panel {
  text-align: center;
  padding: 32px 16px;
}
.queued-icon {
  color: var(--primary, #4a90d9);
  margin-bottom: 12px;
}
.queued-icon.running {
  animation: spin 1.2s linear infinite;
}
.queued-icon.done {
  color: var(--el-color-success);
}
@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
.queued-title {
  font-size: 1.05rem;
  font-weight: 600;
  color: var(--text);
  margin-bottom: 8px;
}
.queued-hint {
  font-size: 0.85rem;
  color: var(--text-muted, #888);
  margin-bottom: 16px;
}
</style>
