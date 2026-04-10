export type TransactionType = 'Income' | 'Expense';

export interface FamilyMember {
  id: number;
  name: string;
  color: string;
  avatar?: string;
  isActive: boolean;
  createdAt: string;
}

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
  memberId: number;
  memberName: string;
  memberColor: string;
  categoryId: number;
  categoryName: string;
  categoryIcon: string;
  categoryColor: string;
  type: TransactionType;
  amount: number;
  note?: string;
  transactionDate: string;
  createdAt: string;
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

export interface MemberBreakdown {
  memberId: number;
  name: string;
  color: string;
  totalExpense: number;
  totalIncome: number;
  percentage: number;
}

export interface MemberSummary {
  members: MemberBreakdown[];
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
  memberId?: number;
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface CreateTransactionRequest {
  memberId: number;
  categoryId: number;
  type: TransactionType;
  amount: number;
  note?: string;
  transactionDate: string;
}

export interface CreateMemberRequest {
  name: string;
  color: string;
  avatar?: string;
}

export interface UpdateMemberRequest {
  name: string;
  color: string;
  avatar?: string;
  isActive: boolean;
}

export interface CreateCategoryRequest {
  name: string;
  type: TransactionType;
  icon: string;
  color: string;
}

export interface BulkImportItem {
  memberId: number;
  categoryId: number;
  type: TransactionType;
  amount: number;
  note?: string;
  transactionDate: string;
}
