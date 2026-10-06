<template>
  <div>
    <PageHeader />

    <el-tabs v-model="activeTab" data-tour="settings-tabs">
      <!-- Categories tab -->
      <el-tab-pane label="หมวดหมู่" name="categories">
        <div class="tab-actions">
          <el-button type="primary" :icon="Plus" @click="openCategoryDialog()"
            >เพิ่มหมวดหมู่</el-button
          >
        </div>
        <el-table v-loading="categoryStore.loading" :data="categoryStore.categories" stripe>
          <el-table-column label="ไอคอน" width="70" align="center">
            <template #default="{ row }">
              <span style="font-size: 1.3rem">{{ row.icon }}</span>
            </template>
          </el-table-column>
          <el-table-column label="ชื่อ" prop="name" />
          <el-table-column label="ประเภท" width="100">
            <template #default="{ row }">
              <el-tag :type="row.type === 'Income' ? 'success' : 'danger'" size="small">
                {{ row.type === 'Income' ? 'รายรับ' : 'รายจ่าย' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="ระบบ" width="80" align="center">
            <template #default="{ row }">
              <el-tag v-if="row.isDefault" type="info" size="small">ระบบ</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="" width="96" fixed="right" align="center">
            <template #default="{ row }">
              <div class="row-actions">
                <el-button
                  text
                  size="small"
                  :icon="Edit"
                  :disabled="row.isDefault"
                  @click="openCategoryDialog(row)"
                />
                <el-button
                  text
                  size="small"
                  :icon="Delete"
                  type="danger"
                  :disabled="row.isDefault"
                  @click="handleDeleteCategory(row.id)"
                />
              </div>
            </template>
          </el-table-column>
        </el-table>
      </el-tab-pane>

      <!-- Members tab -->
      <el-tab-pane label="สมาชิกในบ้าน" name="householdMembers">
        <div class="tab-actions">
          <el-button type="primary" :icon="Plus" @click="openMemberDialog()">เพิ่มสมาชิก</el-button>
        </div>
        <el-table v-loading="memberStore.loading" :data="memberStore.members" stripe>
          <el-table-column label="" width="50" align="center">
            <template #default="{ row }">
              <span
                class="color-preview"
                :style="{ background: row.color, width: '20px', height: '20px' }"
              />
            </template>
          </el-table-column>
          <el-table-column label="ชื่อ" prop="name" />
          <el-table-column label="สถานะ" width="100">
            <template #default="{ row }">
              <el-tag :type="row.isActive ? 'success' : 'info'" size="small">
                {{ row.isActive ? 'ใช้งาน' : 'ปิดใช้งาน' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="" width="96" fixed="right" align="center">
            <template #default="{ row }">
              <div class="row-actions">
                <el-button text size="small" :icon="Edit" @click="openMemberDialog(row)" />
                <el-button
                  text
                  size="small"
                  :icon="Delete"
                  type="danger"
                  @click="handleDeleteMember(row.id)"
                />
              </div>
            </template>
          </el-table-column>
        </el-table>
      </el-tab-pane>
    </el-tabs>

    <!-- Category dialog -->
    <el-dialog
      v-model="categoryDialog"
      :title="editingCategory ? 'แก้ไขหมวดหมู่' : 'เพิ่มหมวดหมู่'"
      width="420px"
    >
      <el-form :model="categoryForm" label-position="top">
        <el-form-item label="ชื่อ" required>
          <el-input v-model="categoryForm.name" placeholder="ชื่อหมวดหมู่" maxlength="100" />
        </el-form-item>
        <el-form-item label="ประเภท" required>
          <el-radio-group v-model="categoryForm.type">
            <el-radio-button value="Income">รายรับ</el-radio-button>
            <el-radio-button value="Expense">รายจ่าย</el-radio-button>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="ไอคอน (emoji)">
          <el-input
            v-model="categoryForm.icon"
            placeholder="เช่น 🍜"
            maxlength="10"
            style="width: 100px"
          />
        </el-form-item>
        <el-form-item label="สี">
          <div class="color-picker-row">
            <span class="color-preview" :style="{ background: categoryForm.color }" />
            <el-input
              v-model="categoryForm.color"
              placeholder="#4A90D9"
              maxlength="7"
              style="width: 120px"
            />
            <div class="preset-colors">
              <span
                v-for="c in presetColors"
                :key="c"
                class="preset-dot"
                :style="{ background: c }"
                @click="categoryForm.color = c"
              />
            </div>
          </div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="categoryDialog = false">ยกเลิก</el-button>
        <el-button type="primary" :loading="saving" @click="handleSaveCategory">บันทึก</el-button>
      </template>
    </el-dialog>

    <!-- Member dialog -->
    <el-dialog
      v-model="memberDialog"
      :title="editingMember ? 'แก้ไขสมาชิก' : 'เพิ่มสมาชิก'"
      width="420px"
    >
      <el-form :model="memberForm" label-position="top">
        <el-form-item label="ชื่อ" required>
          <el-input v-model="memberForm.name" placeholder="ชื่อสมาชิก" maxlength="100" />
        </el-form-item>
        <el-form-item label="สี">
          <div class="color-picker-row">
            <span class="color-preview" :style="{ background: memberForm.color }" />
            <el-input
              v-model="memberForm.color"
              placeholder="#4A90D9"
              maxlength="7"
              style="width: 120px"
            />
            <div class="preset-colors">
              <span
                v-for="c in presetColors"
                :key="c"
                class="preset-dot"
                :style="{ background: c }"
                @click="memberForm.color = c"
              />
            </div>
          </div>
        </el-form-item>
        <el-form-item v-if="editingMember" label="สถานะ">
          <el-switch v-model="memberForm.isActive" active-text="ใช้งาน" inactive-text="ปิดใช้งาน" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="memberDialog = false">ยกเลิก</el-button>
        <el-button type="primary" :loading="saving" @click="handleSaveMember">บันทึก</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { Plus, Edit, Delete } from '@element-plus/icons-vue';
import PageHeader from '@/components/admin/PageHeader.vue';
import { useCategoryStore } from '@/stores/categoryStore';
import { useMemberStore } from '@/stores/memberStore';
import { useNotify } from '@/composables/useNotify';
import type { Category, Member, TransactionType } from '@/types';

const categoryStore = useCategoryStore();
const memberStore = useMemberStore();
const { toast, confirm } = useNotify();

const activeTab = ref('categories');
const saving = ref(false);

const presetColors = [
  '#4A90D9',
  '#27ae60',
  '#e74c3c',
  '#f39c12',
  '#9b59b6',
  '#1abc9c',
  '#e67e22',
  '#3498db',
];

// Category dialog
const categoryDialog = ref(false);
const editingCategory = ref<Category | null>(null);
const categoryForm = ref({
  name: '',
  type: 'Expense' as TransactionType,
  icon: '💰',
  color: '#4A90D9',
});

function openCategoryDialog(category?: Category) {
  editingCategory.value = category ?? null;
  categoryForm.value = category
    ? { name: category.name, type: category.type, icon: category.icon, color: category.color }
    : { name: '', type: 'Expense', icon: '💰', color: '#4A90D9' };
  categoryDialog.value = true;
}

async function handleSaveCategory() {
  if (!categoryForm.value.name.trim()) {
    toast('กรุณาระบุชื่อหมวดหมู่', 'warning');
    return;
  }
  saving.value = true;
  try {
    let result;
    if (editingCategory.value) {
      result = await categoryStore.updateCategory(editingCategory.value.id, categoryForm.value);
    } else {
      result = await categoryStore.createCategory(categoryForm.value);
    }
    if (result?.isSuccess) {
      toast('บันทึกสำเร็จ', 'success');
      categoryDialog.value = false;
    } else {
      toast(result?.error ?? 'เกิดข้อผิดพลาด', 'error');
    }
  } finally {
    saving.value = false;
  }
}

async function handleDeleteCategory(id: number) {
  const ok = await confirm('ต้องการลบหมวดหมู่นี้หรือไม่?', { dangerConfirm: true });
  if (!ok) return;
  const result = await categoryStore.deleteCategory(id);
  if (result?.isSuccess) toast('ลบหมวดหมู่สำเร็จ', 'success');
  else toast(result?.error ?? 'เกิดข้อผิดพลาด', 'error');
}

// Member dialog
const memberDialog = ref(false);
const editingMember = ref<Member | null>(null);
const memberForm = ref({
  name: '',
  color: '#4A90D9',
  isActive: true,
});

function openMemberDialog(member?: Member) {
  editingMember.value = member ?? null;
  memberForm.value = member
    ? { name: member.name, color: member.color, isActive: member.isActive }
    : { name: '', color: '#4A90D9', isActive: true };
  memberDialog.value = true;
}

async function handleSaveMember() {
  if (!memberForm.value.name.trim()) {
    toast('กรุณาระบุชื่อสมาชิก', 'warning');
    return;
  }
  saving.value = true;
  try {
    let result;
    if (editingMember.value) {
      result = await memberStore.updateMember(editingMember.value.id, memberForm.value);
    } else {
      result = await memberStore.createMember(memberForm.value);
    }
    if (result?.isSuccess) {
      toast('บันทึกสำเร็จ', 'success');
      memberDialog.value = false;
    } else {
      toast(result?.error ?? 'เกิดข้อผิดพลาด', 'error');
    }
  } finally {
    saving.value = false;
  }
}

async function handleDeleteMember(id: number) {
  const ok = await confirm('ต้องการลบสมาชิกนี้หรือไม่?', { dangerConfirm: true });
  if (!ok) return;
  const result = await memberStore.deleteMember(id);
  if (result?.isSuccess) toast('ลบสมาชิกสำเร็จ', 'success');
  else toast(result?.error ?? 'เกิดข้อผิดพลาด', 'error');
}

onMounted(async () => {
  await Promise.all([categoryStore.fetchCategories(), memberStore.fetchMembers()]);
});
</script>

<style scoped>
.tab-actions {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 12px;
}
.color-preview {
  display: inline-block;
  width: 32px;
  height: 32px;
  border-radius: 6px;
  border: 1px solid var(--border, #ddd);
}
.color-picker-row {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}
.preset-colors {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}
.preset-dot {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  cursor: pointer;
  border: 2px solid transparent;
  transition: border-color 0.15s;
}
.preset-dot:hover {
  border-color: var(--text, #333);
}
</style>
