import { onMounted, onUnmounted, ref } from 'vue';

const isOffline = ref(false);
const isPoorConnection = ref(false);

const updateConnectionStatus = () => {
  const connection = (
    navigator as Navigator & {
      connection?: {
        effectiveType?: string;
        saveData?: boolean;
      };
    }
  ).connection;

  if (connection) {
    const effectiveType = connection.effectiveType || '';
    const isSlow = effectiveType === 'slow-2g' || effectiveType === '2g';
    isPoorConnection.value = Boolean(connection.saveData) || isSlow;
  } else {
    isPoorConnection.value = false;
  }
};

const checkReachability = async (): Promise<boolean> => {
  try {
    const googleUrl = 'https://www.google.com/generate_204';
    const controller = new AbortController();
    const timeoutId = window.setTimeout(() => controller.abort(), 3000);

    // Check internet reachability via Google (no-cors -> opaque response is OK if fetch resolves)
    try {
      await fetch(googleUrl, {
        method: 'GET',
        cache: 'no-store',
        mode: 'no-cors',
        signal: controller.signal,
      });

      window.clearTimeout(timeoutId);
      return true;
    } catch {
      console.warn('Google reachability check failed, trying fallback URL');
    }

    window.clearTimeout(timeoutId);
    return false;
  } catch {
    return false;
  }
};

const updateOnlineStatus = async () => {
  const navigatorOnline =
    typeof navigator !== 'undefined' && typeof navigator.onLine === 'boolean'
      ? navigator.onLine
      : undefined;

  let reachable = true;
  if (navigatorOnline === false) {
    reachable = false;
  } else {
    reachable = await checkReachability();
  }

  isOffline.value = !reachable;

  updateConnectionStatus();
};

export const useNetworkStatus = () => {
  const handleConnectionChange = () => updateConnectionStatus();
  const handleUpdate = () => void updateOnlineStatus();
  let pollId: number | undefined;

  onMounted(() => {
    void updateOnlineStatus();
    window.addEventListener('online', handleUpdate);
    window.addEventListener('offline', handleUpdate);
    window.addEventListener('focus', handleUpdate);
    document.addEventListener('visibilitychange', handleUpdate);

    const connection = (
      navigator as Navigator & {
        connection?: {
          // eslint-disable-next-line no-unused-vars
          addEventListener?: (event: string, handler: () => void) => void;
        };
      }
    ).connection;

    if (connection?.addEventListener) {
      connection.addEventListener('change', handleConnectionChange);
    }

    pollId = window.setInterval(handleUpdate, 8000);
  });

  onUnmounted(() => {
    window.removeEventListener('online', handleUpdate);
    window.removeEventListener('offline', handleUpdate);
    window.removeEventListener('focus', handleUpdate);
    document.removeEventListener('visibilitychange', handleUpdate);

    const connection = (
      navigator as Navigator & {
        connection?: {
          // eslint-disable-next-line no-unused-vars
          removeEventListener?: (event: string, handler: () => void) => void;
        };
      }
    ).connection;

    if (connection?.removeEventListener) {
      connection.removeEventListener('change', handleConnectionChange);
    }

    if (pollId !== undefined) {
      window.clearInterval(pollId);
      pollId = undefined;
    }
  });

  return {
    isOffline,
    isPoorConnection,
  };
};
