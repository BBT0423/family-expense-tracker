import { defineStore } from 'pinia';
import { ref } from 'vue';
import { reportService } from '@/services/reportService';
import type { MonthlySummary, YearlySummary, CategorySummary, MemberSummary } from '@/types';

export const useReportStore = defineStore('reports', () => {
  const monthlySummary = ref<MonthlySummary | null>(null);
  const yearlySummary = ref<YearlySummary | null>(null);
  const categorySummary = ref<CategorySummary | null>(null);
  const memberSummary = ref<MemberSummary | null>(null);
  const loading = ref(false);

  async function fetchMonthlySummary(month: number, year: number) {
    const { data } = await reportService.getMonthlySummary(month, year);
    if (data?.isSuccess) monthlySummary.value = data.value ?? null;
  }

  async function fetchYearlySummary(year: number) {
    const { data } = await reportService.getYearlySummary(year);
    if (data?.isSuccess) yearlySummary.value = data.value ?? null;
  }

  async function fetchCategorySummary(month: number, year: number) {
    const { data } = await reportService.getCategorySummary(month, year);
    if (data?.isSuccess) categorySummary.value = data.value ?? null;
  }

  async function fetchMemberSummary(month: number, year: number) {
    const { data } = await reportService.getMemberSummary(month, year);
    if (data?.isSuccess) memberSummary.value = data.value ?? null;
  }

  async function fetchAll(month: number, year: number) {
    loading.value = true;
    try {
      await Promise.all([
        fetchMonthlySummary(month, year),
        fetchYearlySummary(year),
        fetchCategorySummary(month, year),
        fetchMemberSummary(month, year),
      ]);
    } finally {
      loading.value = false;
    }
  }

  return {
    monthlySummary,
    yearlySummary,
    categorySummary,
    memberSummary,
    loading,
    fetchMonthlySummary,
    fetchYearlySummary,
    fetchCategorySummary,
    fetchMemberSummary,
    fetchAll,
  };
});
