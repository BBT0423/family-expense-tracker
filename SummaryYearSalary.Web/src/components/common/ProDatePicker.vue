<template>
  <el-date-picker
    v-bind="$attrs"
    :model-value="modelValue"
    :format="beFormat"
    :placeholder="placeholder"
    :type="type"
    value-format="YYYY-MM-DD"
    @update:model-value="$emit('update:modelValue', $event)"
  />
</template>

<script setup lang="ts">
import dayjs from 'dayjs';
import buddhistEra from 'dayjs/plugin/buddhistEra';
dayjs.extend(buddhistEra);

withDefaults(
  defineProps<{
    modelValue?: string | null;
    placeholder?: string;
    type?: 'date' | 'daterange' | 'month' | 'year';
  }>(),
  {
    modelValue: null,
    placeholder: 'เลือกวันที่',
    type: 'date',
  }
);

defineEmits<{ 'update:modelValue': [value: string | null] }>();

// Display format with Buddhist Era year (BBBB = BE year, e.g. 2568)
const beFormat = 'DD/MM/BBBB';
</script>
