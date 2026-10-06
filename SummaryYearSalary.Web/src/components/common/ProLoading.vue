<template>
  <div class="ov" role="alertdialog" aria-busy="true" :aria-label="title">
    <div class="b-wrap">
      <div class="orb">
        <div class="track"></div>
        <div class="spin"><i></i></div>
        <div class="spin"><i></i></div>
        <div class="core">฿</div>
      </div>
      <p class="title">{{ title }}</p>
      <p v-if="hint" class="hint">{{ hint }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ title?: string; hint?: string }>(), {
  title: 'กำลังโหลด…',
  hint: 'กรุณารอสักครู่',
});
</script>

<style scoped>
.ov {
  position: fixed;
  inset: 0;
  z-index: 10000;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(8, 40, 50, 0.38);
  backdrop-filter: blur(6px) saturate(0.9);
  -webkit-backdrop-filter: blur(6px);
  animation: ovIn 0.25s ease-out both;
}
@keyframes ovIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
@keyframes cardIn {
  from {
    opacity: 0;
    transform: translateY(8px) scale(0.98);
  }
  to {
    opacity: 1;
    transform: none;
  }
}

.b-wrap {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  animation: cardIn 0.3s ease-out both;
}
.orb {
  position: relative;
  width: 120px;
  height: 120px;
  margin-bottom: 24px;
}
.orb .core {
  position: absolute;
  inset: 30px;
  border-radius: 50%;
  background: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
  font-weight: 700;
  color: #0e7490;
  box-shadow: 0 12px 30px rgba(4, 40, 52, 0.3);
  animation: pulse 1.6s ease-in-out infinite;
}
.orb .track {
  position: absolute;
  inset: 0;
  border-radius: 50%;
  border: 2px solid rgba(255, 255, 255, 0.25);
}
.orb .spin {
  position: absolute;
  inset: 0;
  animation: spin 1.6s linear infinite;
}
.orb .spin:nth-of-type(3) {
  animation-duration: 2.4s;
  animation-direction: reverse;
  inset: 12px;
}
.orb .spin i {
  position: absolute;
  top: -5px;
  left: 50%;
  margin-left: -5px;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: #fff;
  box-shadow: 0 0 12px rgba(255, 255, 255, 0.8);
}
.orb .spin:nth-of-type(3) i {
  background: #67e8f9;
  width: 8px;
  height: 8px;
  margin-left: -4px;
  top: -4px;
}
.b-wrap .title {
  color: #fff;
  font-size: 18px;
  font-weight: 700;
  margin: 0 0 4px;
}
.b-wrap .hint {
  color: rgba(255, 255, 255, 0.85);
  font-size: 13px;
  margin: 0;
}
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
@keyframes pulse {
  0%,
  100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.06);
  }
}

@media (prefers-reduced-motion: reduce) {
  * {
    animation-duration: 0.01s !important;
    animation-iteration-count: 1 !important;
  }
}
</style>
