<template>
  <div>
    <PageHeader />

    <el-card shadow="never">
      <div class="import-stepper">
        <div
          v-for="cell in stepCells"
          :key="cell.label"
          class="import-stepper-cell"
          :class="{ done: cell.done, now: cell.now }"
        >
          <span class="import-stepper-circle">
            <el-icon v-if="cell.done" size="12"><Check /></el-icon>
            <template v-else>{{ STEP_LABELS.indexOf(cell.label) + 1 }}</template>
          </span>
          <div class="import-stepper-text">
            <div class="import-stepper-label">{{ cell.label }}</div>
            <div class="import-stepper-sub">{{ cell.sub }}</div>
          </div>
        </div>
      </div>

      <!-- Step 1: Upload -->
      <template v-if="step === 0">
        <div class="upload-screen" data-tour="import-upload">
          <div class="upload-intro">
            <div class="upload-intro-title">นำเข้าข้อมูล</div>
            <div class="upload-intro-sub">อัปโหลดไฟล์ แล้วเราจะจับคู่คอลัมน์ให้อัตโนมัติ</div>
          </div>
          <div v-if="parsingFile" class="upload-parsing">
            <el-icon size="30" class="upload-parsing-spinner"><Loading /></el-icon>
            <div class="upload-text">กำลังอ่านไฟล์ Excel...</div>
          </div>
          <el-upload
            v-else
            drag
            accept=".xlsx,.csv"
            :auto-upload="false"
            :limit="1"
            :on-change="onFileChange"
            :file-list="fileList"
            class="upload-dropzone"
          >
            <div class="upload-icon-box">
              <el-icon size="30" color="var(--primary-dark)"><UploadFilled /></el-icon>
            </div>
            <div class="upload-text">คลิกหรือลากไฟล์มาวางที่นี่</div>
            <div class="upload-hint">รองรับไฟล์ .xlsx และ .csv</div>
          </el-upload>
          <a class="upload-template-link" @click="downloadTemplate">ดาวน์โหลดไฟล์ตัวอย่าง</a>
        </div>
      </template>

      <!-- Step 2: Map + Preview (combined) -->
      <template v-if="step === 1">
        <div class="mp-topbar">
          <span class="mp-title">นำเข้าข้อมูล</span>
          <span class="mp-file-badge">
            <el-icon size="14" color="var(--success)"><CircleCheck /></el-icon>
            <span class="mp-file-name">{{ fileList[0]?.name }}</span>
            <a class="mp-file-change" @click="resetImport">เปลี่ยน</a>
          </span>
          <span class="mp-spacer" />
          <span class="mp-auto-map">จับคู่อัตโนมัติ {{ mappedColumnCount }} คอลัมน์</span>
        </div>

        <div class="mp-body">
          <div class="mp-status-row">
            <el-tag type="success" effect="light" round>ถูกต้อง {{ validRows }}</el-tag>
            <el-tag v-if="invalidRowsCount" type="danger" effect="light" round
              >ข้อผิดพลาด {{ invalidRowsCount }}</el-tag
            >
            <span class="mp-spacer" />
            <a class="mp-filter-toggle" @click="showOnlyInvalid = !showOnlyInvalid">
              {{ showOnlyInvalid ? 'แสดงทุกแถว' : 'แสดงเฉพาะที่ผิด' }}
            </a>
          </div>

          <el-auto-resizer class="mp-table" style="height: 400px">
            <template #default="{ height, width }">
              <el-table-v2
                :columns="previewColumns"
                :data="displayedRows"
                :width="width"
                :height="height"
                :row-height="40"
                :header-height="56"
                row-key="idx"
              />
            </template>
          </el-auto-resizer>

          <div v-if="invalidReasonList.length" class="mp-error-legend">
            <span v-for="e in invalidReasonList" :key="e.idx" class="mp-error-item">
              <b>#{{ e.idx }}</b> {{ e.reason }}
            </span>
          </div>
        </div>

        <div class="mp-footer">
          <span class="mp-footer-hint">
            <template v-if="invalidRowsCount"
              >แถวที่ผิด {{ invalidRowsCount }} แถวจะถูกข้าม</template
            >
          </span>
          <div class="step-actions" style="margin-top: 0">
            <el-button @click="resetImport">อัปโหลดใหม่</el-button>
            <el-button type="primary" :disabled="validRows === 0" @click="handleImport">
              นำเข้า {{ validRows }} รายการ
            </el-button>
          </div>
        </div>
      </template>

      <!-- Step 3: Done -->
      <template v-if="step === 2">
        <div class="queued-panel">
          <template v-if="importStatus === 'running'">
            <el-icon size="48" class="queued-icon running"><Loading /></el-icon>
            <p class="queued-title">
              กำลังนำเข้า {{ importCompleted }} รายการ ({{ importPercent }}%)
            </p>
            <el-progress :percentage="importPercent" style="max-width: 320px; margin: 0 auto" />
          </template>
          <template v-else-if="importStatus === 'done'">
            <div class="done-check">
              <el-icon size="34" color="#fff"><Check /></el-icon>
            </div>
            <p class="done-title">นำเข้าข้อมูลเสร็จแล้ว</p>
            <p class="done-sub">
              บันทึก {{ importCompleted }} รายการเรียบร้อย
              <template v-if="invalidRowsCount">· ข้าม {{ invalidRowsCount }} แถว</template>
            </p>
            <div class="done-stats">
              <div class="done-stat">
                <div class="done-stat-label">รายจ่าย</div>
                <div class="done-stat-value expense">฿{{ formatAmount(importedExpenseTotal) }}</div>
              </div>
              <div class="done-stat">
                <div class="done-stat-label">รายรับ</div>
                <div class="done-stat-value income">฿{{ formatAmount(importedIncomeTotal) }}</div>
              </div>
              <div class="done-stat">
                <div class="done-stat-label">หมวดหมู่ใหม่</div>
                <div class="done-stat-value">{{ createdCategoryCount }}</div>
              </div>
            </div>
            <div class="step-actions" style="justify-content: center">
              <el-button v-if="invalidRowsCount" @click="downloadInvalidRows"
                >ดาวน์โหลดแถวที่ข้าม</el-button
              >
              <el-button type="primary" @click="router.push('/pages/transactions')"
                >ดูรายการที่นำเข้า</el-button
              >
            </div>
            <a class="done-reset-link" @click="resetImport">นำเข้าไฟล์อื่น</a>
          </template>
          <template v-else>
            <el-icon size="48" class="queued-icon"><Upload /></el-icon>
            <p class="queued-title">เริ่มนำเข้าแล้ว — ดูสถานะได้ที่มุมขวาบน</p>
            <div class="step-actions" style="justify-content: center">
              <el-button @click="resetImport">นำเข้าไฟล์อื่น</el-button>
              <el-button type="primary" @click="router.push('/pages/transactions')"
                >ดูรายการที่นำเข้า</el-button
              >
            </div>
          </template>
        </div>
      </template>
    </el-card>

    <ImportProgressOverlay
      :visible="overlayVisible"
      :title="overlayTitle"
      :hint="overlayHint"
      :done="overlayDone"
      :total="overlayTotal"
      :unit-label="overlayUnitLabel"
      :steps="IMPORT_STEPS"
      :current-step="importSubStep"
      :finished="importStatus === 'done'"
      @close="showFinished = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, h } from 'vue';
