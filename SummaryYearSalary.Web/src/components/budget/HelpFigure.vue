<template>
  <figure class="help-fig">
    <el-image
      :src="src"
      :alt="alt"
      :preview-src-list="[src]"
      :style="{ aspectRatio: ratio }"
      fit="contain"
      lazy
      hide-on-click-modal
      preview-teleported
    />
    <figcaption>คลิกที่ภาพเพื่อขยาย · ตัวเลขในภาพตรงกับรายการด้านล่าง</figcaption>
  </figure>
</template>

<script setup lang="ts">
defineProps<{ src: string; alt: string; ratio: number }>();
</script>

<style scoped>
.help-fig {
  margin: 12px 0;
  border: 1px solid var(--border);
  border-radius: 12px;
  overflow: hidden;
  background: #fff;
}

.help-fig :deep(.el-image) {
  display: block;
  width: 100%;
  cursor: zoom-in;
}

.help-fig figcaption {
  padding: 6px 12px;
  border-top: 1px solid var(--border);
  background: var(--surface-dark);
  font-size: 12px;
  color: var(--text-muted);
}
</style>
