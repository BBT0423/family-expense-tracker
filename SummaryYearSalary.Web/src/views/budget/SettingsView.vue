<template>
  <div>
    <PageHeader />

    <el-tabs v-model="activeTab">
      <!-- Users tab -->
      <el-tab-pane label="สมาชิกในครอบครัว" name="members">
        <el-table v-loading="userManagementStore.loading" :data="userManagementStore.users" stripe>
          <el-table-column label="ชื่อ" min-width="160">
            <template #default="{ row }">
              <div class="user-name-cell">
                <span class="user-avatar" :style="{ background: avatarColor(row.email) }">
                  {{ initials(row.firstName, row.lastName, row.email) }}
                </span>
                <span>{{ fullName(row.firstName, row.lastName, row.email) }}</span>
              </div>
            </template>
          </el-table-column>
          <el-table-column label="อีเมล" prop="email" min-width="200" />
          <el-table-column label="Role" width="90">
            <template #default="{ row }">
              <el-tag
                v-for="role in row.roles"
                :key="role"
                :type="role === 'Admin' ? 'danger' : 'primary'"
                size="small"
              >{{ role }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="สร้างเมื่อ" width="130">
            <template #default="{ row }">
              {{ formatDate(row.createdAt) }}
            </template>
          </el-table-column>
          <el-table-column label="Login ล่าสุด" width="140">
            <template #default="{ row }">
              {{ row.lastLoginAt ? formatDate(row.lastLoginAt) : '-' }}
            </template>
          </el-table-column>
        </el-table>
      </el-tab-pane>

      <!-- Family Member groups tab -->
      <el-tab-pane label="กลุ่มผู้ใช้จ่าย" name="memberGroups">
        <div class="tab-actions">
          <el-button type="primary" :icon="Plus" @click="openMemberDialog()">เพิ่มกลุ่ม</el-button>
        </div>
        <el-table v-loading="memberStore.loading" :data="memberStore.members" stripe>
          <el-table-column label="สี" width="60">
            <template #default="{ row }">
              <span class="color-dot" :style="{ background: row.color }" />
            </template>
          </el-table-column>
          <el-table-column label="ชื่อ" prop="name" />
          <el-table-column label="สถานะ" width="90">
            <template #default="{ row }">
              <el-tag :type="row.isActive ? 'success' : 'info'" size="small">
                {{ row.isActive ? 'ใช้งาน' : 'ปิดใช้' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="" width="100" fixed="right">
            <template #default="{ row }">
              <el-button text size="small" :icon="Edit" @click="openMemberDialog(row)" />
              <el-button
                text
                size="small"
                :icon="Delete"
                type="danger"
                @click="handleDeleteMember(row.id)"
              />
            </template>
          </el-table-column>
        </el-table>
      </el-tab-pane>

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
          <el-table-column label="" width="100" fixed="right">
            <template #default="{ row }">
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
            </template>
          </el-table-column>
        </el-table>
      </el-tab-pane>
    </el-tabs>

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
          <el-switch v-model="memberForm.isActive" active-text="ใช้งาน" inactive-text="ปิดใช้" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="memberDialog = false">ยกเลิก</el-button>
        <el-button type="primary" :loading="saving" @click="handleSaveMember">บันทึก</el-button>
      </template>
    </el-dialog>

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
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { Plus, Edit, Delete } from '@element-plus/icons-vue';
import PageHeader from '@/components/admin/PageHeader.vue';
import { useMemberStore } from '@/stores/memberStore';
import { useCategoryStore } from '@/stores/categoryStore';
import { useUserManagementStore } from '@/stores/userManagementStore';
import { useNotify } from '@/composables/useNotify';
import type { FamilyMember, Category, TransactionType } from '@/types';
import dayjs from 'dayjs';

const memberStore = useMemberStore();
const categoryStore = useCategoryStore();
const userManagementStore = useUserManagementStore();
const { toast, confirm } = useNotify();

// --- User tab helpers ---
function fullName(firstName?: string, lastName?: string, email?: string): string {
  const name = [firstName, lastName].filter(Boolean).join(' ');
  return name || email || '-';
}

function initials(firstName?: string, lastName?: string, email?: string): string {
  if (firstName) return firstName.charAt(0).toUpperCase();
  if (email) return email.charAt(0).toUpperCase();
  return '?';
}

const avatarColors = ['#4A90D9', '#27ae60', '#e74c3c', '#f39c12', '#9b59b6', '#1abc9c', '#e67e22'];
function avatarColor(email?: string): string {
  if (!email) return avatarColors[0];
  const idx = email.charCodeAt(0) % avatarColors.length;
  return avatarColors[idx];
}

function formatDate(val?: string): string {
  if (!val) return '-';
  return dayjs(val).format('DD/MM/YYYY');
}

const activeTab = ref('members');
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

// Member dialog
const memberDialog = ref(false);
const editingMember = ref<FamilyMember | null>(null);
const memberForm = ref({ name: '', color: '#4A90D9', isActive: true });

function openMemberDialog(member?: FamilyMember) {
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
      result = await memberStore.createMember({
        name: memberForm.value.name,
        color: memberForm.value.color,
      });
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

onMounted(async () => {
  await Promise.all([
    userManagementStore.fetchUsers(),
    memberStore.fetchMembers(),
    categoryStore.fetchCategories(),
  ]);
});
</script>

<style scoped>
.tab-actions {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 12px;
}
.color-dot {
  display: inline-block;
  width: 16px;
  height: 16px;
  border-radius: 50%;
}
.user-name-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}
.user-avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  color: #fff;
  font-weight: 600;
  font-size: 0.85rem;
  flex-shrink: 0;
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