import { storeToRefs } from 'pinia';
import { useRouter, onBeforeRouteLeave } from 'vue-router';
import { ElSelect, ElOption, ElTag } from 'element-plus';
import type { Column } from 'element-plus';
import { UploadFilled, Loading, CircleCheck, Upload, Check } from '@element-plus/icons-vue';
import * as XLSX from 'xlsx';
import PageHeader from '@/components/admin/PageHeader.vue';
import ImportProgressOverlay from '@/components/common/ImportProgressOverlay.vue';
import { useCategoryStore } from '@/stores/categoryStore';
import { useMemberStore } from '@/stores/memberStore';
import { useImportJobStore } from '@/stores/importJobStore';
import { categoryService } from '@/services/categoryService';
import { memberService } from '@/services/memberService';
import { useNotify } from '@/composables/useNotify';
import type { BulkImportItem, TransactionType } from '@/types';
import dayjs from 'dayjs';

const router = useRouter();
const categoryStore = useCategoryStore();
const memberStore = useMemberStore();
const importJobStore = useImportJobStore();
const {
  status: importStatus,
  percent: importPercent,
  completed: importCompleted,
  total: importTotal,
} = storeToRefs(importJobStore);
const { toast, confirm } = useNotify();

const step = ref(0);
const fileList = ref<any[]>([]);
const parsedRows = ref<Record<string, string>[]>([]);
const headers = ref<string[]>([]);
const showOnlyInvalid = ref(false);
const parsingFile = ref(false);

const mapping = ref<Record<string, string>>({
  transactionDate: '',
  type: '',
  categoryName: '',
  amount: '',
  note: '',
  memberName: '',
});

const mappingFields = [
  { key: 'transactionDate', label: 'วันที่ (DD/MM/YYYY ค.ศ. หรือ พ.ศ.)', required: true },
  { key: 'type', label: 'ประเภท (Income/Expense/รายรับ/รายจ่าย)', required: true },
  { key: 'categoryName', label: 'หมวดหมู่', required: true },
  { key: 'amount', label: 'จำนวนเงิน', required: true },
  { key: 'note', label: 'หมายเหตุ', required: false },
  { key: 'memberName', label: 'สมาชิก', required: false },
];

