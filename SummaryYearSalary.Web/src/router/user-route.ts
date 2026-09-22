export const UserRoutes = [
  {
    path: 'users/profile',
    name: 'UserProfile',
    component: () => import('@/views/user/ProfileView.vue'),
    meta: {
      title: 'โปรไฟล์',
      subtitle: 'จัดการข้อมูลบัญชี ความปลอดภัย และการเชื่อมบัญชีคู่',
      breadcrumb: [{ name: 'โปรไฟล์', url: null }],
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
