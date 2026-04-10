<template>
  <div class="ui-test-page">
    <PageHeader />

    <!-- ── useNotify() ── EP Native ─────────────────────────────────── -->
    <div class="card mb-3">
      <div class="card-header">
        <h3 class="card-title">useNotify() — Element Plus Native</h3>
        <p class="card-subtitle">swappable abstraction — เปลี่ยน library แค่ไฟล์เดียว</p>
      </div>

      <!-- Toast -->
      <div class="test-section">
        <h4 class="section-title">Toast <code>toast(message, type)</code></h4>
        <div class="btn-row">
          <el-button type="success" @click="notify.toast('บันทึกสำเร็จแล้ว', 'success')">
            <el-icon><Check /></el-icon> Success
          </el-button>
          <el-button type="danger" @click="notify.toast('เกิดข้อผิดพลาด กรุณาลองใหม่', 'error')">
            <el-icon><Close /></el-icon> Error
          </el-button>
          <el-button type="warning" @click="notify.toast('กรุณาตรวจสอบข้อมูลอีกครั้ง', 'warning')">
            <el-icon><Warning /></el-icon> Warning
          </el-button>
          <el-button @click="notify.toast('ข้อมูลถูกอัปเดตแล้ว', 'info')">
            <el-icon><InfoFilled /></el-icon> Info
          </el-button>
        </div>
      </div>

      <!-- Notify -->
      <div class="test-section">
        <h4 class="section-title">Notification <code>notify(message, type, options)</code></h4>
        <div class="btn-row">
          <el-button
            type="success"
            @click="notify.notify('โหลดข้อมูลเรียบร้อย', 'success', { title: 'สำเร็จ' })"
          >
            <el-icon><BellFilled /></el-icon> Success
          </el-button>
          <el-button
            type="danger"
            @click="notify.notify('ไม่สามารถเชื่อมต่อเซิร์ฟเวอร์', 'error', { title: 'ผิดพลาด' })"
          >
            <el-icon><BellFilled /></el-icon> Error
          </el-button>
          <el-button
            type="warning"
            @click="
              notify.notify('Session ใกล้หมดอายุ', 'warning', { title: 'แจ้งเตือน', duration: 0 })
            "
          >
            <el-icon><BellFilled /></el-icon> Warning (persistent)
          </el-button>
          <el-button
            @click="notify.notify('มีการอัปเดตใหม่', 'info', { position: 'bottom-right' })"
          >
            <el-icon><BellFilled /></el-icon> Bottom-right
          </el-button>
        </div>
      </div>

      <!-- Confirm -->
      <div class="test-section">
        <h4 class="section-title">Confirm <code>await confirm(message, options)</code></h4>
        <div class="btn-row">
          <el-button type="primary" @click="testConfirm">
            <el-icon><QuestionFilled /></el-icon> Confirm (normal)
          </el-button>
          <el-button type="danger" @click="testDangerConfirm">
            <el-icon><Delete /></el-icon> Confirm (danger)
          </el-button>
        </div>
        <div
          v-if="confirmResult !== null"
          class="result-badge"
          :class="confirmResult ? 'result-ok' : 'result-cancel'"
        >
          ผลลัพธ์: {{ confirmResult ? '✅ ยืนยัน (true)' : '❌ ยกเลิก (false)' }}
        </div>
      </div>

      <!-- Alert -->
      <div class="test-section">
        <h4 class="section-title">Alert <code>await alert(message, type)</code></h4>
        <div class="btn-row">
          <el-button type="success" @click="notify.alert('ดำเนินการสำเร็จ', 'success', 'สำเร็จ')">
            Alert Success
          </el-button>
          <el-button
            type="warning"
            @click="notify.alert('Session หมดอายุ กรุณา login ใหม่', 'warning', 'แจ้งเตือน')"
          >
            Alert Warning
          </el-button>
          <el-button
            type="danger"
            @click="notify.alert('ไม่มีสิทธิ์เข้าถึง', 'error', 'ข้อผิดพลาด')"
          >
            Alert Error
          </el-button>
        </div>
      </div>

      <!-- Loading -->
      <div class="test-section">
        <h4 class="section-title">Loading <code>loading(text)</code></h4>
        <div class="btn-row">
          <el-button type="primary" :loading="isLoading" @click="testLoading(2000, 'กำลังโหลด...')">
            Loading 2s
          </el-button>
          <el-button
            type="primary"
            :loading="isLoading"
            @click="testLoading(4000, 'กำลังบันทึกข้อมูล...')"
          >
            Loading 4s (custom text)
          </el-button>
        </div>
      </div>
    </div>

    <!-- ── useGlobalUi() ── Custom Pro* components ──────────────────── -->
    <div class="card mb-3">
      <div class="card-header">
        <h3 class="card-title">useGlobalUi() — Custom Components</h3>
        <p class="card-subtitle">ProMessage / ProAlert / ProConfirm / ProLoading</p>
      </div>

      <!-- ProMessage -->
      <div class="test-section">
        <h4 class="section-title">Message Banner <code>showMessage()</code></h4>
        <ProMessage />
        <div class="btn-row">
          <el-button type="success" @click="globalUi.showMessage('บันทึกสำเร็จ', 'success')">
            Success
          </el-button>
          <el-button type="danger" @click="globalUi.showMessage('เกิดข้อผิดพลาด', 'error')">
            Error
          </el-button>
          <el-button type="warning" @click="globalUi.showMessage('กรุณาตรวจสอบข้อมูล', 'warning')">
            Warning
          </el-button>
          <el-button @click="globalUi.showMessage('ข้อมูล N/A', 'info')"> Info </el-button>
          <el-button plain @click="globalUi.hideMessage()"> Hide </el-button>
        </div>
      </div>

      <!-- ProAlert -->
      <div class="test-section">
        <h4 class="section-title">Alert Modal <code>showAlert()</code></h4>
        <div class="btn-row">
          <el-button type="success" @click="globalUi.showAlert('ดำเนินการสำเร็จ', 'success')">
            Alert Success
          </el-button>
          <el-button type="danger" @click="globalUi.showAlert('เกิดข้อผิดพลาดร้ายแรง', 'error')">
            Alert Error
          </el-button>
          <el-button
            type="warning"
            @click="globalUi.showAlert('คำเตือน: ข้อมูลไม่ครบถ้วน', 'warning')"
          >
            Alert Warning
          </el-button>
        </div>
      </div>

      <!-- ProConfirm -->
      <div class="test-section">
        <h4 class="section-title">Confirm Modal <code>showConfirm()</code></h4>
        <div class="btn-row">
          <el-button type="primary" @click="testGlobalConfirm"> Global Confirm </el-button>
        </div>
        <div
          v-if="globalConfirmResult !== null"
          class="result-badge"
          :class="globalConfirmResult ? 'result-ok' : 'result-cancel'"
        >
          ผลลัพธ์: {{ globalConfirmResult ? '✅ ยืนยัน' : '❌ ยกเลิก' }}
        </div>
      </div>

      <!-- ProLoading -->
      <div class="test-section">
        <h4 class="section-title">Full-screen Loading <code>showLoading()</code></h4>
        <div class="btn-row">
          <el-button type="primary" @click="testGlobalLoading"> Loading 2s </el-button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useNotify } from '@/composables/useNotify';