const FIELD_SHORT_LABELS: Record<string, string> = {
  transactionDate: 'วันที่',
  type: 'ประเภท',
  categoryName: 'หมวดหมู่',
  amount: 'จำนวนเงิน',
  note: 'หมายเหตุ',
  memberName: 'สมาชิก',
};

// Base width is a floor, not a target — flexGrow distributes the table's
// remaining width proportionally so longer-content columns (note, category)
// get more of it instead of every column stretching equally. flexShrink is
// forced to 0 on every column: el-table-v2 defaults it to 1, which let ALL
// columns (including short fixed-format ones like date/amount) shrink below
// their base width on narrow viewports — squeezing "2024-01-01" down to
// "2024-..." well before the table needed to. With shrink disabled the table
// scrolls horizontally on narrow screens instead, which keeps every column
// readable at its intended width.
const FIELD_COLUMN_CONFIG: Record<string, { width: number; flexGrow: number; flexShrink: number }> = {
  transactionDate: { width: 110, flexGrow: 0, flexShrink: 0 },
  type: { width: 90, flexGrow: 0, flexShrink: 0 },
  categoryName: { width: 170, flexGrow: 2, flexShrink: 0 },
  amount: { width: 110, flexGrow: 0, flexShrink: 0 },
  note: { width: 200, flexGrow: 3, flexShrink: 0 },
  memberName: { width: 130, flexGrow: 1, flexShrink: 0 },
};

const templateRows = [
  { col: 'A', name: 'วันที่', required: true, example: '01/04/2026' },
  { col: 'B', name: 'ประเภท', required: true, example: 'รายจ่าย' },
  { col: 'C', name: 'หมวดหมู่', required: true, example: 'อาหาร/เครื่องดื่ม' },
  { col: 'D', name: 'จำนวนเงิน', required: true, example: '150.00' },
  { col: 'E', name: 'หมายเหตุ', required: false, example: 'ข้าวเที่ยง' },
  { col: 'F', name: 'สมาชิก', required: false, example: 'แม่' },
];

function parseType(raw: string): TransactionType | null {
  const s = raw.toLowerCase();
  if (s.includes('income') || s.includes('รายรับ')) return 'Income';
  if (s.includes('expense') || s.includes('รายจ่าย')) return 'Expense';
  return null;
}

interface PreviewRow {
  idx: number;
  transactionDate: string;
  type: TransactionType;
  categoryName: string;
  amount: number;
  note: string;
  memberName: string;
  valid: boolean;
  invalidFields: Set<'date' | 'type' | 'amount' | 'category'>;
  reason: string;
  categoryId?: number;
  memberId?: number;
  isNewCategory: boolean;
  isNewMember: boolean;
}

