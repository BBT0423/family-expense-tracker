import { defineStore } from 'pinia';
import { ref } from 'vue';
import { reportService } from '@/services/reportService';
import type { MonthlySummary, YearlySummary, CategorySummary } from '@/types';

export const useReportStore = defineStore('reports', () => {
  const monthlySummary = ref<MonthlySummary | null>(null);
  const yearlySummary = ref<YearlySummary | null>(null);
  const categorySummary = ref<CategorySummary | null>(null);
  // Separate from monthlySummary so a page can show both the currently
  // selected month's totals AND an always-on all-time total side by side —
  // month=0, year=0 is this API's existing "no date filter" convention
  // (already used by the transaction list query), just applied here too.
  const allTimeSummary = ref<MonthlySummary | null>(null);
  // Previous calendar month's summary — used for the Dashboard's "เทียบเดือนก่อน"
  // comparison stat lines. Separate ref so it doesn't clobber monthlySummary.
  const previousMonthSummary = ref<MonthlySummary | null>(null);
  const loading = ref(false);

  async function fetchMonthlySummary(month: number, year: number, memberId?: number) {
    const { data } = await reportService.getMonthlySummary(month, year, memberId);
    if (data?.isSuccess) monthlySummary.value = data.value ?? null;
  }

  async function fetchAllTimeSummary(memberId?: number) {
    const { data } = await reportService.getMonthlySummary(0, 0, memberId);
    if (data?.isSuccess) allTimeSummary.value = data.value ?? null;
  }

  async function fetchPreviousMonthSummary(month: number, year: number, memberId?: number) {
    const prevMonth = month === 1 ? 12 : month - 1;
    const prevYear = month === 1 ? year - 1 : year;
    const { data } = await reportService.getMonthlySummary(prevMonth, prevYear, memberId);
    if (data?.isSuccess) previousMonthSummary.value = data.value ?? null;
  }

  async function fetchYearlySummary(year: number, memberId?: number) {
    const { data } = await reportService.getYearlySummary(year, memberId);
    if (data?.isSuccess) yearlySummary.value = data.value ?? null;
  }

  async function fetchCategorySummary(month: number, year: number, memberId?: number) {
    const { data } = await reportService.getCategorySummary(month, year, memberId);
    if (data?.isSuccess) categorySummary.value = data.value ?? null;
  }

  async function fetchAll(month: number, year: number, memberId?: number) {
    loading.value = true;
    try {
      await Promise.all([
        fetchMonthlySummary(month, year, memberId),
        fetchYearlySummary(year, memberId),
        fetchCategorySummary(month, year, memberId),
      ]);
    } finally {
      loading.value = false;
    }
  }

  return {
    monthlySummary,
    yearlySummary,
    categorySummary,
    allTimeSummary,
    previousMonthSummary,
    loading,
    fetchMonthlySummary,
    fetchYearlySummary,
    fetchCategorySummary,
    fetchAllTimeSummary,
    fetchPreviousMonthSummary,
    fetchAll,
  };
});
