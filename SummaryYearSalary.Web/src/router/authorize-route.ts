export const AuthorizeRoutes = [
  {
    path: 'welcome',
    name: 'Welcome',
    component: () => import('@/views/authorize/Welcome.vue'),
    meta: {
      title: 'Welcome',
    },
  },
  {
    path: 'forbidden',
    name: 'Forbidden',
    component: () => import('@/views/authorize/Forbidden.vue'),
    meta: {
      title: 'Access Denied',
    },
  },
  {
    path: '',
    redirect: '/authorize/welcome',
  },
];