// Reactive preview: recomputes automatically whenever the header-dropdown
// mapping changes, or once auto-created categories/members land in the
// stores — no manual "confirm mapping" step needed anymore.
const previewRows = computed<PreviewRow[]>(() => {
  const categories = categoryStore.categories;
  const members = memberStore.activeMembers;

  return parsedRows.value.map((row, i) => {
    const dateStr = row[mapping.value.transactionDate] ?? '';
    const categoryName = (row[mapping.value.categoryName] ?? '').trim();
    const amountStr = row[mapping.value.amount] ?? '';
    const note = row[mapping.value.note] ?? '';
    const memberName = (row[mapping.value.memberName] ?? '').trim();
    const rawType = row[mapping.value.type] ?? '';

    const member = memberName
      ? members.find((m) => m.name.toLowerCase() === memberName.toLowerCase())
      : undefined;
    const type = parseType(rawType) ?? 'Expense';
    const amount = parseFloat(amountStr.replace(/,/g, ''));
    // Categories with the same name but different types are distinct
    // entities (confirmed by the backend allowing both to be created) — a
    // name-only match would arbitrarily pick whichever type-variant happens
    // to come first, causing rows to fail type-mismatch validation even
    // when a category with the exact (name, type) pair actually exists.
    const category = categories.find((c) => c.name === categoryName && c.type === type);
    const categoryNameExists = categories.some((c) => c.name === categoryName);

    const rawParsed = dayjs(dateStr, ['DD/MM/YYYY', 'YYYY-MM-DD', 'MM/DD/YYYY']);
    // Excel from Thai users often carries Buddhist-era years (e.g. 16/09/2569)
    const parsed =
      rawParsed.isValid() && rawParsed.year() > 2400 ? rawParsed.subtract(543, 'year') : rawParsed;

    const invalidFields = new Set<'date' | 'type' | 'amount' | 'category'>();
    const reasons: string[] = [];
    let isNewCategory = false;

    if (!categoryName) {
      invalidFields.add('category');
      reasons.push('ไม่ได้ระบุหมวดหมู่');
    } else if (category) {
      // Exact (name, type) match already exists — valid as-is.
    } else if (!categoryNameExists) {
      // No category with this name under any type — will be auto-created
      // on import, not a blocking error.
      isNewCategory = true;
    } else {
      // The name exists, but only under a different type — a genuine
      // mismatch (e.g. wrong ประเภท typed in for an existing category),
      // not something auto-create should silently paper over.
      invalidFields.add('category');
      const existing = categories.find((c) => c.name === categoryName);
      const catTypeLabel = existing?.type === 'Income' ? 'รายรับ' : 'รายจ่าย';
      const rowTypeLabel = type === 'Income' ? 'รายรับ' : 'รายจ่าย';
      reasons.push(
        `หมวดหมู่ "${categoryName}" เป็น${catTypeLabel} แต่ระบุประเภทเป็น${rowTypeLabel}`
      );
    }

    if (parseType(rawType) === null) {
      invalidFields.add('type');
      reasons.push(`ประเภทต้องเป็น รายรับ / รายจ่าย: "${rawType}"`);
    }
    if (isNaN(amount) || amount <= 0) {
      invalidFields.add('amount');
      reasons.push(`จำนวนเงินไม่ถูกต้อง: "${amountStr}"`);
    }
    if (!parsed.isValid()) {
      invalidFields.add('date');
      reasons.push(`วันที่ไม่ถูกต้อง: "${dateStr}"`);
    }

    return {
      idx: i + 1,
      transactionDate: parsed.isValid() ? parsed.format('YYYY-MM-DD') : dateStr,
      type,
      categoryName,
      amount,
      note,
      memberName: member?.name ?? memberName,
      valid: invalidFields.size === 0,
      invalidFields,
      reason: reasons.join(', '),
      categoryId: category?.id,
      memberId: member?.id,
      isNewCategory,
      isNewMember: !!memberName && !member,
    };
  });
});

const validRows = computed(() => previewRows.value.filter((r) => r.valid).length);
const invalidRowsCount = computed(() => previewRows.value.length - validRows.value);
const displayedRows = computed(() =>
  showOnlyInvalid.value ? previewRows.value.filter((r) => !r.valid) : previewRows.value
);
const invalidReasonList = computed(() =>
  previewRows.value.filter((r) => !r.valid).map((r) => ({ idx: r.idx, reason: r.reason }))
);
const mappedColumnCount = computed(() => Object.values(mapping.value).filter((v) => v).length);

// el-table-v2 is virtualized (only renders visible rows into the DOM) but,
// unlike el-table, doesn't support per-column template slots — columns are
// declared as plain objects with cellRenderer/headerCellRenderer functions
// that return VNodes via h(), so this reproduces the same per-field markup
// the old template's v-if/v-else-if chain built.
function renderPreviewCell(fieldKey: string, row: PreviewRow) {
  // el-table-v2 rows are virtualized with a fixed row-height — unlike a
  // regular <el-table>, overflowing/wrapping text isn't clipped by the row,
  // it just spills visually into the (absolutely-positioned) row beneath
  // it. Every cell's text must therefore be forced to a single truncated
  // line itself rather than relying on the row to contain it.
  if (fieldKey === 'transactionDate') {
    return h(
      'span',
      { class: ['mp-cell-box', 'mp-cell-truncate', { bad: row.invalidFields.has('date') }] },
      row.transactionDate
    );
  }
  if (fieldKey === 'type') {
    return h(
      'span',
      { class: ['mp-cell-box', 'mp-cell-truncate', { bad: row.invalidFields.has('type') }] },
      row.type === 'Income' ? 'รายรับ' : 'รายจ่าย'
    );
  }
  if (fieldKey === 'categoryName') {
    return h('div', { class: 'mp-cell-flex' }, [
      h('span', { class: 'mp-cell-truncate' }, row.categoryName),
      row.isNewCategory
        ? h(ElTag, { size: 'small', type: 'primary', effect: 'light' }, () => 'หมวดหมู่ใหม่')
        : null,
    ]);
  }
  if (fieldKey === 'amount') {
    return h(
      'span',
      { class: ['mp-cell-box', 'mp-cell-truncate', { bad: row.invalidFields.has('amount') }] },
      String(row.amount)
    );
  }
  if (fieldKey === 'memberName') {
    return h('div', { class: 'mp-cell-flex' }, [
      h('span', { class: 'mp-cell-truncate' }, row.memberName),
      row.isNewMember
        ? h(ElTag, { size: 'small', type: 'primary', effect: 'light' }, () => 'สมาชิกใหม่')
        : null,
    ]);
  }
  return h('span', { class: 'mp-cell-truncate' }, row.note);
}

