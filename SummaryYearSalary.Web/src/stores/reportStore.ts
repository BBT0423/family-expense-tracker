import { defineStore } from 'pinia';
import { ref } from 'vue';
import { reportService } from '@/services/reportService';
import type { MonthlySummary, YearlySummary, CategorySummary } from '@/types';

export const useReportStore = defineStore('reports', () => {
  const monthlySummary = ref<MonthlySummary | null>(null);
  const yearlySummary = ref<YearlySummary | null>(null);
  const categorySummary = ref<CategorySummary | null>(null);
  const loading = ref(false);

  async function fetchMonthlySummary(month: number, year: number, scopeUserId?: string) {
    const { data } = await reportService.getMonthlySummary(month, year, scopeUserId);
    if (data?.isSuccess) monthlySummary.value = data.value ?? null;
  }

  async function fetchYearlySummary(year: number, scopeUserId?: string) {
    const { data } = await reportService.getYearlySummary(year, scopeUserId);
    if (data?.isSuccess) yearlySummary.value = data.value ?? null;
  }

  async function fetchCategorySummary(month: number, year: number, scopeUserId?: string) {
    const { data } = await reportService.getCategorySummary(month, year, scopeUserId);
    if (data?.isSuccess) categorySummary.value = data.value ?? null;
  }

  async function fetchAll(month: number, year: number, scopeUserId?: string) {
    loading.value = true;
    try {
      await Promise.all([
        fetchMonthlySummary(month, year, scopeUserId),
        fetchYearlySummary(year, scopeUserId),
        fetchCategorySummary(month, year, scopeUserId),
      ]);
    } finally {
      loading.value = false;
    }
  }

  return {
    monthlySummary,
    yearlySummary,
    categorySummary,
    loading,
    fetchMonthlySummary,
    fetchYearlySummary,
    fetchCategorySummary,
    fetchAll,
  };
});
