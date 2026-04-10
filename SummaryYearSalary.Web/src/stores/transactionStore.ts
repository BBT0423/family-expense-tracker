import { defineStore } from 'pinia';
import { ref } from 'vue';
import { transactionService } from '@/services/transactionService';
import type {
  Transaction,
  TransactionFilter,
  CreateTransactionRequest,
  BulkImportItem,
} from '@/types';

export const useTransactionStore = defineStore('transactions', () => {
  const transactions = ref<Transaction[]>([]);
  const totalCount = ref(0);
  const totalPages = ref(0);
  const loading = ref(false);
  const filter = ref<TransactionFilter>({
    month: new Date().getMonth() + 1,
    year: new Date().getFullYear(),
    pageNumber: 1,
    pageSize: 20,
  });

  async function fetchTransactions(f?: TransactionFilter) {
    if (f) filter.value = { ...filter.value, ...f };
    loading.value = true;
    try {
      const { data } = await transactionService.getList(filter.value);
      if (data?.isSuccess && data.value) {
        transactions.value = data.value.items as unknown as Transaction[];
        totalCount.value = data.value.totalCount;
        totalPages.value = data.value.totalPages;
      }
    } finally {
      loading.value = false;
    }
  }

  async function createTransaction(payload: CreateTransactionRequest) {
    const { data } = await transactionService.create(payload);
    return data;
  }

  async function updateTransaction(id: number, payload: CreateTransactionRequest) {
    const { data } = await transactionService.update(id, payload);
    return data;
  }

  async function deleteTransaction(id: number) {
    const { data } = await transactionService.remove(id);
    if (data?.isSuccess) {
      await fetchTransactions();
    }
    return data;
  }

  async function bulkImport(items: BulkImportItem[]) {
    const { data } = await transactionService.bulkImport(items);
    return data;
  }

  return {
    transactions,
    totalCount,
    totalPages,
    loading,
    filter,
    fetchTransactions,
    createTransaction,
    updateTransaction,
    deleteTransaction,
    bulkImport,
  };
});