const previewColumns = computed<Column<PreviewRow>[]>(() => [
  {
    key: 'idx',
    dataKey: 'idx',
    title: 'แถว',
    width: 60,
    flexGrow: 0,
    flexShrink: 0,
    cellRenderer: ({ rowData }) =>
      h('span', { class: { 'mp-idx-bad': !rowData.valid } }, `#${rowData.idx}`),
  },
  ...mappingFields.map((f) => ({
    key: f.key,
    dataKey: f.key,
    title: FIELD_SHORT_LABELS[f.key],
    width: FIELD_COLUMN_CONFIG[f.key].width,
    flexGrow: FIELD_COLUMN_CONFIG[f.key].flexGrow,
    flexShrink: FIELD_COLUMN_CONFIG[f.key].flexShrink,
    headerCellRenderer: () =>
      h('div', { class: 'mp-map-header' }, [
        h('div', { class: 'mp-map-col' }, FIELD_SHORT_LABELS[f.key]),
        h(
          ElSelect,
          {
            modelValue: mapping.value[f.key],
            'onUpdate:modelValue': (v: string) => {
              mapping.value[f.key] = v;
            },
            size: 'small',
            class: 'mp-map-select',
            placeholder: 'เลือกคอลัมน์',
          },
          {
            default: () =>
              headers.value.map((col) => h(ElOption, { key: col, label: col, value: col })),
          }
        ),
      ]),
    cellRenderer: ({ rowData }: { rowData: PreviewRow }) => renderPreviewCell(f.key, rowData),
  })),
]);

function formatAmount(v: number) {
  return v.toLocaleString('th-TH', { minimumFractionDigits: 2 });
}

const STEP_LABELS = ['อัปโหลดไฟล์', 'แมป + ตรวจสอบ', 'นำเข้าสำเร็จ'];
const stepCells = computed(() => {
  const importDone = importStatus.value === 'done';
  return STEP_LABELS.map((label, i) => {
    const done = i < step.value || (i === 2 && importDone);
    const now = !done && i === step.value;
    let sub = '—';
    if (i === 0 && done) sub = `${fileList.value[0]?.name ?? ''} · ${parsedRows.value.length} แถว`;
    else if (i === 1 && done)
      sub = `จับคู่ ${mappedColumnCount.value} คอลัมน์ · ${validRows.value} ถูกต้อง / ${invalidRowsCount.value} ผิด`;
    else if (i === 2 && done) sub = `บันทึก ${importCompleted.value} รายการ`;
    else if (now) sub = 'กำลังทำ';
    return { label, done, now, sub };
  });
});

// Same flat-UI palette used by the app's own default categories (see
// CategorySeeder.cs) — picked at random per new category/member instead of
// one fixed color so auto-created rows stay visually distinguishable.
const AUTO_CREATE_COLOR_PALETTE = [
  '#27ae60',
  '#2ecc71',
  '#1abc9c',
  '#3498db',
  '#9b59b6',
  '#e67e22',
  '#e74c3c',
  '#c0392b',
  '#8e44ad',
  '#e84393',
  '#f39c12',
  '#2980b9',
  '#16a085',
  '#f1c40f',
  '#d35400',
];
function randomColor(): string {
  return AUTO_CREATE_COLOR_PALETTE[Math.floor(Math.random() * AUTO_CREATE_COLOR_PALETTE.length)];
}

// 0 = creating missing categories/members, 1 = brief refetch of the updated
// category/member lists to resolve their ids before building the bulk-save
// payload, 2 = actually saving records. Kept as 3 distinct steps (not 2) so
// the "0/0" transitional moment during step 1's refetch is honestly labeled
// "preparing data" instead of showing under "saving records" where it looks
// like the save itself is stuck at zero.
const IMPORT_STEPS = ['สร้างหมวดหมู่ / สมาชิกใหม่', 'เตรียมข้อมูลนำเข้า', 'บันทึกรายการ'];
const importingPrep = ref(false);
const importSubStep = ref(0);
const autoCreateDone = ref(0);
const autoCreateTotal = ref(0);
const createdCategoryCount = ref(0);

const importedExpenseTotal = computed(() =>
  previewRows.value
    .filter((r) => r.valid && r.type === 'Expense')
    .reduce((sum, r) => sum + r.amount, 0)
);
const importedIncomeTotal = computed(() =>
  previewRows.value
    .filter((r) => r.valid && r.type === 'Income')
    .reduce((sum, r) => sum + r.amount, 0)
);

