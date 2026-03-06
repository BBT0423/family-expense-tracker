<template>
  <div class="avatar">{{ initials }}</div>
</template>
<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  name?: string;
}>();

const initials = computed(() => {
  if (props.name) {
    const names = props.name.split(' ');
    return names
      .map((n) => n.charAt(0))
      .join('')
      .toUpperCase();
  }
  return 'JT';
});
</script>
