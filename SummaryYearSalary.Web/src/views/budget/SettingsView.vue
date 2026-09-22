<template>
  <div>
    <PageHeader />

    <el-tabs v-model="activeTab">
      <!-- Users tab (Admin only) -->
      <el-tab-pane v-if="authStore.isAdmin" label="ผู้ใช้ในระบบ" name="members">
        <div class="tab-actions">
          <el-button type="primary" :icon="Plus" @click="openUserDialog()">เพิ่มผู้ใช้</el-button>
        </div>
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
                >{{ role }}</el-tag
              >
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
          <el-table-column label="" width="60" fixed="right" align="center">
            <template #default="{ row }">
              <el-button text size="small" :icon="Edit" @click="openEditUserDialog(row)" />
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

    <!-- Edit user dialog -->
    <el-dialog v-model="editUserDialog" title="แก้ไขผู้ใช้" width="420px">
      <el-form :model="editUserForm" label-position="top">
        <el-form-item label="ชื่อ" required>
          <el-input v-model="editUserForm.firstName" placeholder="ชื่อ" maxlength="100" />
        </el-form-item>
        <el-form-item label="นามสกุล">
          <el-input v-model="editUserForm.lastName" placeholder="นามสกุล" maxlength="100" />
        </el-form-item>
        <el-form-item label="บทบาท" required>
          <el-radio-group v-model="editUserForm.role">
            <el-radio-button value="User">User</el-radio-button>
            <el-radio-button value="Admin">Admin</el-radio-button>
          </el-radio-group>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="editUserDialog = false">ยกเลิก</el-button>
        <el-button type="primary" :loading="saving" @click="handleSaveEditUser">บันทึก</el-button>
      </template>
    </el-dialog>

    <!-- Create user dialog -->
    <el-dialog v-model="createUserDialog" title="เพิ่มผู้ใช้" width="420px">
      <el-form :model="createUserForm" label-position="top">
        <el-form-item label="ชื่อ" required>
          <el-input v-model="createUserForm.firstName" placeholder="ชื่อ" maxlength="100" />
        </el-form-item>
        <el-form-item label="นามสกุล">
          <el-input v-model="createUserForm.lastName" placeholder="นามสกุล" maxlength="100" />
        </el-form-item>
        <el-form-item label="อีเมล" required>
          <el-input v-model="createUserForm.email" placeholder="you@example.com" />
        </el-form-item>
        <el-form-item label="รหัสผ่าน" required>
          <el-input
            v-model="createUserForm.password"
            type="password"
            show-password
            placeholder="อย่างน้อย 8 ตัว มีตัวใหญ่/เล็ก/เลข/อักขระพิเศษ"
          />
        </el-form-item>
        <el-form-item label="บทบาท" required>
          <el-radio-group v-model="createUserForm.role">
            <el-radio-button value="User">User</el-radio-button>
            <el-radio-button value="Admin">Admin</el-radio-button>
          </el-radio-group>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createUserDialog = false">ยกเลิก</el-button>
        <el-button type="primary" :loading="saving" @click="handleCreateUser"
          >สร้างผู้ใช้</el-button
        >
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { formatThaiDate as formatDate } from '@/utils/formatters';
import { ref, onMounted } from 'vue';
import { Plus, Edit, Delete } from '@element-plus/icons-vue';
import PageHeader from '@/components/admin/PageHeader.vue';
import { useCategoryStore } from '@/stores/categoryStore';
import { useUserManagementStore } from '@/stores/userManagementStore';
import { useAuthStore } from '@/stores/authStore';
import { useNotify } from '@/composables/useNotify';
import type { Category, TransactionType, UserProfile } from '@/types';

const categoryStore = useCategoryStore();
const userManagementStore = useUserManagementStore();
const authStore = useAuthStore();
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

const activeTab = ref(authStore.isAdmin ? 'members' : 'categories');
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

// Edit user name dialog (Admin only)
const editUserDialog = ref(false);
const editingUser = ref<UserProfile | null>(null);
const editUserForm = ref({ firstName: '', lastName: '', role: 'User' as 'Admin' | 'User' });

function openEditUserDialog(user: UserProfile) {
  editingUser.value = user;
  editUserForm.value = {
    firstName: user.firstName,
    lastName: user.lastName,
    role: user.roles.includes('Admin') ? 'Admin' : 'User',
  };
  editUserDialog.value = true;
}

async function handleSaveEditUser() {
  if (!editingUser.value) return;
  if (!editUserForm.value.firstName.trim()) {
    toast('กรุณาระบุชื่อ', 'warning');
    return;
  }
  saving.value = true;
  try {
    const nameResult = await userManagementStore.updateUser(editingUser.value.id, {
      firstName: editUserForm.value.firstName,
      lastName: editUserForm.value.lastName,
    });
    if (!nameResult?.isSuccess) {
      toast(nameResult?.error ?? 'เกิดข้อผิดพลาด', 'error');
      return;
    }

    const roleChanged =
      editUserForm.value.role !== (editingUser.value.roles.includes('Admin') ? 'Admin' : 'User');
    if (roleChanged) {
      const roleResult = await userManagementStore.updateUserRole(
        editingUser.value.id,
        editUserForm.value.role
      );
      if (!roleResult?.isSuccess) {
        toast(roleResult?.error ?? 'เกิดข้อผิดพลาด', 'error');
        return;
      }
    }

    toast('บันทึกสำเร็จ', 'success');
    editUserDialog.value = false;
  } finally {
    saving.value = false;
  }
}

// Create user dialog (Admin only)
const createUserDialog = ref(false);
const createUserForm = ref({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  role: 'User' as 'Admin' | 'User',
});

function openUserDialog() {
  createUserForm.value = { firstName: '', lastName: '', email: '', password: '', role: 'User' };
  createUserDialog.value = true;
}

async function handleCreateUser() {
  const { firstName, email, password } = createUserForm.value;
  if (!firstName.trim() || !email.trim() || !password) {
    toast('กรุณากรอกข้อมูลให้ครบถ้วน', 'warning');
    return;
  }
  saving.value = true;
  try {
    const result = await userManagementStore.createUser(createUserForm.value);
    if (result?.isSuccess) {
      toast('สร้างผู้ใช้สำเร็จ', 'success');
      createUserDialog.value = false;
    } else {
      toast(result?.error ?? 'เกิดข้อผิดพลาด', 'error');
    }
  } finally {
    saving.value = false;
  }
}

onMounted(async () => {
  await Promise.all([
    authStore.isAdmin ? userManagementStore.fetchUsers() : Promise.resolve(),
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