const showFinished = ref(false);
watch(importStatus, (status) => {
  if (status === 'done') showFinished.value = true;
});

const overlayVisible = computed(
  () => importingPrep.value || importStatus.value === 'running' || showFinished.value
);
const overlayTitle = computed(() => 'กำลังนำเข้าข้อมูล…');
const overlayHint = computed(() => 'กรุณาอย่าปิดหรือเปลี่ยนหน้าจนกว่าจะเสร็จ');
const overlayDone = computed(() => {
  if (importSubStep.value === 0) return autoCreateDone.value;
  if (importSubStep.value === 1) return 0;
  return importCompleted.value;
});
const overlayTotal = computed(() => {
  if (importSubStep.value === 0) return autoCreateTotal.value;
  if (importSubStep.value === 1) return 0;
  return importTotal.value;
});
const overlayUnitLabel = computed(() => (importSubStep.value === 0 ? 'หมวดหมู่/สมาชิก' : 'รายการ'));

onBeforeRouteLeave(() => {
  if (!overlayVisible.value) return true;
  toast('กรุณารอให้เสร็จก่อนออกจากหน้านี้', 'warning');
  return false;
});

onMounted(() => {
  categoryStore.fetchCategories();
  memberStore.fetchMembers();
});

function onFileChange(file: any) {
  fileList.value = [file];
  parsingFile.value = true;
  const reader = new FileReader();
  reader.onload = (e) => {
    // Parsing a large file (XLSX.read + sheet_to_json + the reactive
    // recompute of previewRows once step advances) is synchronous and can
    // take a while — defer it one tick so the "parsingFile" spinner set
    // above actually gets painted before the main thread blocks on it.
    setTimeout(() => {
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
          if (lh.includes('สมาชิก') || lh.includes('member')) mapping.value.memberName = h;
        });
      }
      parsedRows.value = json;
      parsingFile.value = false;
      if (json.length > 0) step.value = 1;
    }, 0);
  };
  reader.readAsBinaryString(file.raw);
}

async function handleImport() {
  const ok = await confirm(
    `ยืนยันนำเข้า ${validRows.value} รายการ? ไม่สามารถยกเลิกได้หลังนำเข้าแล้ว`,
    { confirmText: 'นำเข้า' }
  );
  if (!ok) return;

  importingPrep.value = true;
  importSubStep.value = 0;
  autoCreateDone.value = 0;
  autoCreateTotal.value = 0;
  try {
    // Auto-create any category/member the file references that doesn't
    // exist yet, so a bulk import from another system doesn't require
    // pre-creating dozens of categories by hand first.
    const missingCategories = new Map<string, { name: string; type: TransactionType }>();
    const missingMembers = new Map<string, string>();
    for (const row of previewRows.value) {
      if (row.isNewCategory) {
        missingCategories.set(`${row.type}::${row.categoryName.toLowerCase()}`, {
          name: row.categoryName,
          type: row.type,
        });
      }
      if (row.isNewMember) {
        missingMembers.set(row.memberName.toLowerCase(), row.memberName);
      }
    }

    autoCreateTotal.value = missingCategories.size + missingMembers.size;
    createdCategoryCount.value = missingCategories.size;

    // Create everything concurrently instead of one-at-a-time — the store's
    // createCategory/createMember actions each also refetch the entire list
    // afterward, so doing this sequentially cost 2 round-trips per item.
    // Calling the services directly and refreshing the stores once at the
    // end turns O(n) sequential round-trips into a single batch.
    await Promise.all([
      ...Array.from(missingCategories.values()).map(async ({ name, type }) => {
        await categoryService.create({ name, type, icon: '💰', color: randomColor() });
        autoCreateDone.value++;
      }),
      ...Array.from(missingMembers.values()).map(async (name) => {
        await memberService.create({ name, color: randomColor() });
        autoCreateDone.value++;
      }),
    ]);
    // Flip to the "preparing data" step the instant creation itself is done,
    // so the overlay doesn't sit spinning at 100% under the "creating"
    // label while the category/member list refetch below (needed to
    // resolve categoryId/memberId) is in flight.
    importSubStep.value = 1;
    await Promise.all([categoryStore.fetchCategories(), memberStore.fetchMembers()]);
    // Only now does the overlay claim to be "saving records" — matching the
    // moment importJobStore.startImport() below actually starts the job.
    importSubStep.value = 2;
  } finally {
    importingPrep.value = false;
  }

  const items: BulkImportItem[] = previewRows.value
    .filter((r) => r.valid)
    .map((r) => ({
      categoryId: r.categoryId,
      memberId: r.memberId,
      type: r.type,
      amount: r.amount,
      note: r.note || undefined,
      transactionDate: r.transactionDate,
    }));

  // The job itself still runs inside the store (independent of this
  // component's lifecycle), but the overlay + onBeforeRouteLeave guard above
  // keep the user on this page until it finishes.
  importJobStore.startImport(items);
  step.value = 2;
}

