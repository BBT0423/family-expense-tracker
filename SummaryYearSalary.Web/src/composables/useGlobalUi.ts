// Confirm dialog state
export interface ConfirmState {
  show: boolean;
  message: string;
  title?: string;
  confirmText?: string;
  cancelText?: string;
  onConfirm?: () => void;
  onCancel?: () => void;
}

const _confirm = ref<ConfirmState>({
  show: false,
  message: '',
  title: undefined,
  confirmText: undefined,
  cancelText: undefined,
  onConfirm: undefined,
  onCancel: undefined,
});

import type { ProblemDetails } from '@/types';
import { ref } from 'vue';

// Types for alert
export type AlertType = 'success' | 'info' | 'warning' | 'error';

interface MessageState {
  show: boolean;
  message: string;
  type: AlertType;
  closable: boolean;
}

interface AlertState {
  show: boolean;
  message: string;
  type: AlertType;
  closable: boolean;
  title?: string;
  buttonText?: string;
}

const _loading = ref(false);
const _alert = ref<AlertState>({
  show: false,
  message: '',
  type: 'info',
  closable: true,
  title: undefined,
  buttonText: undefined,
});

const _message = ref<MessageState>({
  show: false,
  message: '',
  type: 'info',
  closable: true,
});

function showLoading() {
  _loading.value = true;
}

function hideLoading() {
  _loading.value = false;
}

function showAlert(
  message: string,
  type: AlertType = 'info',
  closable: boolean = true,
  title?: string,
  buttonText?: string
) {
  _alert.value = {
    show: true,
    message,
    type,
    closable,
    title,
    buttonText,
  };
}

function hideAlert() {
  _alert.value.show = false;
  _alert.value.message = '';
}

function showMessage(
  arg1: string | ProblemDetails,
  type: AlertType = 'info',
  closable: boolean = true
): void {
  if (typeof arg1 === 'string') {
    _message.value = {
      show: true,
      message: arg1,
      type,
      closable,
    };
  } else {
    _message.value = {
      show: true,
      message: arg1.detail,
      type: 'error',
      closable: true,
    };
  }

  // Scroll to top to focus the message
  if (typeof window !== 'undefined') {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}

function hideMessage() {
  _message.value.show = false;
  _message.value.message = '';
}

function showConfirm(
  message: string,
  options?: {
    title?: string;
    confirmText?: string;
    cancelText?: string;
    onConfirm?: () => void;
    onCancel?: () => void;
  }
) {
  _confirm.value = {
    show: true,
    message,
    title: options?.title,
    confirmText: options?.confirmText,
    cancelText: options?.cancelText,
    onConfirm: options?.onConfirm,
    onCancel: options?.onCancel,
  };
}

function hideConfirm() {
  _confirm.value.show = false;
  _confirm.value.message = '';
}

function clearAll() {
  hideAlert();
  hideMessage();
  hideConfirm();
}

export const useGlobalUi = () => {
  return {
    loading: _loading,
    showLoading,
    hideLoading,
    alert: _alert,
    showAlert,
    hideAlert,
    message: _message,
    showMessage,
    hideMessage,
    confirm: _confirm,
    showConfirm,
    hideConfirm,
    clearAll,
  };
};
