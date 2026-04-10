import { httpClient } from '@/utils/httpClient';
import type { Category, CreateCategoryRequest, TransactionType } from '@/types';
import type { Result } from '@/types';

export const categoryService = {
  async getAll(type?: TransactionType) {
    return httpClient.get<Result<Category[]>>('/api/categories', {
      params: type ? { type } : undefined,
    });
  },

  async create(payload: CreateCategoryRequest) {
    return httpClient.post<Result<Category>>('/api/categories', payload);
  },

  async update(id: number, payload: CreateCategoryRequest) {
    return httpClient.put<Result<Category>>(`/api/categories/${id}`, payload);
  },

  async remove(id: number) {
    return httpClient.delete<Result<boolean>>(`/api/categories/${id}`);
  },
};
