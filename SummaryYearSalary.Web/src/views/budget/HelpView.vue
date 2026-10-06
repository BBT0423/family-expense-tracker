<template>
  <div>
    <PageHeader>
      <template #actions-left>
        <el-button type="primary" :icon="VideoPlay" @click="startTour">เริ่มทัวร์แนะนำ</el-button>
      </template>
    </PageHeader>

    <div class="help-layout">
      <nav class="help-toc" aria-label="สารบัญ">
        <div class="help-toc-title">สารบัญ</div>
        <button
          v-for="s in sections"
          :key="s.id"
          type="button"
          class="help-toc-item"
          :class="{ active: activeId === s.id }"
          @click="scrollTo(s.id)"
        >
          {{ s.title }}
        </button>
      </nav>

      <div class="help-content">
        <el-card id="start" shadow="never" class="help-card">
          <h2>เริ่มต้นใช้งาน</h2>
          <p>
            แอปนี้ใช้บันทึกและสรุปรายรับ-รายจ่ายของครอบครัว เปิดแอปแล้วเข้าใช้งานได้ทันที
            ไม่ต้องล็อกอิน ข้อมูลทั้งหมดเก็บอยู่ในเครื่องของคุณ
          </p>
          <p>เมนูด้านซ้ายพาไปยังแต่ละหน้า:</p>
          <ul>
            <li><b>แดชบอร์ด</b> — ภาพรวมรายรับ-รายจ่ายของเดือนที่เลือก</li>
            <li><b>รายการธุรกรรม</b> — ดู ค้นหา แก้ไข และลบรายการทั้งหมด</li>
            <li><b>เพิ่มรายการ</b> — บันทึกรายรับหรือรายจ่ายใหม่</li>
            <li><b>นำเข้าข้อมูล</b> — นำเข้ารายการจำนวนมากจากไฟล์ Excel</li>
            <li><b>ตั้งค่า</b> — จัดการหมวดหมู่และสมาชิกในบ้าน</li>
          </ul>
          <p class="help-tip">
            ปุ่ม <b>?</b> มุมขวาบนของทุกหน้า ใช้เริ่มทัวร์แนะนำใหม่หรือเปิดคู่มือนี้ได้ตลอด
          </p>
        </el-card>

        <el-card id="dashboard" shadow="never" class="help-card">
          <h2>แดชบอร์ด</h2>
          <HelpFigure
            :src="imgDashboard"
            :ratio="1.2204"
            alt="หน้าแดชบอร์ด พร้อมเลขกำกับ 1-4"
          />
          <ol class="callouts">
            <li>
              <span class="num">1</span>
              <div>
                <b>เลือกเดือนและสมาชิก</b> — ใช้ช่องเลือกเดือน/ปี (เลือกเดือนในอนาคตไม่ได้)
                หรือคลิกที่คอลัมน์ของเดือนในกราฟเพื่อเปลี่ยนเดือนทันที
                และสลับระหว่าง "ทุกคน" กับสมาชิกแต่ละคน ตัวเลขทั้งหน้าจะเปลี่ยนตาม
              </div>
            </li>
            <li>
              <span class="num">2</span>
              <div>
                <b>การ์ดสรุป</b> — คงเหลือ รายรับ และรายจ่ายของเดือนที่เลือก
                พร้อมลูกศรบอกว่าเพิ่มขึ้นหรือลดลงกี่เปอร์เซ็นต์เมื่อเทียบกับเดือนก่อน
              </div>
            </li>
            <li>
              <span class="num">3</span>
              <div>
                <b>กราฟ 12 เดือน</b> — เดือนที่เลือกมีแถบสีฟ้าและตัวอักษรหนา
                กดชื่อ "รายรับ" หรือ "รายจ่าย" ที่มุมการ์ดเพื่อซ่อน/แสดงแท่งนั้น
                (ซ่อนทั้งสองพร้อมกันไม่ได้)
              </div>
            </li>
            <li>
              <span class="num">4</span>
              <div>
                <b>รายจ่ายตามหมวดหมู่</b> — จัดอันดับหมวดหมู่ คลิกที่หมวดหมู่เพื่อดูรายการข้างใน
                และกด "เปิดในหน้ารายการ" เพื่อไปดูต่อในหน้ารายการธุรกรรมที่กรองไว้แล้ว
                กด "ทั้งหมด N หมวด" เพื่อดูตารางครบทุกหมวด
              </div>
            </li>
          </ol>
          <ul>
            <li><b>ทุกช่วงเวลา</b> — แถวยอดรวมตั้งแต่เริ่มบันทึก ไม่เปลี่ยนตามเดือนที่เลือก</li>
            <li><b>Export Excel</b> — ส่งออกรายการของเดือนที่เลือกเป็นไฟล์ Excel</li>
          </ul>
        </el-card>

        <el-card id="add" shadow="never" class="help-card">
          <h2>เพิ่มรายการ</h2>
          <p>กด "เพิ่มรายการ" แล้วกรอกข้อมูลตามลำดับ:</p>
          <HelpFigure
            :src="imgAddForm"
            :ratio="1.6"
            alt="ฟอร์มเพิ่มรายการ พร้อมเลขกำกับ 1-7"
          />
          <ol class="callouts">
            <li><span class="num">1</span><div>เลือกประเภท <b>รายรับ</b> หรือ <b>รายจ่าย</b></div></li>
            <li><span class="num">2</span><div>เลือกวันที่</div></li>
            <li><span class="num">3</span><div>ใส่จำนวนเงิน</div></li>
            <li>
              <span class="num">4</span>
              <div>เลือกหมวดหมู่ (รายการหมวดหมู่จะตรงกับประเภทที่เลือก)</div>
            </li>
            <li><span class="num">5</span><div>เลือกว่าเป็นของสมาชิกคนไหน</div></li>
            <li><span class="num">6</span><div>ใส่หมายเหตุถ้าต้องการ</div></li>
            <li><span class="num">7</span><div>กดปุ่ม "เพิ่มรายการ" เพื่อบันทึก</div></li>
          </ol>
          <p class="help-tip">
            ถ้ายังไม่มีหมวดหมู่หรือสมาชิกที่ต้องการ ให้ไปเพิ่มที่หน้า "ตั้งค่า" ก่อน
          </p>
        </el-card>

        <el-card id="transactions" shadow="never" class="help-card">
          <h2>รายการธุรกรรม</h2>
          <HelpFigure
            :src="imgTransactions"
            :ratio="1.6"
            alt="หน้ารายการธุรกรรม พร้อมเลขกำกับ 1-5"
          />
          <ol class="callouts">
            <li>
              <span class="num">1</span>
              <div>
                <b>กรองรายการ</b> — ตามเดือน/ปี (เว้นว่างเพื่อดูทุกเดือน) ประเภท หมวดหมู่ สมาชิก
                หรือค้นหาจากหมายเหตุ
              </div>
            </li>
            <li>
              <span class="num">2</span>
              <div>ติ๊กช่องหน้าแถวที่ต้องการ (ติ๊กที่หัวตารางเพื่อเลือกทั้งหมดในหน้า)</div>
            </li>
            <li>
              <span class="num">3</span>
              <div>
                ปุ่ม <b>"ลบที่เลือก"</b> จะโผล่ที่ด้านบนเมื่อมีแถวที่ติ๊กไว้
                มีกล่องยืนยันและแถบแสดงความคืบหน้าระหว่างลบ
              </div>
            </li>
            <li>
              <span class="num">4</span>
              <div><b>แก้ไข / ลบ</b> — ปุ่มท้ายแถว การลบจะมีกล่องยืนยันก่อนเสมอ</div>
            </li>
            <li>
              <span class="num">5</span>
              <div>
                <b>ยอดรวมทั้งหมด</b> — แสดงใต้ตาราง ไม่เปลี่ยนตามตัวกรองวันที่
                (แต่เปลี่ยนตามสมาชิกที่เลือก)
              </div>
            </li>
          </ol>
          <h3>ดูรายละเอียดและประวัติการแก้ไข</h3>
          <HelpFigure
            :src="imgHover"
            :ratio="1.6"
            alt="กล่องรายละเอียดรายการที่ขึ้นเมื่อเลื่อนเมาส์ไปที่หมวดหมู่ พร้อมเลขกำกับ 1-3"
          />
          <ol class="callouts">
            <li>
              <span class="num">1</span>
              <div>เลื่อนเมาส์ไปที่ <b>ชื่อหมวดหมู่</b> ของแถว (มีเส้นประใต้ชื่อ)</div>
            </li>
            <li>
              <span class="num">2</span>
              <div>จะเห็นรายละเอียด และ <b>ประวัติ</b> การแก้ไขของรายการนั้น</div>
            </li>
            <li>
              <span class="num">3</span>
              <div>ปุ่ม <b>แก้ไข / ทำซ้ำ / ลบ</b> — "ทำซ้ำ" ใช้สร้างรายการใหม่จากรายการนี้</div>
            </li>
          </ol>
        </el-card>

        <el-card id="import" shadow="never" class="help-card">
          <h2>นำเข้าข้อมูลจาก Excel</h2>
          <p>
            เหมาะกับการย้ายข้อมูลเดิมเข้ามาทีละมาก ๆ รองรับไฟล์ <b>.xlsx</b> และ <b>.csv</b>
            ไฟล์ควรมีคอลัมน์ตามนี้:
          </p>
          <el-table :data="importColumns" size="small" class="help-table">
            <el-table-column prop="name" label="คอลัมน์" width="110" />
            <el-table-column label="จำเป็น" width="90">
              <template #default="{ row }">{{ row.required ? 'จำเป็น' : 'ไม่บังคับ' }}</template>
            </el-table-column>
            <el-table-column prop="note" label="รูปแบบ" />
            <el-table-column prop="example" label="ตัวอย่าง" width="150" />
          </el-table>

          <h3>ขั้นที่ 1: อัปโหลดไฟล์</h3>
          <HelpFigure
            :src="imgImportUpload"
            :ratio="1.7998"
            alt="หน้าอัปโหลดไฟล์ Excel พร้อมเลขกำกับ 1-2"
          />
          <ol class="callouts">
            <li><span class="num">1</span><div>คลิกหรือลากไฟล์ .xlsx / .csv มาวางในกรอบ</div></li>
            <li>
              <span class="num">2</span>
              <div>ยังไม่มีไฟล์? กด <b>"ดาวน์โหลดไฟล์ตัวอย่าง"</b> เพื่อดูรูปแบบที่ถูกต้อง</div>
            </li>
          </ol>

          <h3>ขั้นที่ 2: แมป + ตรวจสอบ</h3>
          <HelpFigure
            :src="imgImportMap"
            :ratio="1.3841"
            alt="หน้าแมปคอลัมน์และตรวจสอบข้อมูล พร้อมเลขกำกับ 1-4"
          />
          <ol class="callouts">
            <li>
              <span class="num">1</span>
              <div>
                ระบบจับคู่คอลัมน์ให้อัตโนมัติ ถ้าไม่ตรงให้เปลี่ยนที่ <b>ช่องเลือกบนหัวตาราง</b>
                ผลตรวจสอบจะอัปเดตทันที
              </div>
            </li>
            <li>
              <span class="num">2</span>
              <div>
                ช่องที่ข้อมูลผิดจะมี <b>กรอบสีแดง</b> (เช่น จำนวนเงินที่ไม่ใช่ตัวเลข)
                แถวที่ผิดจะถูกข้ามตอนนำเข้า
              </div>
            </li>
            <li>
              <span class="num">3</span>
              <div>
                ป้าย <b>"หมวดหมู่ใหม่"</b> / "สมาชิกใหม่" หมายถึงยังไม่มีในระบบ
                จะถูกสร้างให้อัตโนมัติตอนนำเข้า
              </div>
            </li>
            <li>
              <span class="num">4</span>
              <div>กด <b>"นำเข้า N รายการ"</b> เพื่อเริ่มบันทึก (นับเฉพาะแถวที่ถูกต้อง)</div>
            </li>
          </ol>
          <p>
            เมื่อเสร็จ กด "ดาวน์โหลดแถวที่ข้าม" เพื่อนำแถวที่ผิดไปแก้ไข แล้วอัปโหลดใหม่ได้
          </p>
          <p class="help-tip">
            ถ้าไฟล์ใหญ่ (หลักพันแถว) ระบบจะแสดงความคืบหน้าและบันทึกเป็นชุด ๆ
            หากเกิดข้อผิดพลาดกลางทาง รายการที่บันทึกไปแล้วจะยังอยู่ ไม่ถูกย้อนกลับ
          </p>
        </el-card>

        <el-card id="settings" shadow="never" class="help-card">
          <h2>ตั้งค่า: หมวดหมู่และสมาชิก</h2>
          <HelpFigure
            :src="imgSettings"
            :ratio="1.6"
            alt="หน้าตั้งค่า แท็บหมวดหมู่ พร้อมเลขกำกับ 1-3"
          />
          <ol class="callouts">
            <li>
              <span class="num">1</span>
              <div>สลับระหว่างแท็บ <b>หมวดหมู่</b> และ <b>สมาชิกในบ้าน</b></div>
            </li>
            <li>
              <span class="num">2</span>
              <div>
                กดปุ่มเพิ่ม (เพิ่มหมวดหมู่ / เพิ่มสมาชิก) — หมวดหมู่ระบุชื่อ ประเภท
                (รายรับ/รายจ่าย) ไอคอน (emoji) และสี ส่วนสมาชิกระบุชื่อและสี
              </div>
            </li>
            <li>
              <span class="num">3</span>
              <div>
                หมวดหมู่ที่มีป้าย <b>"ระบบ"</b> เป็นหมวดหมู่เริ่มต้น แก้ไขหรือลบไม่ได้
              </div>
            </li>
          </ol>
          <p>
            สมาชิกใช้เลือกว่ารายการเป็นของใคร และแก้ไขสถานะของสมาชิกได้ในหน้าแก้ไข
          </p>
        </el-card>

        <el-card id="security" shadow="never" class="help-card">
          <h2>ล็อกหน้าจอและโหมดมืด</h2>
          <ul>
            <li>
              <b>ล็อกหน้าจออัตโนมัติ</b> — เมื่อไม่ได้ใช้งานนาน 30 นาที ระบบจะซ่อนข้อมูล
              กดปุ่ม "แตะเพื่อดูต่อ" เพื่อกลับมาใช้งาน
            </li>
            <li><b>โหมดมืด</b> — กดปุ่มรูปพระจันทร์/พระอาทิตย์มุมขวาบนเพื่อสลับธีม ระบบจำค่าที่เลือกไว้</li>
          </ul>
        </el-card>

        <el-card id="faq" shadow="never" class="help-card">
          <h2>คำถามที่พบบ่อย</h2>
          <el-collapse>
            <el-collapse-item title="ข้อมูลของฉันเก็บอยู่ที่ไหน?" name="1">
              เก็บอยู่ในเครื่องคอมพิวเตอร์เครื่องนี้เท่านั้น ไม่ได้ส่งขึ้นอินเทอร์เน็ต
              ควรสำรองไฟล์ข้อมูลเป็นระยะ หรือใช้ "Export Excel" เก็บไว้อีกชั้น
            </el-collapse-item>
            <el-collapse-item title="ปิดโปรแกรมตอนกำลังบันทึกหรือนำเข้าได้ไหม?" name="2">
              ถ้ายังมีงานบันทึกค้างอยู่ ระบบจะขึ้นกล่องเตือนก่อนปิด แนะนำให้รอจนเสร็จก่อน
              เพื่อไม่ให้ข้อมูลที่นำเข้าไม่ครบ
            </el-collapse-item>
            <el-collapse-item title="นำเข้าแล้วบางแถวหายไป?" name="3">
              แถวที่วันที่ ประเภท หรือจำนวนเงินไม่ถูกต้องจะถูกข้าม
              ดาวน์โหลด "แถวที่ข้าม" เพื่อดูสาเหตุ แก้ไขแล้วนำเข้าอีกครั้ง
            </el-collapse-item>
            <el-collapse-item title="เลือกเดือนในอนาคตไม่ได้?" name="4">
              ถูกต้องแล้ว แดชบอร์ดให้เลือกได้ถึงเดือนปัจจุบันเท่านั้น
            </el-collapse-item>
            <el-collapse-item title="อยากดูทัวร์แนะนำอีกครั้ง?" name="5">
              กดปุ่ม ? มุมขวาบน แล้วเลือก "เริ่มทัวร์แนะนำใหม่" หรือกดปุ่ม "เริ่มทัวร์แนะนำ" ที่หัวหน้านี้
            </el-collapse-item>
          </el-collapse>
        </el-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue';
