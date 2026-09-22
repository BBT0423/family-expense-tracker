<template>
  <el-select
    :model-value="modelValue"
    placeholder="ตัวเอง"
    style="width: 160px"
    @update:model-value="(val: string | undefined) => emit('update:modelValue', val)"
  >
    <el-option label="ตัวเอง" :value="undefined as unknown as string" />
    <el-option label="ทั้งหมด (ทุกคน)" value="all" />
    <el-option
      v-for="u in userDirectoryStore.entries"
      :key="u.id"
      :label="u.displayName"
      :value="u.id"
    />
  </el-select>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useUserDirectoryStore } from '@/stores/userDirectoryStore';

defineProps<{ modelValue?: string }>();
const emit = defineEmits<{ 'update:modelValue': [value: string | undefined] }>();

const userDirectoryStore = useUserDirectoryStore();

onMounted(() => {
  userDirectoryStore.fetchDirectory();
});
</script>