function downloadTemplate() {
  const rows = [Object.fromEntries(templateRows.map((r) => [r.name, r.example]))];
  const ws = XLSX.utils.json_to_sheet(rows);
  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, 'Template');
  XLSX.writeFile(wb, 'แบบฟอร์มนำเข้าข้อมูล.xlsx');
}

function downloadInvalidRows() {
  const invalid = previewRows.value.filter((r) => !r.valid);
  if (!invalid.length) return;
  const rows = invalid.map((r) => ({
    วันที่: r.transactionDate,
    ประเภท: r.type === 'Income' ? 'รายรับ' : 'รายจ่าย',
    หมวดหมู่: r.categoryName,
    จำนวนเงิน: r.amount,
    หมายเหตุ: r.note,
    สมาชิก: r.memberName,
    สาเหตุ: r.reason,
  }));
  const ws = XLSX.utils.json_to_sheet(rows);
  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, 'แถวที่ผิด');
  XLSX.writeFile(wb, 'แถวที่ต้องแก้ไข.xlsx');
}

function resetImport() {
  step.value = 0;
  fileList.value = [];
  parsedRows.value = [];
  headers.value = [];
  showOnlyInvalid.value = false;
  createdCategoryCount.value = 0;
  mapping.value = {
    transactionDate: '',
    type: '',
    categoryName: '',
    amount: '',
    note: '',
    memberName: '',
  };
}
</script>

