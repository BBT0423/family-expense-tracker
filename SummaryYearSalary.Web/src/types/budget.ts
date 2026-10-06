export type TransactionType = 'Income' | 'Expense';

export interface Category {
  id: number;
  name: string;
  type: TransactionType;
  icon: string;
  color: string;
  isDefault: boolean;
}

export interface Transaction {
  id: number;
  categoryId: number;
  categoryName: string;
  categoryIcon: string;
  categoryColor: string;
  type: TransactionType;
  amount: number;
  note?: string;
  transactionDate: string;
  createdAt: string;
  createdByName?: string;
  ownerName?: string;
  memberId?: number;
  memberName?: string;
  memberColor?: string;
}

export interface Member {
  id: number;
  name: string;
  color: string;
  isActive: boolean;
}

export interface CreateMemberRequest {
  name: string;
  color: string;
}

export interface UpdateMemberRequest {
  name: string;
  color: string;
  isActive: boolean;
}

export interface MonthlySummary {
  totalIncome: number;
  totalExpense: number;
  balance: number;
  transactionCount: number;
}

export interface MonthlyBreakdown {
  month: number;
  income: number;
  expense: number;
}

export interface YearlySummary {
  months: MonthlyBreakdown[];
}

export interface CategoryBreakdown {
  categoryId: number;
  name: string;
  icon: string;
  color: string;
  total: number;
  percentage: number;
  count: number;
}

export interface CategorySummary {
  categories: CategoryBreakdown[];
}

export interface Budget {
  id: number;
  categoryId: number;
  categoryName?: string;
  categoryIcon?: string;
  month: number;
  year: number;
  limitAmount: number;
}

export interface TransactionFilter {
  month?: number;
  year?: number;
  type?: TransactionType;
  categoryId?: number;
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
  userId?: string;
  memberId?: number;
}

export interface CreateTransactionRequest {
  categoryId: number;
  memberId?: number;
  type: TransactionType;
  amount: number;
  note?: string;
  transactionDate: string;
}

export interface TransactionHistoryEntry {
  id: number;
  fieldName: string;
  oldValue?: string;
  newValue?: string;
  changedByName: string;
  changedAt: string;
}

export interface CreateCategoryRequest {
  name: string;
  type: TransactionType;
  icon: string;
  color: string;
}

export interface BulkImportItem {
  categoryId: number;
  memberId?: number;
  type: TransactionType;
  amount: number;
  note?: string;
  transactionDate: string;
}
