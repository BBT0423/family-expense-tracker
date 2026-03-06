<template>
  <div class="nav-item">
    <RouterLink :to="href" class="nav-link" :class="{ active }">
      <span class="nav-icon">
        <slot name="icon" />
      </span>
      <span class="nav-text">
        <slot />
      </span>
      <ProBadge
        v-if="badgeCount && badgeCount.value > 0"
        class="nav-badge"
        :text="badgeCount.value.toString()"
        :status-id="100"
      />
    </RouterLink>
  </div>
</template>
<script setup lang="ts">
import type { Ref } from 'vue';
import ProBadge from '@/components/common/ProBadge.vue';

defineProps<{
  href: string;
  active?: boolean;
  badgeCount?: Ref<number>;
}>();
</script>
