import { httpClient } from '@/utils/httpClient';
import type { Member, CreateMemberRequest, UpdateMemberRequest } from '@/types';
import type { Result } from '@/types';

export const memberService = {
  async getAll() {
    return httpClient.get<Result<Member[]>>('/api/members');
  },

  async create(payload: CreateMemberRequest) {
    return httpClient.post<Result<Member>>('/api/members', payload);
  },

  async update(id: number, payload: UpdateMemberRequest) {
    return httpClient.put<Result<Member>>(`/api/members/${id}`, payload);
  },

  async remove(id: number) {
    return httpClient.delete<Result<boolean>>(`/api/members/${id}`);
  },
};
