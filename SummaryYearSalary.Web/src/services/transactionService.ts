import { httpClient } from '@/utils/httpClient';
import type {
  Transaction,
  CreateTransactionRequest,
  BulkImportItem,
  TransactionFilter,
  TransactionHistoryEntry,
} from '@/types';
import type { Result } from '@/types';
import type { PaginatedList } from '@/utils/paginatedList';

export const transactionService = {
  async getList(filter: TransactionFilter) {
    return httpClient.get<Result<PaginatedList<Transaction>>>('/api/transactions', {
      params: filter,
    });
  },

  async create(payload: CreateTransactionRequest) {
    return httpClient.post<Result<Transaction>>('/api/transactions', payload);
  },

  async update(id: number, payload: CreateTransactionRequest) {
    return httpClient.put<Result<Transaction>>(`/api/transactions/${id}`, payload);
  },

  async remove(id: number) {
    return httpClient.delete<Result<boolean>>(`/api/transactions/${id}`);
  },

  async getHistory(id: number) {
    return httpClient.get<Result<TransactionHistoryEntry[]>>(`/api/transactions/${id}/history`);
  },

  async bulkImport(items: BulkImportItem[]) {
    // Large imports (thousands of rows) can take a while server-side —
    // give this one request more room than the 60s default before it aborts.
    return httpClient.post<Result<number>>(
      '/api/transactions/bulk-import',
      { items },
      { timeout: 180000 }
    );
  },
};
