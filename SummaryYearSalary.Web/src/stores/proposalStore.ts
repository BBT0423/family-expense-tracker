import { defineStore } from 'pinia';
import { ref } from 'vue';
import { proposalService } from '@/services/proposalService';
import type { AcceptTransactionProposalRequest, TransactionProposal } from '@/types';

export const useProposalStore = defineStore('proposals', () => {
  const incoming = ref<TransactionProposal[]>([]);
  const loading = ref(false);

  async function fetchIncoming() {
    loading.value = true;
    try {
      const { data } = await proposalService.getIncoming();
      if (data?.isSuccess && data.value) incoming.value = data.value;
    } finally {
      loading.value = false;
    }
  }

  async function accept(id: number, payload: AcceptTransactionProposalRequest) {
    const { data } = await proposalService.accept(id, payload);
    if (data?.isSuccess) await fetchIncoming();
    return data;
  }

  async function reject(id: number) {
    const { data } = await proposalService.reject(id);
    if (data?.isSuccess) await fetchIncoming();
    return data;
  }

  return { incoming, loading, fetchIncoming, accept, reject };
});
