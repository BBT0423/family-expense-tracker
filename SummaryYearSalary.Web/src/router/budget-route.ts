export const BudgetRoutes = [
  {
    path: '',
    redirect: { name: 'Dashboard' },
  },
  {
    path: 'dashboard',
    name: 'Dashboard',
    component: () => import('@/views/budget/DashboardView.vue'),
    meta: {
      title: 'แดชบอร์ด',
      subtitle: 'ภาพรวมรายรับ-รายจ่ายของครอบครัว',
      breadcrumb: [{ name: 'แดชบอร์ด', url: null }],
    },
  },
  {
    path: 'transactions',
    name: 'Transactions',
    component: () => import('@/views/budget/TransactionsView.vue'),
    meta: {
      title: 'รายการธุรกรรม',
      subtitle: 'ดูและจัดการรายการรายรับ-รายจ่ายทั้งหมด',
      breadcrumb: [{ name: 'รายการธุรกรรม', url: null }],
    },
  },
  {
    path: 'transactions/add',
    name: 'AddTransaction',
    component: () => import('@/views/budget/TransactionFormView.vue'),
    meta: {
      title: 'เพิ่มรายการ',
      subtitle: 'บันทึกรายรับหรือรายจ่ายใหม่',
      breadcrumb: [
        { name: 'รายการธุรกรรม', url: '/pages/transactions' },
        { name: 'เพิ่มรายการ', url: null },
      ],
    },
  },
  {
    path: 'transactions/:id/edit',
    name: 'EditTransaction',
    component: () => import('@/views/budget/TransactionFormView.vue'),
    meta: {
      title: 'แก้ไขรายการ',
      subtitle: 'แก้ไขข้อมูลรายการที่บันทึกไว้',
      breadcrumb: [
        { name: 'รายการธุรกรรม', url: '/pages/transactions' },
        { name: 'แก้ไขรายการ', url: null },
      ],
    },
  },
  {
    path: 'reports',
    name: 'Reports',
    component: () => import('@/views/budget/ReportsView.vue'),
    meta: {
      title: 'รายงาน',
      subtitle: 'วิเคราะห์และสรุปรายรับ-รายจ่าย',
      breadcrumb: [{ name: 'รายงาน', url: null }],
    },
  },
  {
    path: 'import',
    name: 'Import',
    component: () => import('@/views/budget/ImportView.vue'),
    meta: {
      title: 'นำเข้าข้อมูล',
      subtitle: 'นำเข้ารายการจากไฟล์ Excel',
      breadcrumb: [{ name: 'นำเข้าข้อมูล', url: null }],
    },
  },
  {
    path: 'settings',
    name: 'Settings',
    component: () => import('@/views/budget/SettingsView.vue'),
    meta: {
      title: 'ตั้งค่า',
      subtitle: 'จัดการสมาชิกและหมวดหมู่',
      breadcrumb: [{ name: 'ตั้งค่า', url: null }],
    },
  },
];
