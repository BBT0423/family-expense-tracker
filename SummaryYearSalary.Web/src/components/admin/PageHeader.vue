<template>
  <div class="page-header">
    <div class="grid grid-2" style="justify-content: space-between">
      <div>
        <nav v-if="breadcrumbs.length" class="breadcrumb-nav">
          <ol class="breadcrumb-list">
            <li v-for="(crumb, idx) in breadcrumbs" :key="idx" class="breadcrumb-item">
              <template v-if="crumb.url && idx !== breadcrumbs.length - 1">
                <router-link :to="crumb.url">{{ crumb.name }}</router-link>
              </template>
              <template v-else>
                <span>{{ crumb.name }}</span>
              </template>
            </li>
          </ol>
        </nav>
        <h1 v-if="pageTitle" class="page-title">{{ pageTitle }}</h1>
        <p v-if="pageSubtitle" class="page-subtitle">{{ pageSubtitle }}</p>
      </div>
      <div class="action-left">
        <slot name="actions-left" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';

type Breadcrumb = { name: string; url?: string | null };

const route = useRoute();
const pageTitle = computed(() => route.meta.title || '');
const pageSubtitle = computed(() => route.meta.subtitle || '');
const breadcrumbs = computed(() => (route.meta.breadcrumb as Breadcrumb[]) || []);
</script>

<style scoped>
.page-header {
  margin-bottom: 2rem;
}

.page-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: var(--text);
}

.page-subtitle {
  color: var(--text);
}

.breadcrumb-list {
  display: flex;
  gap: 0.5rem;
  list-style: none;
  padding: 0;
  margin: 0;
}

.breadcrumb-item {
  color: var(--text-light, #888);
  font-size: 0.875em;
}

.breadcrumb-item:not(:last-child)::after {
  content: '/';
  margin: 0 0.5em;
  color: var(--text-light, #bbb);
}

.action-left {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: flex-end;
}
</style>
