export const UserRoutes = [
  {
    path: '',
    redirect: { name: 'UserProfile' },
  },
  {
    path: 'users/profile',
    name: 'UserProfile',
    component: () => import('@/views/user/ProfileView.vue'),
    meta: {
      title: 'User Profile',
      subtitle: 'Manage your account information and settings',
      breadcrumb: [
        { name: 'User Profile', url: '/users/profile' },
        { name: 'Details', url: null },
      ],
    },
  },
  {
    path: 'ui-test',
    name: 'UiTest',
    component: () => import('@/views/common/UiTestView.vue'),
    meta: {
      title: 'UI Component Test',
      subtitle: 'ทดสอบ common UI components',
      breadcrumb: [{ name: 'UI Test', url: null }],
    },
  },
];
