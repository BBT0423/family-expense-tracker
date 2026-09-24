import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { transactionService } from '@/services/transactionService';
import { useNotify } from '@/composables/useNotify';
import { toProblemDetails } from '@/utils/utilities';
import type { BulkImportItem } from '@/types';

const CHUNK_SIZE = 200;

export type ImportJobStatus = 'idle' | 'running' | 'done' | 'error';

export const useImportJobStore = defineStore('importJob', () => {
  const total = ref(0);
  const completed = ref(0);
  const status = ref<ImportJobStatus>('idle');
  const errorMessage = ref<string | null>(null);

  const percent = computed(() =>
    total.value ? Math.round((completed.value / total.value) * 100) : 0
  );

  // Runs the chunked upload as a store action (not tied to any component's
  // lifecycle) so it keeps going after the triggering view unmounts/navigates.
  async function startImport(items: BulkImportItem[]) {
    if (status.value === 'running') return;

    total.value = items.length;
    completed.value = 0;
    status.value = 'running';
    errorMessage.value = null;

    const { notify } = useNotify();

    for (let i = 0; i < items.length; i += CHUNK_SIZE) {
      const chunk = items.slice(i, i + CHUNK_SIZE);
      try {
        await transactionService.bulkImport(chunk);
        completed.value += chunk.length;
      } catch (e) {
        status.value = 'error';
        errorMessage.value = toProblemDetails(e).detail;
        notify(
          `นำเข้าล้มเหลวหลังจากสำเร็จไปแล้ว ${completed.value} รายการ — ${errorMessage.value}`,
          'error',
          { title: 'นำเข้าไม่สำเร็จ' }
        );
        return;
      }
    }

    status.value = 'done';
    notify(`นำเข้าสำเร็จ ${completed.value} รายการ`, 'success', { title: 'นำเข้าสำเร็จ' });
  }

  function reset() {
    total.value = 0;
    completed.value = 0;
    status.value = 'idle';
    errorMessage.value = null;
  }

  return { total, completed, status, errorMessage, percent, startImport, reset };
});
