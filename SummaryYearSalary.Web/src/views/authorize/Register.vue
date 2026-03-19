<template>
  <div class="auth-login-container">
    <div class="auth-logo">
      <h1 class="auth-welcome-text">สมัครสมาชิก</h1>
      <p class="auth-subtitle">สร้างบัญชีใหม่</p>
    </div>
    <ProMessage />
    <form @submit.prevent="register">
      <ProGrid :columns="1">
        <div class="form-row">
          <div class="form-group">
            <label class="form-label" for="firstName">ชื่อ *</label>
            <input
              id="firstName"
              v-model="form.firstName"
              class="form-input"
              type="text"
              placeholder="ชื่อ"
              autocomplete="given-name"
              required
            />
            <div v-if="errors.firstName" class="form-error">{{ errors.firstName }}</div>
          </div>
          <div class="form-group">
            <label class="form-label" for="lastName">นามสกุล</label>
            <input
              id="lastName"
              v-model="form.lastName"
              class="form-input"
              type="text"
              placeholder="นามสกุล"
              autocomplete="family-name"
            />
          </div>
        </div>

        <div class="form-group">
          <label class="form-label" for="email">Email *</label>
          <input
            id="email"
            v-model="form.email"
            class="form-input"
            type="email"
            placeholder="your@email.com"
            autocomplete="email"
            required
          />
          <div v-if="errors.email" class="form-error">{{ errors.email }}</div>
        </div>

        <div class="form-group">
          <label class="form-label" for="password">รหัสผ่าน *</label>
          <input
            id="password"
            v-model="form.password"
            class="form-input"
            type="password"
            placeholder="อย่างน้อย 8 ตัวอักษร"
            autocomplete="new-password"
            required
          />
          <div v-if="errors.password" class="form-error">{{ errors.password }}</div>
        </div>

        <div class="form-group mb-4">
          <label class="form-label" for="confirmPassword">ยืนยันรหัสผ่าน *</label>
          <input
            id="confirmPassword"
            v-model="form.confirmPassword"
            class="form-input"
            type="password"
            placeholder="กรอกรหัสผ่านอีกครั้ง"
            autocomplete="new-password"
            required
          />
          <div v-if="errors.confirmPassword" class="form-error">{{ errors.confirmPassword }}</div>
        </div>

        <button
          type="submit"
          class="btn btn-primary btn-full"
          :class="{ 'btn-loading': loading }"
          :disabled="loading"
        >
          <span v-if="loading">กำลังสมัคร...</span>
          <span v-else>สมัครสมาชิก</span>
        </button>
      </ProGrid>
    </form>

    <p class="login-link">
      มีบัญชีแล้ว?
      <router-link to="/authorize/login">เข้าสู่ระบบ</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import { useRegister } from '@/composables/useRegister';

const { form, errors, loading, register } = useRegister();
</script>

<style scoped>
.form-error {
  color: #e53e3e;
  font-size: 0.97em;
  margin-top: 4px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.login-link {
  text-align: center;
  font-size: 14px;
  color: #718096;
  margin-top: 1rem;
}

.login-link a {
  color: #667eea;
  text-decoration: none;
  font-weight: 500;
}

.login-link a:hover {
  text-decoration: underline;
}
</style>
