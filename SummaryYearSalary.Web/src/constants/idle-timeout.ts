/**
 * Idle Timeout Configuration
 *
 * Configure the application's idle timeout behavior
 */

export interface IdleTimeoutSettings {
  /**
   * Timeout duration in milliseconds
   * Default: 15 minutes (900000 ms)
   */
  timeout: number;

  /**
   * Events to track for user activity
   */
  events: string[];

  /**
   * Whether idle timeout is enabled
   * Default: true
   */
  enabled: boolean;
}

/**
 * Default idle timeout configuration
 */
export const idleTimeoutConfig: IdleTimeoutSettings = {
  // 15 minutes by default
  timeout: 15 * 60 * 1000,

  // Events that indicate user activity
  events: ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click'],

  // Enable idle timeout by default
  enabled: true,
};

/**
 * Get idle timeout from environment or use default
 */
export const getIdleTimeout = (): number => {
  // Check if there's an environment variable for idle timeout
  const envTimeout = import.meta.env.VITE_IDLE_TIMEOUT_MINUTES;

  if (envTimeout) {
    const minutes = Number.parseInt(envTimeout, 10);
    if (!Number.isNaN(minutes) && minutes > 0) {
      return minutes * 60 * 1000;
    }
  }

  return idleTimeoutConfig.timeout;
};

/**
 * Check if idle timeout is enabled
 */
export const isIdleTimeoutEnabled = (): boolean => {
  const envEnabled = import.meta.env.VITE_IDLE_TIMEOUT_ENABLED;

  if (envEnabled !== undefined) {
    return envEnabled === 'true';
  }

  return idleTimeoutConfig.enabled;
};
