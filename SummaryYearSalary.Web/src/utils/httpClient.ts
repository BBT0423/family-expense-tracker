import axios, {
  type AxiosInstance,
  type AxiosProgressEvent,
  type AxiosRequestConfig,
  type AxiosResponse,
} from 'axios';

/**
 * Extended config that surfaces upload/download progress callbacks.
 * Drop-in replacement for AxiosRequestConfig anywhere a progress bar is needed.
 *
 * @example — file import with live percentage
 * ```ts
 * const progress = ref(0)
 * await httpClient.post('/api/transactions/bulk', formData, {
 *   headers: { 'Content-Type': 'multipart/form-data' },
 *   onUploadProgress: (e) => {
 *     progress.value = Math.round((e.loaded / (e.total ?? 1)) * 100)
 *   },
 * })
 * // bind: <AppProgress :model-value="progress" label="กำลังนำเข้าข้อมูล..." />
 * ```
 */
export interface HttpProgressConfig extends AxiosRequestConfig {
  onUploadProgress?: (event: AxiosProgressEvent) => void;
  onDownloadProgress?: (event: AxiosProgressEvent) => void;
}
import axiosRetry from 'axios-retry';
import { SecureStorage } from '@/utils/storage';
import {
  STORAGE_EXPIRES_IN,
  STORAGE_REFRESH_TOKEN,
  STORAGE_TOKEN,
  STORAGE_USER,
} from '@/constants/storage';
import { authService } from '@/services/authService';
import { useGlobalUi } from '@/composables/useGlobalUi';

// Configure axios retry globally
axiosRetry(axios, {
  retries: Number(import.meta.env.VITE_MAXIMUM_RETRIES) || 3,
  retryDelay: axiosRetry.exponentialDelay,
});
interface CacheEntry {
  response: AxiosResponse;
  timestamp: number;
}

/**
 * HTTP client configuration with interceptors, caching, and token management
 */
class HttpClient {
  private readonly client: AxiosInstance;
  private cachedToken: string | null = null;
  private readonly getCache = new Map<string, CacheEntry>();
  private readonly cacheTTL: number;
  private readonly baseURL: string;
  private readonly timeout: number;

  constructor() {
    this.cacheTTL = Number(import.meta.env.VITE_CACHE_TTL) || 900000; // 15 minutes
    this.baseURL = '';
    this.timeout = 60000;

    this.client = this.createAxiosInstance();
    this.setupInterceptors();
  }

  private createAxiosInstance(): AxiosInstance {
    return axios.create({
      baseURL: this.baseURL,
      timeout: this.timeout,
      headers: {
        'Content-Type': 'application/json',
      },
    });
  }

  /**
   * Initialize cached token from storage
   * This must be called after instantiation to properly load the token
   */
  public async initialize(): Promise<void> {
    try {
      this.cachedToken = SecureStorage.getItem(STORAGE_TOKEN);
    } catch (error) {
      console.warn('Failed to initialize token from storage:', error);
    }
  }

  private logout(): void {
    try {
      // Save current page as redirect URL for passive logout (401/403)
      const currentPath = globalThis.location.pathname + globalThis.location.search;
      const publicPaths = ['/authorize/', '/404', '/denied', '/'];
      const isPublicPath =
        publicPaths.some((p) => currentPath.startsWith(p)) || currentPath === '/';

      // Only save redirect for protected pages
      if (!isPublicPath && currentPath !== '/authorize/login') {
        sessionStorage.setItem('REDIRECT_URL', currentPath);
      }

      SecureStorage.removeItem(STORAGE_TOKEN);
      SecureStorage.removeItem(STORAGE_REFRESH_TOKEN);
      this.cachedToken = null;
    } catch (storageError) {
      console.warn('Failed to clear auth data from storage:', storageError);
    }

    // Redirect to login page
    if (globalThis.location.pathname !== '/authorize/login') {
      globalThis.location.href = '/authorize/login';
    }
  }

  private setupInterceptors(): void {
    this.setupRequestInterceptor();
    this.setupResponseInterceptor();
  }

  private setupRequestInterceptor(): void {
    this.client.interceptors.request.use(
      async (config) => {
        try {
          const token = SecureStorage.getItem(STORAGE_TOKEN);
          if (token) {
            config.headers.Authorization = `Bearer ${token}`;
          }
        } catch {
          if (this.cachedToken) {
            config.headers.Authorization = `Bearer ${this.cachedToken}`;
          }
        }

        // Clear global UI states before each request
        useGlobalUi().clearAll();

        return config;
      },
      (error: any) => {
        throw error;
      }
    );
  }

