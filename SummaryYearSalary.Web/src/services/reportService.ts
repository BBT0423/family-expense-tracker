import { httpClient } from '@/utils/httpClient';
import type { MonthlySummary, YearlySummary, CategorySummary } from '@/types';
import type { Result } from '@/types';

export const reportService = {
  async getMonthlySummary(month: number, year: number, memberId?: number) {
    return httpClient.get<Result<MonthlySummary>>('/api/reports/summary', {
      params: { month, year, memberId },
    });
  },

  async getYearlySummary(year: number, memberId?: number) {
    return httpClient.get<Result<YearlySummary>>('/api/reports/yearly', {
      params: { year, memberId },
    });
  },

  async getCategorySummary(month: number, year: number, memberId?: number) {
    return httpClient.get<Result<CategorySummary>>('/api/reports/by-category', {
      params: { month, year, memberId },
    });
  },

  async exportMonthly(month: number, year: number) {
    return httpClient.get<Blob>('/api/reports/export', {
      params: { month, year },
      responseType: 'blob',
    });
  },
};
