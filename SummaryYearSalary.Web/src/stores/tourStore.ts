import { defineStore } from 'pinia';
import { ref } from 'vue';
import { SecureStorage } from '@/utils/storage';
import { STORAGE_TOUR_DONE } from '@/constants/storage';

export const useTourStore = defineStore('tour', () => {
  const completed = ref(SecureStorage.getItem(STORAGE_TOUR_DONE) === '1');
  const open = ref(false);

  function start() {
    open.value = true;
  }

  function finish() {
    open.value = false;
    completed.value = true;
    SecureStorage.setItem(STORAGE_TOUR_DONE, '1');
  }

  return { completed, open, start, finish };
});
