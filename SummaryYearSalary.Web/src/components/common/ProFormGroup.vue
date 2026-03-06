<template>
  <div class="form-group" :class="{ 'form-group-horizontal': horizontal }">
    <label v-if="label" class="form-label" :class="{ required: required }" :for="forId">
      {{ label }} <span v-if="required">*</span>
      <span
        v-if="info || $slots.info"
        class="form-info-icon"
        @mouseenter="show = true"
        @mouseleave="show = false"
      >
        ℹ️
        <div v-if="show" class="form-info-tooltip">
          <slot name="info">{{ info }}</slot>
        </div>
      </span>
    </label>
    <slot></slot>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
const props = defineProps({
  label: { type: String, default: '' },
  for: { type: String, default: '' },
  required: { type: Boolean, default: false },
  info: { type: String, default: '' },
  horizontal: { type: Boolean, default: false },
});
const forId = props.for;
const show = ref(false);
</script>
<style scoped>
.form-group-horizontal {
  display: flex;
  align-items: center;
  gap: 10px;
}
.form-group-horizontal .form-label {
  white-space: nowrap;
}
</style>