<style scoped>
.import-stepper {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
  background: var(--surface, #fff);
  border: 1px solid var(--border);
  border-radius: 14px;
  padding: 16px 22px;
  margin-bottom: 24px;
}
.import-stepper-cell {
  display: flex;
  gap: 10px;
  align-items: flex-start;
}
.import-stepper-circle {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  border: 2px solid var(--border);
  color: var(--text-muted);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
  flex: none;
  box-sizing: border-box;
}
.import-stepper-cell.now .import-stepper-circle {
  border-color: var(--primary);
  color: var(--primary-dark);
}
.import-stepper-cell.done .import-stepper-circle {
  background: var(--success);
  border-color: var(--success);
  color: #fff;
}
.import-stepper-label {
  font-size: 13px;
  font-weight: 700;
  color: var(--text-muted);
}
.import-stepper-cell.now .import-stepper-label,
.import-stepper-cell.done .import-stepper-label {
  color: var(--text);
}
.import-stepper-cell.now .import-stepper-label {
  color: var(--primary-dark);
}
.import-stepper-sub {
  font-size: 11.5px;
  color: var(--text-muted);
  margin-top: 1px;
}

.done-check {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: var(--success);
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 10px;
  box-shadow: 0 10px 24px rgba(22, 163, 74, 0.3);
}
.done-title {
  font-size: 19px;
  font-weight: 800;
  margin: 0 0 4px;
}
.done-sub {
  font-size: 13px;
  color: var(--text-muted);
  margin: 0 0 16px;
}
.done-stats {
  display: flex;
  justify-content: center;
  gap: 10px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}
.done-stat {
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 12px 18px;
  min-width: 120px;
}
.done-stat-label {
  font-size: 11.5px;
  color: var(--text-muted);
}
.done-stat-value {
  font-family: var(--font-mono);
  font-size: 16px;
  font-weight: 600;
  margin-top: 2px;
}
.done-stat-value.expense {
  color: var(--danger);
}
.done-stat-value.income {
  color: var(--success);
}
.done-reset-link {
  display: inline-block;
  margin-top: 14px;
  font-size: 12.5px;
  font-weight: 600;
  cursor: pointer;
  color: var(--primary-dark);
}

.upload-screen {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 18px;
  padding: 32px 0;
}
.upload-intro {
  text-align: center;
}
.upload-intro-title {
  font-size: 20px;
  font-weight: 800;
}
.upload-intro-sub {
  font-size: 13.5px;
  color: var(--text-light, #666);
  margin-top: 4px;
}
.upload-dropzone {
  width: 100%;
  max-width: 560px;
}
.upload-dropzone :deep(.el-upload-dragger) {
  padding: 40px 20px;
}
.upload-parsing {
  width: 100%;
  max-width: 560px;
  height: 200px;
  border: 2px dashed var(--border);
  border-radius: 18px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  color: var(--primary-dark);
}
.upload-parsing-spinner {
  animation: upload-parsing-spin 1s linear infinite;
}
@keyframes upload-parsing-spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
.upload-icon-box {
  width: 60px;
  height: 60px;
  border-radius: 16px;
  background: var(--primary-light, #cffafe);
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 12px;
}
.upload-text {
  font-size: 16px;
  font-weight: 700;
  margin-bottom: 4px;
}
.upload-hint {
  font-size: 0.85rem;
  color: var(--text-light, #888);
}
.upload-template-link {
  font-size: 12.5px;
  font-weight: 600;
  cursor: pointer;
  color: var(--primary-dark);
}

.mp-topbar {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 12px 4px;
  border-bottom: 1px solid var(--border);
  margin-bottom: 16px;
  flex-wrap: wrap;
}
.mp-title {
  font-size: 15px;
  font-weight: 800;
}
.mp-file-badge {
  display: flex;
  align-items: center;
  gap: 8px;
  background: var(--surface-dark, #f4f4f5);
  border-radius: 8px;
  padding: 6px 10px;
  font-size: 12.5px;
}
.mp-file-name {
  font-family: var(--font-mono);
}
.mp-file-change {
  font-weight: 600;
  cursor: pointer;
  color: var(--primary-dark);
}
.mp-spacer {
  flex: 1;
}
.mp-auto-map {
  font-size: 12.5px;
  color: var(--text-muted);
}

.mp-body {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.mp-status-row {
  display: flex;
  align-items: center;
  gap: 8px;
}
.mp-filter-toggle {
  font-size: 12.5px;
  font-weight: 600;
  cursor: pointer;
  color: var(--primary-dark);
}
.mp-table {
  border: 1px solid var(--border);
  border-radius: 6px;
  overflow: hidden;
}
.mp-table :deep(.el-table-v2__header-row) {
  background: var(--surface-dark, #f4f4f5);
}
.mp-table :deep(.el-table-v2__header-cell) {
  background: transparent;
  font-weight: 600;
  color: var(--text);
}
.mp-table :deep(.el-table-v2__row) {
  border-bottom: 1px solid var(--border);
}
.mp-table :deep(.el-table-v2__row-cell) {
  font-size: 13px;
  color: var(--text);
  overflow: hidden;
}
.mp-table :deep(.el-table-v2__row:hover) {
  background: var(--surface-dark, #f4f4f5);
}
/* These classes are applied to VNodes created imperatively via h() inside
   el-table-v2's cellRenderer/headerCellRenderer (required since el-table-v2
   doesn't support template slots per column) — such nodes are never part of
   this SFC's compiled template, so Vue's scoped-style attribute is never
   attached to them and plain scoped selectors silently never match. Every
   rule that needs to reach them must go through :deep() off a real
   template-rendered ancestor (.mp-table) instead. */
.mp-table :deep(.mp-idx-bad) {
  color: var(--danger);
  font-weight: 600;
}
.mp-table :deep(.mp-map-header) {
  display: flex;
  flex-direction: column;
  gap: 4px;
  /* el-table-v2's header cell is a row flexbox (align-items: center) — a
     flex item with no explicit width sizes to its content instead of
     stretching to fill the cell, which left the dropdown narrow even after
     the column itself grew via flexGrow. */
  width: 100%;
  min-width: 0;
}
.mp-table :deep(.mp-map-col) {
  font-size: 10.5px;
  color: var(--text-muted);
  font-family: var(--font-mono);
}
.mp-table :deep(.mp-map-select) {
  width: 100%;
  /* Columns can grow much wider than a column-name dropdown needs (e.g.
     "หมายเหตุ" stretches past 400px) — cap it so the select stays a normal
     control size instead of stretching absurdly to match the data column. */
  max-width: 170px;
}
.mp-table :deep(.mp-cell-box) {
  display: block;
  max-width: 100%;
  border: 1.5px solid transparent;
  border-radius: 6px;
  padding: 2px 6px;
  box-sizing: border-box;
  font-family: var(--font-mono);
  font-size: 12px;
}
.mp-table :deep(.mp-cell-box.bad) {
  border-color: var(--danger);
  background: rgba(220, 38, 38, 0.08);
  color: var(--danger);
}
.mp-table :deep(.mp-cell-truncate) {
  display: block;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.mp-table :deep(.mp-cell-flex) {
  display: flex;
  align-items: center;
  gap: 6px;
  min-width: 0;
}
.mp-table :deep(.mp-cell-flex .mp-cell-truncate) {
  flex: 1;
  min-width: 0;
}
.mp-error-legend {
  display: flex;
  gap: 18px;
  flex-wrap: wrap;
  font-size: 12px;
  color: var(--text-muted);
}
.mp-error-item b {
  color: var(--danger);
}

.mp-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid var(--border);
  flex-wrap: wrap;
}
.mp-footer-hint {
  font-size: 12.5px;
  color: var(--text-muted);
}

.step-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 24px;
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
</style>
