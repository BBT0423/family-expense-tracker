<template>
  <div class="header-actions">
    <button
      class="theme-btn"
      :aria-label="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
      :title="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
      tabindex="0"
      @click="handleThemeToggle"
      @keyup.enter.space="handleThemeToggle"
    >
      <span class="theme-icon" :class="{ rotate: isDark }">
        <svg
          v-if="!isDark"
          width="22"
          height="22"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <circle cx="12" cy="12" r="5" />
          <line x1="12" y1="1" x2="12" y2="3" />
          <line x1="12" y1="21" x2="12" y2="23" />
          <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" />
          <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" />
          <line x1="1" y1="12" x2="3" y2="12" />
          <line x1="21" y1="12" x2="23" y2="12" />
          <line x1="4.22" y1="19.78" x2="5.64" y2="18.36" />
          <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" />
        </svg>
        <svg
          v-else
          width="22"
          height="22"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <path d="M21 12.79A9 9 0 1 1 11.21 3a7 7 0 0 0 9.79 9.79z" />
        </svg>
      </span>
      <span class="theme-label">{{ isDark ? 'Light' : 'Dark' }}</span>
    </button>
  </div>
</template>

<script setup lang="ts">
import { inject } from 'vue';
const isDark = inject('isDark');
const toggleTheme = inject('toggleTheme');
function handleThemeToggle() {
  if (typeof toggleTheme === 'function') toggleTheme();
}
</script>

<style scoped>
.theme-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: var(--layout-bg);
  border: 1px solid var(--layout-border);
  padding: 0.5rem 1rem;
  border-radius: var(--radius);
  cursor: pointer;
  color: var(--layout-text);
  font-size: 1rem;
  font-weight: 500;
  transition:
    background 0.2s,
    color 0.2s,
    border 0.2s;
  outline: none;
}

.theme-btn:focus {
  box-shadow: 0 0 0 1px var(--primary-light);
}

.theme-btn:hover {
  background: var(--primary);
  color: white;
  border-color: var(--primary-dark);
}

.theme-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.3rem;
  transition: transform 0.4s cubic-bezier(0.4, 2, 0.6, 1);
}
.theme-icon.rotate {
  transform: rotate(180deg);
}

.theme-label {
  display: none;
  margin-left: 0.25rem;
  font-size: 1rem;
  font-weight: 400;
}

@media (min-width: 600px) {
  .theme-label {
    display: inline;
  }
}
</style>