  private setupResponseInterceptor(): void {
    this.client.interceptors.response.use(
      (response: AxiosResponse) => response,
      async (error) => {
        const originalRequest = error.config;
        const isAuthRoute = originalRequest?.url?.startsWith('/api/authorize');
        console.log('Auth route:', isAuthRoute, 'URL:', originalRequest?.url);

        if (error.response?.status === 401 && !isAuthRoute) {
          return this.handleTokenRefresh(error);
        }
        throw error;
      }
    );
  }

  private async handleTokenRefresh(error: any): Promise<any> {
    const originalRequest = error.config;

    if (!originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = SecureStorage.getItem(STORAGE_REFRESH_TOKEN);
        const user = SecureStorage.getItem(STORAGE_USER)
          ? JSON.parse(SecureStorage.getItem(STORAGE_USER)!)
          : null;

        if (refreshToken && user) {
          const { data } = await authService.refreshToken({
            refreshToken,
            userId: user.id,
          });

          if (data?.isSuccess && data?.value) {
            this.updateAuthData(data.value);
            originalRequest.headers = originalRequest.headers || {};
            originalRequest.headers.Authorization = `Bearer ${this.cachedToken}`;
            return this.client(originalRequest);
          }
        }
      } catch (refreshError) {
        console.error('Token refresh failed:', refreshError);
      }
    }

    this.logout();
    throw error;
  }

  private updateAuthData(authData: any): void {
    SecureStorage.setItem(STORAGE_TOKEN, authData.token);
    SecureStorage.setItem(STORAGE_REFRESH_TOKEN, authData.refreshToken);
    SecureStorage.setItem(STORAGE_EXPIRES_IN, String(authData.expiresIn));
    this.cachedToken = authData.token;
  }

  // Public API methods

  /**
   * GET request with optional in-memory cache and expiration
   */
  public async get<T = any>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    const useCache: boolean = config?.params?.useCache ?? false;
    const cacheKey = this.generateCacheKey(url, config);

    if (useCache) {
      const cachedResponse = this.getCachedResponse(cacheKey);
      if (cachedResponse) {
        return cachedResponse;
      }
    }

    const response = await this.client.get<T>(url, config);

    if (useCache) {
      this.setCacheEntry(cacheKey, response);
    }

    return response;
  }

  /**
   * POST request — supports onUploadProgress / onDownloadProgress via HttpProgressConfig
   */
  public async post<T = any>(
    url: string,
    data?: any,
    config?: HttpProgressConfig
  ): Promise<AxiosResponse<T>> {
    return this.client.post<T>(url, data, config);
  }

  /**
   * PUT request — supports onUploadProgress / onDownloadProgress via HttpProgressConfig
   */
  public async put<T = any>(
    url: string,
    data?: any,
    config?: HttpProgressConfig
  ): Promise<AxiosResponse<T>> {
    return this.client.put<T>(url, data, config);
  }

  /**
   * DELETE request
   */
  public async delete<T = any>(
    url: string,
    config?: AxiosRequestConfig
  ): Promise<AxiosResponse<T>> {
    return this.client.delete<T>(url, config);
  }

  // Cache management methods

  private generateCacheKey(url: string, config?: AxiosRequestConfig): string {
    return url + (config ? JSON.stringify(config.params || config) : '');
  }

  private getCachedResponse(cacheKey: string): AxiosResponse | null {
    const entry = this.getCache.get(cacheKey);
    if (!entry) return null;

    const now = Date.now();
    if (now - entry.timestamp < this.cacheTTL) {
      return entry.response;
    }

    this.getCache.delete(cacheKey);
    return null;
  }

  private setCacheEntry(cacheKey: string, response: any): void {
    this.getCache.set(cacheKey, {
      response,
      timestamp: Date.now(),
    });
  }

  /**
   * Invalidate a specific cache entry for GET requests
   */
  public invalidateGetCache(url: string, config?: AxiosRequestConfig): void {
    const cacheKey = this.generateCacheKey(url, config);
    this.getCache.delete(cacheKey);
  }

  /**
   * Clear all GET cache entries
   */
  public clearGetCache(): void {
    this.getCache.clear();
  }
}

// Create singleton instance
const httpClientInstance = new HttpClient();

// Initialize the token asynchronously
try {
  await httpClientInstance.initialize();
} catch (error) {
  console.error('Failed to initialize HTTP client:', error);
}

export const httpClient = httpClientInstance;