import { useGlobalUi } from '@/composables/useGlobalUi';

const notify = useNotify();
const globalUi = useGlobalUi();

// ── useNotify confirm state ────────────────────────────────────────────
const confirmResult = ref<boolean | null>(null);
const isLoading = ref(false);

async function testConfirm() {
  confirmResult.value = null;
  confirmResult.value = await notify.confirm('ต้องการดำเนินการต่อหรือไม่?', {
    title: 'ยืนยันการดำเนินการ',
    confirmText: 'ดำเนินการ',
    cancelText: 'ยกเลิก',
  });
}

async function testDangerConfirm() {
  confirmResult.value = null;
  confirmResult.value = await notify.confirm('ข้อมูลที่ลบจะไม่สามารถกู้คืนได้', {
    title: 'ยืนยันการลบ',
    confirmText: 'ลบ',
    cancelText: 'ยกเลิก',
    type: 'error',
    dangerConfirm: true,
  });
}

async function testLoading(duration: number, text: string) {
  if (isLoading.value) return;
  isLoading.value = true;
  const loader = notify.loading(text);
  setTimeout(() => {
    loader.close();
    isLoading.value = false;
    notify.toast(`Loading ปิดแล้ว (${duration / 1000}s)`, 'success');
  }, duration);
}

// ── useGlobalUi confirm state ──────────────────────────────────────────
const globalConfirmResult = ref<boolean | null>(null);

function testGlobalConfirm() {
  globalConfirmResult.value = null;
  globalUi.showConfirm('ต้องการดำเนินการต่อหรือไม่?', {
    title: 'ยืนยัน',
    confirmText: 'ยืนยัน',
    cancelText: 'ยกเลิก',
    onConfirm: () => {
      globalConfirmResult.value = true;
    },
    onCancel: () => {
      globalConfirmResult.value = false;
    },
  });
}

async function testGlobalLoading() {
  globalUi.showLoading();
  setTimeout(() => {
    globalUi.hideLoading();
    notify.toast('Loading ปิดแล้ว', 'success');
  }, 2000);
}
</script>

<style scoped>
.ui-test-page {
  max-width: 960px;
}

.test-section {
  padding: 1.25rem 0;
  border-bottom: 1px solid var(--border);
}

.test-section:last-child {
  border-bottom: none;
  padding-bottom: 0;
}

.section-title {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--text);
  margin: 0 0 0.75rem 0;
}

.section-title code {
  font-size: 0.8rem;
  background: var(--surface-dark);
  padding: 0.1em 0.4em;
  border-radius: 4px;
  color: var(--primary-light);
  font-weight: 400;
}

.btn-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
}

.result-badge {
  margin-top: 0.75rem;
  display: inline-block;
  padding: 0.35rem 0.85rem;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 600;
}

.result-ok {
  background: var(--success-light);
  color: var(--success-dark);
}

.result-cancel {
  background: rgba(220, 38, 38, 0.1);
  color: var(--danger);
}
</style>
