export interface TourStepConfig {
  title: string;
  description: string;
  /** Page the step lives on; the tour navigates there before highlighting. */
  route?: string;
  /** CSS selector of the element to highlight. Omit for a centered step. */
  selector?: string;
  placement?: 'top' | 'bottom' | 'left' | 'right' | 'top-start' | 'bottom-start' | 'right-start';
  /** Needs the desktop sidebar/header to be visible — skipped on compact screens. */
  desktopOnly?: boolean;
}

export const tourSteps: TourStepConfig[] = [
  {
    title: 'ยินดีต้อนรับ',
    description:
      'แอปนี้ช่วยบันทึกและดูสรุปรายรับ-รายจ่ายของครอบครัว ทัวร์สั้น ๆ นี้จะพาไปดูแต่ละหน้า กด "ถัดไป" ได้เลย หรือกด × เพื่อข้าม',
    route: '/pages/dashboard',
  },
  {
    title: 'เมนูหลัก',
    description: 'สลับระหว่างแดชบอร์ด รายการธุรกรรม เพิ่มรายการ นำเข้าข้อมูล และตั้งค่าได้จากเมนูด้านข้างนี้',
    route: '/pages/dashboard',
    selector: '[data-tour="sidebar-nav"]',
    placement: 'right-start',
    desktopOnly: true,
  },
  {
    title: 'เลือกเดือนและสมาชิก',
    description: 'เลือกเดือน/ปีที่ต้องการดู และกรองตามสมาชิกในบ้าน (หรือดูรวมทุกคน) ข้อมูลทั้งหน้าจะเปลี่ยนตาม',
    route: '/pages/dashboard',
    selector: '[data-tour="dash-filter"]',
    placement: 'bottom-start',
  },
  {
    title: 'สรุปประจำเดือน',
    description: 'คงเหลือ รายรับ และรายจ่ายของเดือนที่เลือก พร้อมเทียบกับเดือนก่อนหน้า',
    route: '/pages/dashboard',
    selector: '[data-tour="dash-summary"]',
    placement: 'bottom',
  },
  {
    title: 'กราฟรายรับ-รายจ่าย 12 เดือน',
    description: 'ดูแนวโน้มทั้งปี คลิกที่คอลัมน์ของเดือนใดก็ได้เพื่อเปลี่ยนไปดูเดือนนั้นทันที',
    route: '/pages/dashboard',
    selector: '[data-tour="dash-chart"]',
    placement: 'top',
  },
  {
    title: 'รายจ่ายตามหมวดหมู่',
    description: 'จัดอันดับหมวดหมู่ที่ใช้จ่ายมากที่สุดในเดือนนี้ คลิกที่หมวดหมู่เพื่อดูรายการข้างใน',
    route: '/pages/dashboard',
    selector: '[data-tour="dash-category"]',
    placement: 'left',
  },
  {
    title: 'รายการธุรกรรม',
    description: 'ค้นหาและกรองรายการตามเดือน ประเภท หมวดหมู่ และสมาชิก',
    route: '/pages/transactions',
    selector: '[data-tour="txn-filter"]',
    placement: 'bottom',
  },
  {
    title: 'จัดการรายการ',
    description:
      'แก้ไขหรือลบรายการได้ที่ปุ่มท้ายแถว ติ๊กเลือกหลายแถวเพื่อลบพร้อมกัน และเลื่อนเมาส์ที่หมวดหมู่เพื่อดูรายละเอียดและประวัติการแก้ไข',
    route: '/pages/transactions',
    selector: '[data-tour="txn-table"] .el-table__body tr',
    placement: 'bottom',
  },
  {
    title: 'เพิ่มรายการใหม่',
    description: 'เลือกรายรับ/รายจ่าย ใส่วันที่ จำนวนเงิน หมวดหมู่ และสมาชิก แล้วกดบันทึก',
    route: '/pages/transactions/add',
    selector: '[data-tour="txn-form"]',
    placement: 'right',
  },
  {
    title: 'นำเข้าจากไฟล์ Excel',
    description:
      'มีข้อมูลเดิมอยู่ในไฟล์ .xlsx หรือ .csv? อัปโหลดที่นี่ ระบบจะจับคู่คอลัมน์ให้ ตรวจแถวที่ผิดพลาด และสร้างหมวดหมู่ที่ยังไม่มีให้อัตโนมัติ (มีไฟล์ตัวอย่างให้ดาวน์โหลด)',
    route: '/pages/import',
    selector: '[data-tour="import-upload"]',
    placement: 'top',
  },
  {
    title: 'ตั้งค่า',
    description: 'เพิ่ม/แก้ไขหมวดหมู่ และจัดการสมาชิกในบ้านได้ที่นี่',
    route: '/pages/settings',
    selector: '[data-tour="settings-tabs"] .el-tabs__header',
    placement: 'bottom',
  },
  {
    title: 'ต้องการความช่วยเหลือ?',
    description: 'กดปุ่ม ? ตรงนี้เมื่อไรก็ได้ เพื่อเริ่มทัวร์ใหม่หรือเปิดคู่มือการใช้งานฉบับเต็ม',
    route: '/pages/settings',
    selector: '[data-tour="help-btn"]',
    placement: 'bottom',
    desktopOnly: false,
  },
];