import { VideoPlay } from '@element-plus/icons-vue';
import PageHeader from '@/components/admin/PageHeader.vue';
import { useTourStore } from '@/stores/tourStore';
import imgDashboard from '@/assets/help/dashboard.webp';
import imgTransactions from '@/assets/help/transactions.webp';
import imgHover from '@/assets/help/transaction-hover.webp';
import imgAddForm from '@/assets/help/add-form.webp';
import imgImportUpload from '@/assets/help/import-upload.webp';
import imgImportMap from '@/assets/help/import-map.webp';
import imgSettings from '@/assets/help/settings.webp';

const tourStore = useTourStore();

const sections = [
  { id: 'start', title: 'เริ่มต้นใช้งาน' },
  { id: 'dashboard', title: 'แดชบอร์ด' },
  { id: 'add', title: 'เพิ่มรายการ' },
  { id: 'transactions', title: 'รายการธุรกรรม' },
  { id: 'import', title: 'นำเข้าจาก Excel' },
  { id: 'settings', title: 'ตั้งค่า' },
  { id: 'security', title: 'ล็อกหน้าจอ / โหมดมืด' },
  { id: 'faq', title: 'คำถามที่พบบ่อย' },
];

const importColumns = [
  { name: 'วันที่', required: true, note: 'DD/MM/YYYY (ปี ค.ศ. หรือ พ.ศ. ก็ได้)', example: '01/04/2026' },
  { name: 'ประเภท', required: true, note: 'รายรับ / รายจ่าย (หรือ Income / Expense)', example: 'รายจ่าย' },
  { name: 'หมวดหมู่', required: true, note: 'ชื่อหมวดหมู่ ถ้ายังไม่มีจะสร้างให้อัตโนมัติ', example: 'อาหาร/เครื่องดื่ม' },
  { name: 'จำนวนเงิน', required: true, note: 'ตัวเลข', example: '150.00' },
  { name: 'หมายเหตุ', required: false, note: 'ข้อความอะไรก็ได้', example: 'ข้าวเที่ยง' },
  { name: 'สมาชิก', required: false, note: 'ชื่อสมาชิก ถ้ายังไม่มีจะสร้างให้อัตโนมัติ', example: 'แม่' },
];

