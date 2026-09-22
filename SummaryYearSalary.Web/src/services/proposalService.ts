import { httpClient } from '@/utils/httpClient';
import type {
  AcceptTransactionProposalRequest,
  CreateTransactionProposalRequest,
  Result,
  TransactionProposal,
  UserLookup,
} from '@/types';

export const proposalService = {
  async create(payload: CreateTransactionProposalRequest) {
    return httpClient.post<Result<number>>('/api/transactionproposals', payload);
  },

  async getIncoming() {
    return httpClient.get<Result<TransactionProposal[]>>('/api/transactionproposals/incoming');
  },

  async accept(id: number, payload: AcceptTransactionProposalRequest) {
    return httpClient.post<Result<boolean>>(`/api/transactionproposals/${id}/accept`, payload);
  },

  async reject(id: number) {
    return httpClient.post<Result<boolean>>(`/api/transactionproposals/${id}/reject`);
  },

  async lookupUser(email: string) {
    return httpClient.get<Result<UserLookup>>('/api/users/lookup', { params: { email } });
  },
};
