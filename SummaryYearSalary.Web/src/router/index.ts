import { createRouter, createWebHistory } from 'vue-router';
import { AuthorizeRoutes } from './authorize-route';
import AuthorizeLayout from '@/layouts/AuthorizeLayout.vue';
import routeMiddleware from './route-middleware';
import AdminLayout from '@/layouts/AdminLayout.vue';
import { UserRoutes } from './user-route';
import { BudgetRoutes } from './budget-route';

const routes = [
  {
    path: '/',
    redirect: '/welcome',
  },
  {
    path: '/authorize',
    component: AuthorizeLayout,
    children: AuthorizeRoutes,
  },
  {
    path: '/pages',
    component: AdminLayout,
    meta: {
      requiresAuth: true,
    },
    children: [...UserRoutes, ...BudgetRoutes],
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/authorize/login',
  },
  {
    path: '/denied',
    name: 'AccessDenied',
    component: () => import('@/views/common/AccessDeniedView.vue'),
    meta: {
      title: 'Access Denied',
    },
  },
  {
    path: '/404',
    name: '404',
    component: () => import('@/views/common/NotFoundView.vue'),
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

routeMiddleware(router);

export default router;
