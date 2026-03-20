/**
 * useNotify — UI Notification Service (swappable abstraction)
 *
 * ── HOW TO SWAP LIBRARY ────────────────────────────────────────────────────
 *  1. เปลี่ยน import section ด้านล่าง (เพิ่ม/ลบ import ของ library ใหม่)
 *  2. เปลี่ยน implementation ของแต่ละ function ให้ call library ใหม่
 *  3. views / composables อื่น ไม่ต้องแก้เลย
 * ──────────────────────────────────────────────────────────────────────────
 */

// ─── Library imports ── เปลี่ยนแค่ส่วนนี้เมื่อ swap ───────────────────────
import { ElMessage, ElMessageBox, ElNotification, ElLoading } from 'element-plus';
// ──────────────────────────────────────────────────────────────────────────

// ─── Types (library-agnostic — ไม่มี EP types รั่วออกไป) ──────────────────

export type NotifyType = 'success' | 'error' | 'warning' | 'info';

export interface ConfirmOptions {
  /** หัวข้อ dialog */
  title?: string;
  /** ข้อความปุ่มยืนยัน (default: ตกลง) */
  confirmText?: string;
  /** ข้อความปุ่มยกเลิก (default: ยกเลิก) */
  cancelText?: string;
  /** ประเภท icon ใน dialog */
  type?: NotifyType;
  /** ทำให้ปุ่ม confirm เป็นสีแดง (สำหรับ destructive action) */
  dangerConfirm?: boolean;
}

export interface NotifyOptions {
  /** หัวข้อ notification (default: Success / Error / …) */
  title?: string;
  /** ระยะเวลาแสดง ms — 0 = ค้างไว้ตลอด (default: 4500) */
  duration?: number;
  /** ตำแหน่งแสดง (default: top-right) */
  position?: 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left';
}

export interface LoadingInstance {
  close(): void;
}

// ──────────────────────────────────────────────────────────────────────────

export function useNotify() {
  /**
   * Toast — ข้อความสั้นๆ แสดงชั่วคราว auto-dismiss
   * @example toast('บันทึกสำเร็จ', 'success')
   */
  function toast(message: string, type: NotifyType = 'info', duration = 3000): void {
    // ── EP: ElMessage ──────────────────────────────────────────────────────
    ElMessage({ message, type, duration, showClose: true, grouping: true });
    // ──────────────────────────────────────────────────────────────────────
  }

  /**
   * Notify — corner notification พร้อม title
   * @example notify('โหลดข้อมูลสำเร็จ', 'success', { title: 'สำเร็จ' })
   */
  function notify(
    message: string,
    type: NotifyType = 'info',
    options: NotifyOptions = {}
  ): void {
    // ── EP: ElNotification ────────────────────────────────────────────────
    ElNotification({
      title: options.title ?? capitalize(type),
      message,
      type,
      duration: options.duration ?? 4500,
      position: (options.position ?? 'top-right') as any,
    });
    // ──────────────────────────────────────────────────────────────────────
  }

  /**
   * Confirm — Promise-based dialog, return true = ยืนยัน / false = ยกเลิก
   * @example
   * const ok = await confirm('ลบข้อมูลนี้?', { dangerConfirm: true })
   * if (ok) { ... }
   */
  async function confirm(
    message: string,
    options: ConfirmOptions = {}
  ): Promise<boolean> {
    try {
      // ── EP: ElMessageBox.confirm ────────────────────────────────────────
      await ElMessageBox.confirm(message, options.title ?? 'ยืนยัน', {
        confirmButtonText: options.confirmText ?? 'ตกลง',
        cancelButtonText: options.cancelText ?? 'ยกเลิก',
        type: options.type ?? 'warning',
        confirmButtonClass: options.dangerConfirm ? 'el-button--danger' : '',
      });
      // ────────────────────────────────────────────────────────────────────
      return true;
    } catch {
      return false; // user กด Cancel หรือ × หรือ Esc
    }
  }

  /**
   * Alert — Promise-based alert (1 ปุ่ม OK) รอจนผู้ใช้กดปิด
   * @example await alert('Session หมดอายุ', 'warning', 'แจ้งเตือน')
   */
  async function alert(
    message: string,
    type: NotifyType = 'info',
    title?: string
  ): Promise<void> {
    // ── EP: ElMessageBox.alert ────────────────────────────────────────────
    await ElMessageBox.alert(message, title ?? capitalize(type), {
      type,
      confirmButtonText: 'ตกลง',
    });
    // ──────────────────────────────────────────────────────────────────────
  }

  /**
   * Loading — full-screen overlay, returns instance with .close()
   * @example
   * const loader = loading('กำลังบันทึก...')
   * try { await api.save() } finally { loader.close() }
   */
  function loading(text = 'กำลังโหลด...'): LoadingInstance {
    // ── EP: ElLoading.service ─────────────────────────────────────────────
    return ElLoading.service({
      lock: true,
      text,
      background: 'rgba(0, 0, 0, 0.55)',
    });
    // ──────────────────────────────────────────────────────────────────────
  }

  return { toast, notify, confirm, alert, loading };
}

// ─── Helper ───────────────────────────────────────────────────────────────
function capitalize(s: string) {
  return s.charAt(0).toUpperCase() + s.slice(1);
}
