<template>
  <div v-if="alert.show" class="pro-alert-backdrop">
    <div class="pro-alert-modal" :class="alert.type">
      <div class="pro-alert-header">
        <span class="pro-alert-icon">
          <svg
            v-if="alert.type === 'success'"
            width="24"
            height="24"
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
            v-else-if="alert.type === 'error'"
            width="24"
            height="24"
            viewBox="0 0 20 20"
            fill="none"
          >
            <circle cx="10" cy="10" r="10" fill="#ffeaea" />
            <path d="M7 7l6 6M13 7l-6 6" stroke="#d32f2f" stroke-width="2" stroke-linecap="round" />
          </svg>
          <svg
            v-else-if="alert.type === 'warning'"
            width="24"
            height="24"
            viewBox="0 0 20 20"
            fill="none"
          >
            <circle cx="10" cy="10" r="10" fill="#fffbe6" />
            <path d="M10 6v5m0 3h.01" stroke="#b38600" stroke-width="2" stroke-linecap="round" />
          </svg>
          <svg v-else width="24" height="24" viewBox="0 0 20 20" fill="none">
            <circle cx="10" cy="10" r="10" fill="#e6f0fa" />
            <path d="M10 7v3m0 3h.01" stroke="#1976d2" stroke-width="2" stroke-linecap="round" />
            <circle cx="10" cy="14" r="1" fill="#1976d2" />
          </svg>
        </span>
        <div class="pro-alert-title">{{ alert.title || 'Alert' }}</div>
      </div>
      <div class="pro-alert-body">
        <div class="pro-alert-message">{{ alert.message }}</div>
      </div>
      <div class="pro-alert-footer">
        <button class="pro-alert-action" @click="hideAlert">{{ alert.buttonText || 'OK' }}</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useGlobalUi } from '@/composables/useGlobalUi';
const { alert, hideAlert } = useGlobalUi();
</script>
<style scoped>
.pro-alert-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.32);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s;
}

.pro-alert-modal {
  display: flex;
  flex-direction: column;
  align-items: stretch;
  border-radius: var(--radius);
  font-size: 1.08em;
  background: var(--bg);
  color: var(--text);
  border: 1px solid var(--border);
  box-shadow:
    0 8px 40px rgba(0, 0, 0, 0.18),
    0 1.5px 6px rgba(0, 0, 0, 0.08);
  min-width: 340px;
  max-width: 400px;
  min-height: 80px;
  position: relative;
  animation: pro-alert-modal-pop 0.22s cubic-bezier(0.4, 0, 0.2, 1);
}

.pro-alert-header {
  display: flex;
  align-items: center;
  margin-bottom: 0.7em;
  border-bottom: 1px solid var(--border);
  padding: 1rem;
}

.pro-alert-title {
  font-weight: 700;
  font-size: 1.18em;
  color: var(--text);
  margin: 0;
  padding: 0;
  line-height: 1.2;
}

.pro-alert-body {
  margin-bottom: 1.5em;
}

.pro-alert-message {
  font-size: 1.04em;
  color: var(--text);
  line-height: 1.6;
  word-break: break-word;
  padding: 1rem 1.5rem;
}

.pro-alert-footer {
  display: flex;
  justify-content: flex-end;
  padding: 1rem 1.5rem;
  border-top: 1px solid var(--border);
}

@keyframes pro-alert-modal-pop {
  0% {
    transform: scale(0.92);
    opacity: 0.7;
  }

  100% {
    transform: scale(1);
    opacity: 1;
  }
}

.pro-alert-icon {
  margin-right: 0.9em;
  font-size: 1.3em;
  display: flex;
  align-items: center;
}

.pro-alert-content {
  flex: 1;
}

.pro-alert-action {
  background: var(--primary);
  color: #fff;
  border: none;
  border-radius: 0.5em;
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;
  box-shadow: 0 2px 8px rgba(25, 118, 210, 0.1);
  transition:
    background 0.18s,
    box-shadow 0.18s;
  letter-spacing: 0.02em;
  padding: 0.55em 2.2em;
}

.pro-alert-action:hover {
  background: var(--primary-dark);
}
</style>