const activeId = ref(sections[0].id);

// The last section can't always reach the top of the viewport, so a click
// pins the highlight to the clicked item until the smooth scroll settles.
let pinnedUntil = 0;

function scrollTo(id: string) {
  activeId.value = id;
  pinnedUntil = Date.now() + 900;
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function startTour() {
  tourStore.start();
}

let observer: IntersectionObserver | null = null;
onMounted(() => {
  observer = new IntersectionObserver(
    (entries) => {
      if (Date.now() < pinnedUntil) return;
      const visible = entries.filter((e) => e.isIntersecting);
      if (visible.length) activeId.value = visible[0].target.id;
    },
    { rootMargin: '-80px 0px -60% 0px' }
  );
  sections.forEach((s) => {
    const el = document.getElementById(s.id);
    if (el) observer!.observe(el);
  });
});
onBeforeUnmount(() => observer?.disconnect());
</script>

<style scoped>
.help-layout {
  display: grid;
  grid-template-columns: 200px minmax(0, 1fr);
  gap: 20px;
  align-items: start;
}

.help-toc {
  position: sticky;
  top: calc(var(--header-height, 56px) + 16px);
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: 12px;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-control, 10px);
}

.help-toc-title {
  padding: 0 8px 6px;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.07em;
  color: var(--text-muted);
}

