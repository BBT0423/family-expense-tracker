export const AuthorizeRoutes = [
  {
    path: 'login',
    name: 'Login',
    component: () => import('@/views/authorize/Login.vue'),
    meta: {
      title: 'Login',
    },
  },
  {
    path: 'otp',
    name: 'Otp',
    component: () => import('@/views/authorize/Otp.vue'),
    meta: {
      title: 'Verify OTP',
    },
  },
  {
    path: 'change-password',
    name: 'ChangePassword',
    component: () => import('@/views/authorize/ChangePasswordView.vue'),
    meta: {
      title: 'Change Password',
      requiresAuth: true,
    },
  },
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
    redirect: '/authorize/login',
  },
];
