<template>
  <header class="header">
    <div class="header-left">
      <button
        type="button"
        class="menu-btn"
        aria-label="Open menu"
        @click="$emit('toggle-sidebar')"
      >
        <el-icon size="20"><Menu /></el-icon>
      </button>
      <nav class="header-crumb" aria-label="Breadcrumb">
        Family Expense
        <span v-if="pageTitle" class="header-crumb-current">/ {{ pageTitle }}</span>
      </nav>
    </div>

    <div class="header-right">
      <!-- Help: replay tour / open manual -->
      <el-dropdown trigger="click" placement="bottom-end">
        <el-button text circle aria-label="ช่วยเหลือ" data-tour="help-btn">
          <el-icon size="18"><QuestionFilled /></el-icon>
        </el-button>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item @click="replayTour">เริ่มทัวร์แนะนำใหม่</el-dropdown-item>
            <el-dropdown-item @click="router.push('/pages/help')">คู่มือการใช้งาน</el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>

      <!-- Theme toggle -->
      <el-tooltip :content="isDark ? 'Light mode' : 'Dark mode'" placement="bottom">
        <el-button text circle @click="toggleTheme()">
          <el-icon size="18">
            <Sunny v-if="isDark" />
            <Moon v-else />
          </el-icon>
        </el-button>
      </el-tooltip>

      <!-- Import job progress -->
      <el-dropdown v-if="importStatus !== 'idle'" trigger="click" placement="bottom-end">
        <el-button text circle>
          <el-progress
            v-if="importStatus === 'running'"
            type="circle"
            :percentage="importPercent"
            :width="22"
            :stroke-width="3"
            :show-text="false"
          />
          <el-icon v-else-if="importStatus === 'done'" size="18" color="var(--el-color-success)"
            ><CircleCheck
          /></el-icon>
          <el-icon v-else size="18" color="var(--el-color-danger)"><CircleClose /></el-icon>
        </el-button>
        <template #dropdown>
          <div class="notif-panel import-panel">
            <div class="notif-panel-header">
              <span>สถานะนำเข้าข้อมูล</span>
            </div>
            <div class="import-panel-body">
              <template v-if="importStatus === 'running'">
                <div class="import-status-text">
                  กำลังนำเข้า {{ importCompleted }}/{{ importTotal }} รายการ ({{ importPercent }}%)
                </div>
                <el-progress :percentage="importPercent" />
              </template>
              <template v-else-if="importStatus === 'done'">
                <div class="import-status-text">นำเข้าสำเร็จ {{ importCompleted }} รายการ</div>
                <el-button size="small" text @click="importJobStore.reset()">ปิด</el-button>
              </template>
              <template v-else-if="importStatus === 'error'">
                <div class="import-status-text error">
                  นำเข้าล้มเหลวหลังจากสำเร็จไปแล้ว {{ importCompleted }} รายการ
                </div>
                <div class="import-status-detail">{{ importErrorMessage }}</div>
                <el-button size="small" text @click="importJobStore.reset()">ปิด</el-button>
              </template>
            </div>
          </div>
        </template>
      </el-dropdown>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed, inject } from 'vue';
import { storeToRefs } from 'pinia';
import { useRoute, useRouter } from 'vue-router';
import { CircleCheck, CircleClose, QuestionFilled } from '@element-plus/icons-vue';
import { useImportJobStore } from '@/stores/importJobStore';
import { useTourStore } from '@/stores/tourStore';

defineEmits(['toggle-sidebar']);

const route = useRoute();
const router = useRouter();
const tourStore = useTourStore();

function replayTour() {
  tourStore.start();
}

const importJobStore = useImportJobStore();
const {
  status: importStatus,
  percent: importPercent,
  completed: importCompleted,
  total: importTotal,
  errorMessage: importErrorMessage,
} = storeToRefs(importJobStore);

const isDark = inject<{ value: boolean }>('isDark');
const toggleTheme = inject<() => void>('toggleTheme', () => {});

const pageTitle = computed(() => route.meta?.title as string | undefined);
</script>

<style scoped>
.header-crumb {
  font-size: 13px;
  color: var(--text-muted);
  white-space: nowrap;
}

.header-crumb-current {
  color: var(--text);
  font-weight: 600;
}

.header-right {
  gap: 4px;
}

.notif-panel {
  width: 320px;
  max-height: 420px;
  display: flex;
  flex-direction: column;
}

.import-panel-body {
  padding: 12px 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.import-status-text {
  font-size: 0.85rem;
  color: var(--text);
}

.import-status-text.error {
  color: var(--el-color-danger);
}

.import-status-detail {
  font-size: 0.78rem;
  color: var(--text-muted);
}

.notif-panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--text);
  border-bottom: 1px solid var(--border);
}
</style>
