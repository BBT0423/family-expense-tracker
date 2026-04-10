import { httpClient } from '@/utils/httpClient';
import type {
  Transaction,
  CreateTransactionRequest,
  BulkImportItem,
  TransactionFilter,
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

  async bulkImport(items: BulkImportItem[]) {
    return httpClient.post<Result<{ importedCount: number }>>('/api/transactions/bulk-import', {
      items,
    });
  },
};
