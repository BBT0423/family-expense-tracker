<template>
  <div class="user-info">
    <h4>{{ name || 'Unknown' }}</h4>
    <p>{{ email || 'No email provided' }}</p>
  </div>
</template>
<script setup lang="ts">
defineProps<{
  name: string | undefined;
  email: string | undefined;
}>();
</script>
