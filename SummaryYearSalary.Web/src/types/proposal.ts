import type { TransactionType } from './budget';

export interface TransactionProposal {
  id: number;
  proposedByName: string;
  type: TransactionType;
  amount: number;
  categoryId: number;
  categoryName: string;
  categoryIcon: string;
  categoryColor: string;
  note?: string;
  transactionDate: string;
  createdAt: string;
}

export interface CreateTransactionProposalRequest {
  recipientEmail: string;
  type: TransactionType;
  amount: number;
  categoryId: number;
  note?: string;
  transactionDate: string;
}

export interface AcceptTransactionProposalRequest {
  categoryId?: number;
}

export interface UserLookup {
  displayName: string;
  email: string;
}
