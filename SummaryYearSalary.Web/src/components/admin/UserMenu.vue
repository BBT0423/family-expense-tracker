<template>
  <div class="user-menu" @click="toggleDropdown">
    <UserAvatar :name="user?.name" />
    <UserInfo :name="user?.name" :email="user?.email" />
    <UserDropdown :open="dropdownOpen" :user="user" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useAuthStore } from '@/stores/authStore';
const { user } = useAuthStore();

const dropdownOpen = ref(false);

function toggleDropdown(e: MouseEvent) {
  e.stopPropagation();
  dropdownOpen.value = !dropdownOpen.value;
}

function closeDropdown() {
  dropdownOpen.value = false;
}

function handleClickOutside() {
  closeDropdown();
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside);
});
onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside);
});
</script>
