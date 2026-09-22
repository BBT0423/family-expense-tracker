import { httpClient } from '@/utils/httpClient';
import type { MonthlySummary, YearlySummary, CategorySummary } from '@/types';
import type { Result } from '@/types';

export const reportService = {
  async getMonthlySummary(month: number, year: number, scopeUserId?: string) {
    return httpClient.get<Result<MonthlySummary>>('/api/reports/summary', {
      params: { month, year, userId: scopeUserId },
    });
  },

  async getYearlySummary(year: number, scopeUserId?: string) {
    return httpClient.get<Result<YearlySummary>>('/api/reports/yearly', {
      params: { year, userId: scopeUserId },
    });
  },

  async getCategorySummary(month: number, year: number, scopeUserId?: string) {
    return httpClient.get<Result<CategorySummary>>('/api/reports/by-category', {
      params: { month, year, userId: scopeUserId },
    });
  },

  async exportMonthly(month: number, year: number, scopeUserId?: string) {
    return httpClient.get<Blob>('/api/reports/export', {
      params: { month, year, userId: scopeUserId },
      responseType: 'blob',
    });
  },
};
