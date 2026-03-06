<template>
  <Teleport to="body">
    <div v-if="show" class="pro-modal-overlay" @click="handleOverlayClick">
      <div class="pro-modal-content" :class="modalSizeClass" @click.stop>
        <!-- Header -->
        <div v-if="showHeader" class="pro-modal-header">
          <slot name="header">
            <h5 class="pro-modal-title">{{ title }}</h5>
          </slot>
          <button
            v-if="showCloseButton"
            type="button"
            class="pro-modal-close"
            :aria-label="closeAriaLabel"
            @click="handleClose"
          >
            &times;
          </button>
        </div>

        <!-- Body -->
        <div class="pro-modal-body" :class="{ 'no-header': !showHeader, 'no-footer': !showFooter }">
          <slot />
        </div>

        <!-- Footer -->
        <div v-if="showFooter" class="pro-modal-footer">
          <slot name="footer">
            <button
              v-if="showConfirmButton"
              type="button"
              class="btn btn-primary"
              :disabled="confirmDisabled"
              @click="handleConfirm"
            >
              {{ confirmText }}
            </button>
            <button
              v-if="showCancelButton"
              type="button"
              class="btn btn-secondary"
              @click="handleCancel"
            >
              {{ cancelText }}
            </button>
          </slot>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, watch, nextTick } from 'vue';

interface Props {
  show: boolean;
  title?: string;
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'full';
  showHeader?: boolean;
  showFooter?: boolean;
  showCloseButton?: boolean;
  showCancelButton?: boolean;
  showConfirmButton?: boolean;
  cancelText?: string;
  confirmText?: string;
  confirmDisabled?: boolean;
  closeOnOverlayClick?: boolean;
  closeAriaLabel?: string;
}

const props = withDefaults(defineProps<Props>(), {
  title: '',
  size: 'md',
  showHeader: true,
  showFooter: true,
  showCloseButton: true,
  showCancelButton: true,
  showConfirmButton: true,
  cancelText: 'Cancel',
  confirmText: 'Confirm',
  confirmDisabled: false,
  closeOnOverlayClick: false,
  closeAriaLabel: 'Close modal',
});

const emit = defineEmits<{
  'update:show': [value: boolean];
  close: [];
  cancel: [];
  confirm: [];
}>();

const modalSizeClass = computed(() => {
  const sizeClasses = {
    sm: 'pro-modal-sm',
    md: 'pro-modal-md',
    lg: 'pro-modal-lg',
    xl: 'pro-modal-xl',
    full: 'pro-modal-full',
  };
  return sizeClasses[props.size];
});

const handleClose = () => {
  emit('update:show', false);
  emit('close');
};

const handleCancel = () => {
  emit('cancel');
  handleClose();
};

const handleConfirm = () => {
  emit('confirm');
};

const handleOverlayClick = () => {
  if (props.closeOnOverlayClick) {
    handleClose();
  }
};

// Handle escape key
watch(
  () => props.show,
  (newShow) => {
    if (newShow) {
      nextTick(() => {
        const handleEscape = (e: KeyboardEvent) => {
          if (e.key === 'Escape') {
            handleClose();
          }
        };
        document.addEventListener('keydown', handleEscape);

        // Cleanup on modal close
        const cleanup = () => {
          document.removeEventListener('keydown', handleEscape);
        };

        // Store cleanup function for when modal closes
        (window as any).__proModalCleanup = cleanup;
      });
    } else if ((window as any).__proModalCleanup) {
      // Cleanup escape listener
      (window as any).__proModalCleanup();
      delete (window as any).__proModalCleanup;
    }
  }
);
</script>

<style scoped>
.pro-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(2px);
}

.pro-modal-content {
  background: var(--bg, #ffffff);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
  max-height: 90vh;
  animation: modalFadeIn 0.2s ease-out;
}

@keyframes modalFadeIn {
  from {
    opacity: 0;
    transform: scale(0.95) translateY(-20px);
  }
  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}

/* Size variants */
.pro-modal-sm {
  width: 500px;
  max-width: 90vw;
}

.pro-modal-md {
  width: 700px;
  max-width: 90vw;
}

.pro-modal-lg {
  width: 900px;
  max-width: 95vw;
}

.pro-modal-xl {
  width: 1200px;
  max-width: 95vw;
}

.pro-modal-full {
  width: 95vw;
  height: 95vh;
  max-width: none;
  max-height: none;
}

.pro-modal-header {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--border, #dee2e6);
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0;
}

.pro-modal-title {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text, #212529);
}

.pro-modal-close {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  color: var(--text, #212529);
  transition: all 0.2s ease;
}

.pro-modal-close:hover {
  background: var(--bg, #f8f9fa);
  color: var(--text, #000000);
}

.pro-modal-body {
  padding: 1.5rem;
  overflow-y: auto;
  flex: 1;
}

.pro-modal-body.no-header {
  padding-top: 1.5rem;
}

.pro-modal-body.no-footer {
  padding-bottom: 1.5rem;
}

.pro-modal-footer {
  padding: 1rem 1.5rem;
  border-top: 1px solid var(--border, #dee2e6);
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  flex-shrink: 0;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .pro-modal-content {
    margin: 1rem;
    max-height: calc(100vh - 2rem);
  }

  .pro-modal-lg,
  .pro-modal-xl {
    width: 100%;
    max-width: calc(100vw - 2rem);
  }
}
</style>
