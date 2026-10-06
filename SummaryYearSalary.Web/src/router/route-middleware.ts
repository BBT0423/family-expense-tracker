import { useGlobalUi } from '@/composables/useGlobalUi';
import { useAuthStore } from '@/stores/authStore';
import type { Router } from 'vue-router';

/**
 * Middleware function for Vue Router.
 * This function is used to perform actions before each route navigation.
 *
 * @param router - The Vue Router instance.
 */
const routeMiddleware = (router: Router) => {
  router.beforeEach(async (to, from, next) => {
    const title = typeof to.meta.title === 'string' ? to.meta.title : 'App';
    globalThis.document.title = `${title} - SummaryYearSalary`;
    // Router guard refer link: https://pinia.vuejs.org/core-concepts/outside-component-usage.html
    const authStore = useAuthStore();
    const { clearAll } = useGlobalUi();

    // clear all previous message and states
    clearAll();

    if (to.meta.requiresAuth && !authStore.isAuthenticated) {
      next('/authorize/welcome');
      return;
    }

    next();
  });
};

export default routeMiddleware;
