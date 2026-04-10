import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { categoryService } from '@/services/categoryService';
import type { Category, CreateCategoryRequest, TransactionType } from '@/types';

export const useCategoryStore = defineStore('categories', () => {
  const categories = ref<Category[]>([]);
  const loading = ref(false);

  const incomeCategories = computed(() => categories.value.filter((c) => c.type === 'Income'));
  const expenseCategories = computed(() => categories.value.filter((c) => c.type === 'Expense'));

  function filterByType(type: TransactionType) {
    return categories.value.filter((c) => c.type === type);
  }

  async function fetchCategories() {
    loading.value = true;
    try {
      const { data } = await categoryService.getAll();
      if (data?.isSuccess && data.value) {
        categories.value = data.value;
      }
    } finally {
      loading.value = false;
    }
  }

  async function createCategory(payload: CreateCategoryRequest) {
    const { data } = await categoryService.create(payload);
    if (data?.isSuccess) {
      await fetchCategories();
    }
    return data;
  }

  async function updateCategory(id: number, payload: CreateCategoryRequest) {
    const { data } = await categoryService.update(id, payload);
    if (data?.isSuccess) {
      await fetchCategories();
    }
    return data;
  }

  async function deleteCategory(id: number) {
    const { data } = await categoryService.remove(id);
    if (data?.isSuccess) {
      await fetchCategories();
    }
    return data;
  }

  return {
    categories,
    loading,
    incomeCategories,
    expenseCategories,
    filterByType,
    fetchCategories,
    createCategory,
    updateCategory,
    deleteCategory,
  };
});
