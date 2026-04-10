import { httpClient } from '@/utils/httpClient';
import type { MonthlySummary, YearlySummary, CategorySummary, MemberSummary } from '@/types';
import type { Result } from '@/types';

export const reportService = {
  async getMonthlySummary(month: number, year: number) {
    return httpClient.get<Result<MonthlySummary>>('/api/reports/summary', {
      params: { month, year },
    });
  },

  async getYearlySummary(year: number) {
    return httpClient.get<Result<YearlySummary>>('/api/reports/yearly', { params: { year } });
  },

  async getCategorySummary(month: number, year: number) {
    return httpClient.get<Result<CategorySummary>>('/api/reports/by-category', {
      params: { month, year },
    });
  },

  async getMemberSummary(month: number, year: number) {
    return httpClient.get<Result<MemberSummary>>('/api/reports/by-member', {
      params: { month, year },
    });
  },

  async exportMonthly(month: number, year: number) {
    return httpClient.get<Blob>('/api/reports/export', {
      params: { month, year },
      responseType: 'blob',
    });
  },
};
