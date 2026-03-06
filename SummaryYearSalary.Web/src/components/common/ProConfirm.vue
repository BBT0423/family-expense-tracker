<template>
  <div v-if="confirm.show" class="pro-alert-backdrop">
    <div class="pro-alert-modal confirm-modal">
      <div class="pro-alert-header">
        <span class="pro-alert-icon">
          <svg width="24" height="24" viewBox="0 0 20 20" fill="none">
            <circle cx="10" cy="10" r="10" fill="#e6f0fa" />
            <path d="M10 7v3m0 3h.01" stroke="#1976d2" stroke-width="2" stroke-linecap="round" />
            <circle cx="10" cy="14" r="1" fill="#1976d2" />
          </svg>
        </span>
        <div class="pro-alert-title">{{ confirm.title || 'Confirm' }}</div>
      </div>
      <div class="pro-alert-body">
        <div class="pro-alert-message">{{ confirm.message }}</div>
      </div>
      <div class="pro-alert-footer">
        <button class="pro-alert-action pro-alert-cancel" @click="onCancel">
          {{ confirm.cancelText || 'Cancel' }}
        </button>
        <button
          v-if="confirm.confirmText"
          class="pro-alert-action pro-alert-confirm"
          @click="onConfirm"
        >
          {{ confirm.confirmText || 'Confirm' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useGlobalUi } from '@/composables/useGlobalUi';
const { confirm, hideConfirm } = useGlobalUi();

function onConfirm() {
  if (typeof confirm.value.onConfirm === 'function') {
    confirm.value.onConfirm();
  }
  hideConfirm();
}

function onCancel() {
  if (typeof confirm.value.onCancel === 'function') {
    confirm.value.onCancel();
  }
  hideConfirm();
}
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
  gap: 0.7em;
  border-top: 1px solid var(--border);
}
.pro-alert-cancel {
  background: var(--border);
  color: var(--text);
  border: 1px solid var(--border);
  font-weight: 500;
  border-radius: 0.5em;
  font-size: 0.82rem;
  cursor: pointer;
  padding: 0.55em 2.2em;
  transition:
    background 0.18s,
    box-shadow 0.18s;
}
.pro-alert-cancel:hover {
  background: var(--surface-dark);
}
.pro-alert-confirm {
  background: var(--primary);
  color: #fff;
  border: none;
  font-weight: 600;
  border-radius: 0.5em;
  font-size: 0.82rem;
  cursor: pointer;
  padding: 0.55em 2.2em;
  transition:
    background 0.18s,
    box-shadow 0.18s;
}
.pro-alert-confirm:hover {
  background: var(--primary-dark);
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
</style>
