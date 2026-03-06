import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useLockScreenStore = defineStore('lockScreen', () => {
  const isLocked = ref(false);
  const lockTime = ref<number | null>(null);

  function lock() {
    isLocked.value = true;
    lockTime.value = Date.now();
  }

  function unlock() {
    isLocked.value = false;
    lockTime.value = null;
  }

  return {
    isLocked,
    lockTime,
    lock,
    unlock,
  };
});
