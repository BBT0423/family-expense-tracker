import type { App } from 'vue';
import { useGlobalUi } from '@/composables/useGlobalUi';

interface Options {
  interval?: number;
}

export default {
  install(app: App, options: Options = {}) {
    const checkInterval = options.interval ?? 60 * 1000; // default: 1 min
    let currentVersion: string | null = null;

    async function fetchVersion() {
      try {
        const res = await fetch('/meta.json', { cache: 'no-store' });
        const { version } = await res.json();

        if (!currentVersion) {
          currentVersion = version;
        } else if (version !== currentVersion) {
          console.log('New version detected:', version);

          // Use custom confirm dialog instead of native confirm()
          const { showConfirm } = useGlobalUi();
          showConfirm('A new version is available. Reload now?', {
            title: 'Update Available',
            confirmText: 'Reload',
            cancelText: 'Later',
            onConfirm: () => {
              globalThis.location.reload();
            },
            onCancel: () => {
              console.log('User chose to update later');
            },
          });
        }
      } catch (err) {
        console.error('Version check failed:', err);
      }
    }

    // Initial check
    fetchVersion();

    // Poll periodically
    setInterval(fetchVersion, checkInterval);

    // Also check when window regains focus
    globalThis.addEventListener('focus', fetchVersion);

    // Handle dynamic import chunk errors gracefully
    globalThis.addEventListener(
      'error',
      (e) => {
        const target = e.target as HTMLScriptElement;
        if (target?.tagName === 'SCRIPT' && target.src.includes('/assets/')) {
          console.error('Chunk load failed, forcing reload');
          globalThis.location.reload();
        }
      },
      true
    );
  },
};
