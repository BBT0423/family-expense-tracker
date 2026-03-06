<template>
  <div v-if="message.show" class="pro-message-wrapper">
    <div class="pro-message" :class="message.type">
      <span class="pro-message-icon">
        <svg
          v-if="message.type === 'success'"
          width="20"
          height="20"
          viewBox="0 0 20 20"
          fill="none"
        >
          <circle cx="10" cy="10" r="10" fill="#e6f9ed" />
          <path
            d="M6 10.5l2.5 2.5 5-5"
            stroke="#1a7f37"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round"
          />
        </svg>
        <svg
          v-else-if="message.type === 'error'"
          width="20"
          height="20"
          viewBox="0 0 20 20"
          fill="none"
        >
          <circle cx="10" cy="10" r="10" fill="#ffeaea" />
          <path d="M7 7l6 6M13 7l-6 6" stroke="#d32f2f" stroke-width="2" stroke-linecap="round" />
        </svg>
        <svg
          v-else-if="message.type === 'warning'"
          width="20"
          height="20"
          viewBox="0 0 20 20"
          fill="none"
        >
          <circle cx="10" cy="10" r="10" fill="#fffbe6" />
          <path d="M10 6v5m0 3h.01" stroke="#b38600" stroke-width="2" stroke-linecap="round" />
        </svg>
        <svg v-else width="20" height="20" viewBox="0 0 20 20" fill="none">
          <circle cx="10" cy="10" r="10" fill="#e6f0fa" />
          <path d="M10 7v3m0 3h.01" stroke="#1976d2" stroke-width="2" stroke-linecap="round" />
          <circle cx="10" cy="14" r="1" fill="#1976d2" />
        </svg>
      </span>
      <span class="pro-message-content">
        {{ message.message }}
      </span>
      <button
        v-if="message.closable"
        class="pro-message-close"
        aria-label="Close"
        @click="hideMessage"
      >
        <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
          <path d="M5 5l8 8M13 5l-8 8" stroke="#888" stroke-width="2" stroke-linecap="round" />
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useGlobalUi } from '@/composables/useGlobalUi';
const { message, hideMessage } = useGlobalUi();
</script>

<style scoped>
.pro-message {
  display: flex;
  align-items: center;
  padding: 0.8em 1em;
  border-radius: 0.4em;
  font-size: 1em;
  margin: 0.5em 0;
  background: #f5f5f5;
  color: #333;
  border: 1px solid #e0e0e0;
  position: relative;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
  min-height: 44px;
}

.pro-message-icon {
  margin-right: 0.7em;
  font-size: 1.2em;
  display: flex;
  align-items: center;
}

.pro-message-content {
  flex: 1;
  line-height: 1.24rem;
}

.pro-message-close {
  background: none;
  border: none;
  padding: 0.2em;
  color: #888;
  cursor: pointer;
  margin-left: 0.7em;
  line-height: 1;
}

.pro-message.success {
  background: #e6f9ed;
  color: #1a7f37;
  border-color: #b6e2c7;
}

.pro-message.error {
  background: #ffeaea;
  color: #d32f2f;
  border-color: #f5c6cb;
}

.pro-message.warning {
  background: #fffbe6;
  color: #b38600;
  border-color: #ffe58f;
}

.pro-message.info {
  background: #e6f0fa;
  color: #1976d2;
  border-color: #90caf9;
}

.pro-message-wrapper {
  width: 100%;
  margin-bottom: 1em;
  left: 0;
  position: relative;
  box-sizing: border-box;
  padding-left: 0;
  padding-right: 0;
}

.pro-message {
  width: 100%;
  border-radius: 0.4em;
  margin: 0;
}
</style>
