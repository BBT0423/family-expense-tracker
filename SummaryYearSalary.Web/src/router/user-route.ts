export const UserRoutes = [
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
