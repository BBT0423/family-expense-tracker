import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { memberService } from '@/services/memberService';
import type { Member, CreateMemberRequest, UpdateMemberRequest } from '@/types';

export const useMemberStore = defineStore('members', () => {
  const members = ref<Member[]>([]);
  const loading = ref(false);

  const activeMembers = computed(() => members.value.filter((m) => m.isActive));

  async function fetchMembers() {
    loading.value = true;
    try {
      const { data } = await memberService.getAll();
      if (data?.isSuccess && data.value) {
        members.value = data.value;
      }
    } finally {
      loading.value = false;
    }
  }

  async function createMember(payload: CreateMemberRequest) {
    const { data } = await memberService.create(payload);
    if (data?.isSuccess) {
      await fetchMembers();
    }
    return data;
  }

  async function updateMember(id: number, payload: UpdateMemberRequest) {
    const { data } = await memberService.update(id, payload);
    if (data?.isSuccess) {
      await fetchMembers();
    }
    return data;
  }

  async function deleteMember(id: number) {
    const { data } = await memberService.remove(id);
    if (data?.isSuccess) {
      await fetchMembers();
    }
    return data;
  }

  return {
    members,
    loading,
    activeMembers,
    fetchMembers,
    createMember,
    updateMember,
    deleteMember,
  };
});