.help-toc-item {
  text-align: left;
  padding: 7px 10px;
  border: none;
  border-radius: 8px;
  background: none;
  font: inherit;
  font-size: 13px;
  font-weight: 600;
  color: var(--text-muted);
  cursor: pointer;
  transition:
    background 0.15s ease,
    color 0.15s ease;
}

.help-toc-item:hover {
  background: var(--surface-dark);
  color: var(--text);
}

.help-toc-item.active {
  background: var(--active-nav-bg);
  color: var(--active-nav-text);
}

.help-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-width: 0;
}

.help-card {
  scroll-margin-top: calc(var(--header-height, 56px) + 16px);
}

.help-card h2 {
  margin: 0 0 10px;
  font-size: 1.1rem;
  font-weight: 800;
  color: var(--text);
}

.help-card p,
.help-card li {
  font-size: 0.9rem;
  line-height: 1.7;
  color: var(--text);
}

.help-card ul,
.help-card ol {
  margin: 8px 0;
  padding-left: 22px;
}

.help-card li + li {
  margin-top: 4px;
}

.help-card h3 {
  margin: 18px 0 6px;
  font-size: 0.95rem;
  font-weight: 800;
  color: var(--text);
}

.callouts {
  list-style: none;
  padding-left: 0;
}

.callouts li {
  display: flex;
  align-items: flex-start;
  gap: 10px;
}

.callouts .num {
  flex: none;
  width: 24px;
  height: 24px;
  margin-top: 2px;
  border-radius: 50%;
  background: #0891b2;
  color: #fff;
  font-size: 13px;
  font-weight: 700;
  line-height: 24px;
  text-align: center;
}

.help-tip {
  margin: 12px 0 0;
  padding: 10px 12px;
  border-radius: 10px;
  background: var(--active-nav-bg);
  color: var(--active-nav-text);
}

.help-card .help-tip {
  color: var(--active-nav-text);
}

.help-table {
  margin: 10px 0;
}

.help-steps {
  margin-top: 12px;
}

@media (max-width: 900px) {
  .help-layout {
    grid-template-columns: minmax(0, 1fr);
  }

  .help-toc {
    position: static;
    flex-direction: row;
    flex-wrap: wrap;
  }

  .help-toc-title {
    width: 100%;
  }
}
</style>
