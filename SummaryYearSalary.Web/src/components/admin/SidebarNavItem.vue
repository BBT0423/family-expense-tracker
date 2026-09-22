<template>
  <RouterLink :to="href" class="nav-link" :class="{ active }">
    <span class="nav-icon"><slot name="icon" /></span>
    <span class="nav-text"><slot /></span>
  </RouterLink>
</template>

<script setup lang="ts">
defineProps<{ href: string; active?: boolean }>();
</script>
