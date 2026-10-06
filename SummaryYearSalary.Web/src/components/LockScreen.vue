<template>
  <div v-if="isLocked" class="lock-screen-overlay">
    <div class="lock-screen-wrap">
      <div class="ep-card ep-card--dashed-warn">
        <div class="ep-card__body" style="text-align: center">
          <!-- Badge -->
          <div style="margin-bottom: 16px">
            <span class="ep-lock-badge">⏱ หน้าจอถูกล็อกอัตโนมัติ</span>
          </div>

          <!-- Avatar -->
          <div class="ep-avatar" style="width: 72px; height: 72px; font-size: 28px">
            <img
              v-if="user?.avatar"
              :src="user.avatar"
              :alt="user.name"
              style="width: 100%; height: 100%; object-fit: cover; border-radius: 50%"
            />
            <span v-else>{{ userInitials }}</span>
          </div>

          <!-- User info -->
          <div class="ep-h1" style="font-size: 18px; margin-bottom: 4px">
            {{ user?.name || 'User' }}
          </div>
          <div style="font-size: 12px; color: var(--ep-text-3); margin-bottom: 20px">
            ข้อมูลถูกซ่อนไว้ชั่วคราวเพื่อความเป็นส่วนตัว
          </div>

          <button class="ep-btn ep-btn--primary" @click="handleUnlock">แตะเพื่อดูต่อ</button>

          <!-- Lock time -->
          <div
            style="
              margin-top: 16px;
              padding-top: 14px;
              border-top: 1px solid var(--ep-border-light);
              font-size: 12px;
              color: var(--ep-text-4);
            "
          >
            ล็อกเมื่อ {{ lockedTimeText }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onUnmounted } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { useLockScreenStore } from '@/stores/lockScreenStore';

const props = defineProps<{
  isLocked: boolean;
  onUnlock: () => void;
}>();

const authStore = useAuthStore();
const lockScreenStore = useLockScreenStore();

const lockedTimeText = ref('เมื่อกี้');

const user = computed(() => authStore.user);

const userInitials = computed(() => {
  if (!user.value?.name) return 'U';
  const names = user.value.name.split(' ');
  if (names.length >= 2) return (names[0][0] + names[1][0]).toUpperCase();
  return user.value.name.substring(0, 2).toUpperCase();
});

let timeUpdateInterval: ReturnType<typeof setInterval> | null = null;

const updateLockedTime = () => {
  if (!lockScreenStore.lockTime) {
    lockedTimeText.value = 'เมื่อกี้';
    return;
  }
  const diff = Date.now() - lockScreenStore.lockTime;
  const minutes = Math.floor(diff / 60000);
  if (minutes < 1) lockedTimeText.value = 'เมื่อกี้';
  else if (minutes === 1) lockedTimeText.value = '1 นาทีที่แล้ว';
  else if (minutes < 60) lockedTimeText.value = `${minutes} นาทีที่แล้ว`;
  else {
    const hours = Math.floor(minutes / 60);
    lockedTimeText.value = hours === 1 ? '1 ชั่วโมงที่แล้ว' : `${hours} ชั่วโมงที่แล้ว`;
  }
};

watch(
  () => props.isLocked,
  (locked) => {
    if (locked) {
      lockScreenStore.lock();
      updateLockedTime();
      timeUpdateInterval = setInterval(updateLockedTime, 30000);
    } else {
      lockScreenStore.unlock();
      if (timeUpdateInterval) {
        clearInterval(timeUpdateInterval);
        timeUpdateInterval = null;
      }
    }
  },
  { immediate: true }
);

onUnmounted(() => {
  if (timeUpdateInterval) clearInterval(timeUpdateInterval);
});

// No credential check — this is just a privacy overlay while idle, not a
// real auth gate (there's nothing to log back into as a "different user").
const handleUnlock = () => {
  props.onUnlock();
};
</script>

<style scoped>
.lock-screen-overlay {
  position: fixed;
  inset: 0;
  background: linear-gradient(135deg, #0e7490 0%, #0891b2 100%);
  z-index: 10000;
  display: flex;
  align-items: center;
  justify-content: center;
  animation: lockFadeIn 0.3s ease-in;
}

@keyframes lockFadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

.lock-screen-wrap {
  width: 100%;
  max-width: 400px;
  padding: 20px;
}

/* ep-card dashed-warn header color override */
.ep-card--dashed-warn .ep-card__header {
  background: #fdf6ec;
  color: #9a6400;
}

@media (max-width: 480px) {
  .lock-screen-wrap {
    padding: 12px;
  }
}
</style>
