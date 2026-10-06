<template>
  <Teleport to="body">
    <div
      v-if="visible"
      class="ov"
      :class="{ finished, out: closing }"
      role="alertdialog"
      aria-busy="true"
      :aria-label="title"
      @click="onOverlayClick"
    >
      <div class="a-card">
        <div class="sheet">
          <div class="row">
            <span class="dot" style="background: #0891b2"></span>
            <span class="ln" style="width: 64px"></span>
            <span class="tag" style="background: #0891b2">อาหาร</span>
            <span class="ok">
              <svg
                width="9"
                height="9"
                viewBox="0 0 24 24"
                fill="none"
                stroke="#fff"
                stroke-width="4"
                stroke-linecap="round"
                stroke-linejoin="round"
              >
                <polyline points="20 6 9 17 4 12" />
              </svg>
            </span>
          </div>
          <div class="row">
            <span class="dot" style="background: #f59e0b"></span>
            <span class="ln" style="width: 48px"></span>
            <span class="tag" style="background: #f59e0b">เดินทาง</span>
            <span class="ok">
              <svg
                width="9"
                height="9"
                viewBox="0 0 24 24"
                fill="none"
                stroke="#fff"
                stroke-width="4"
                stroke-linecap="round"
                stroke-linejoin="round"
              >
                <polyline points="20 6 9 17 4 12" />
              </svg>
            </span>
          </div>
          <div class="row">
            <span class="dot" style="background: #8b5cf6"></span>
            <span class="ln" style="width: 56px"></span>
            <span class="tag" style="background: #8b5cf6">สมาชิก</span>
            <span class="ok">
              <svg
                width="9"
                height="9"
                viewBox="0 0 24 24"
                fill="none"
                stroke="#fff"
                stroke-width="4"
                stroke-linecap="round"
                stroke-linejoin="round"
              >
                <polyline points="20 6 9 17 4 12" />
              </svg>
            </span>
          </div>
        </div>

        <div class="head">
          <p class="title">{{ title }}</p>
          <p v-if="hint" class="hint">{{ hint }}</p>
        </div>

        <ul class="steps">
          <li
            v-for="(label, i) in steps"
            :key="label"
            :class="finished || i < currentStep ? 'done' : i === currentStep ? 'now' : ''"
          >
            <span class="ic">
              <svg
                v-if="finished || i < currentStep"
                width="10"
                height="10"
                viewBox="0 0 24 24"
                fill="none"
                stroke="#fff"
                stroke-width="4"
                stroke-linecap="round"
                stroke-linejoin="round"
              >
                <polyline points="20 6 9 17 4 12" />
              </svg>
            </span>
            {{ label }}
            <span v-if="!finished && i === currentStep && total" class="cnt">{{ percent }}%</span>
          </li>
        </ul>

        <div v-if="total" class="pct-row">
          <span>{{ done }} / {{ total }} {{ unitLabel }}</span>
          <span class="pct">{{ percent }}%</span>
        </div>
        <div v-if="total" class="bar"><i :style="{ width: percent + '%' }" /></div>

        <div class="done-box">
          <div class="big">
            <svg
              width="26"
              height="26"
              viewBox="0 0 24 24"
              fill="none"
              stroke="#fff"
              stroke-width="3.2"
              stroke-linecap="round"
              stroke-linejoin="round"
            >
              <polyline points="20 6 9 17 4 12" />
            </svg>
          </div>
          <p class="dt">{{ finishedTitle }}</p>
          <p class="dh">{{ finishedMessageText }}</p>
          <p class="dismiss">คลิกที่ใดก็ได้เพื่อปิด</p>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';

const props = withDefaults(
  defineProps<{
    visible: boolean;
    title: string;
    hint?: string;
    done: number;
    total: number;
    steps: string[];
    currentStep: number;
    finished?: boolean;
    unitLabel?: string;
    // Overridable so this overlay reads correctly when reused for actions
    // other than import (e.g. bulk delete) — default text still matches
    // the original import wording exactly.
    finishedTitle?: string;
    finishedMessage?: string;
  }>(),
  { unitLabel: 'รายการ', finishedTitle: 'นำเข้าข้อมูลเสร็จแล้ว' }
);

const emit = defineEmits<{ close: [] }>();

const finishedMessageText = computed(
  () => props.finishedMessage ?? `บันทึก ${props.total} รายการเรียบร้อย`
);

const percent = computed(() => (props.total ? Math.round((props.done / props.total) * 100) : 0));

const closing = ref(false);
function onOverlayClick() {
  if (!props.finished || closing.value) return;
  closing.value = true;
  setTimeout(() => {
    closing.value = false;
    emit('close');
  }, 200);
}
</script>

<style scoped>
.ov {
  position: fixed;
  inset: 0;
  z-index: 5000;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(8, 40, 50, 0.38);
  backdrop-filter: blur(6px) saturate(0.9);
  -webkit-backdrop-filter: blur(6px);
  animation: ovIn 0.25s ease-out both;
}
.ov.finished {
  cursor: pointer;
}
.ov.out {
  animation: ovOut 0.2s ease-in forwards;
}
@keyframes ovOut {
  to {
    opacity: 0;
  }
}

.head {
  text-align: center;
  margin-bottom: 18px;
}
.title {
  font-size: 17px;
  font-weight: 700;
  margin: 0 0 4px;
  color: #0b3b47;
}
.hint {
  font-size: 13px;
  color: #5b7780;
  margin: 0;
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

.a-card {
  width: 420px;
  max-width: calc(100vw - 32px);
  text-align: left;
  background: #fff;
  border-radius: 18px;
  padding: 28px 28px 24px;
  box-shadow: 0 24px 60px rgba(4, 40, 52, 0.28);
  animation: cardIn 0.3s 0.05s ease-out both;
}
.sheet {
  margin: 0 auto 20px;
  width: 220px;
  border-radius: 12px;
  background: #f2f8fa;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.ov.finished .sheet,
.ov.finished .head,
.ov.finished .pct-row,
.ov.finished .bar {
  display: none;
}
.ov.finished .steps {
  border-top: none;
  padding-top: 0;
}
.row {
  display: flex;
  align-items: center;
  gap: 8px;
  height: 22px;
  opacity: 0;
  animation: rowIn 2.4s ease-in-out infinite;
}
.row:nth-child(2) {
  animation-delay: 0.4s;
}
.row:nth-child(3) {
  animation-delay: 0.8s;
}
.row .dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  flex: none;
}
.row .ln {
  height: 8px;
  border-radius: 4px;
  background: #cfe3e8;
}
.row .tag {
  margin-left: auto;
  height: 16px;
  padding: 0 7px;
  border-radius: 999px;
  font-size: 10px;
  font-weight: 700;
  line-height: 16px;
  color: #fff;
}
.row .ok {
  width: 16px;
  height: 16px;
  flex: none;
  border-radius: 50%;
  background: #16a34a;
  display: flex;
  align-items: center;
  justify-content: center;
  transform: scale(0);
  animation: okPop 2.4s ease-in-out infinite;
}
.row:nth-child(2) .ok {
  animation-delay: 0.4s;
}
.row:nth-child(3) .ok {
  animation-delay: 0.8s;
}
@keyframes rowIn {
  0% {
    opacity: 0;
    transform: translateX(-14px);
  }
  12%,
  80% {
    opacity: 1;
    transform: none;
  }
  100% {
    opacity: 0;
    transform: translateX(10px);
  }
}
@keyframes okPop {
  0%,
  22% {
    transform: scale(0);
  }
  32% {
    transform: scale(1.15);
  }
  38%,
  80% {
    transform: scale(1);
  }
  100% {
    transform: scale(0);
  }
}

.steps {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin: 0;
  padding: 16px 0 0;
  border-top: 1px solid #edf2f4;
  list-style: none;
}
.steps li {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 13.5px;
  color: #9fb3b9;
}
.steps .ic {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  flex: none;
  border: 2px solid #d5e3e7;
  display: flex;
  align-items: center;
  justify-content: center;
  box-sizing: border-box;
}
.steps li.done {
  color: #0b3b47;
}
.steps li.done .ic {
  background: #16a34a;
  border-color: #16a34a;
  animation: okIn 0.3s ease-out both;
}
.steps li.now {
  color: #0e7490;
  font-weight: 600;
}
.steps li.now .ic {
  border-color: #0891b2;
  border-top-color: transparent;
  animation: spin 0.8s linear infinite;
}
.steps li .cnt {
  margin-left: auto;
  font-size: 12px;
  font-weight: 600;
  color: #5b7780;
  font-variant-numeric: tabular-nums;
}
@keyframes okIn {
  from {
    transform: scale(0.5);
  }
  to {
    transform: scale(1);
  }
}
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.bar {
  margin-top: 20px;
  height: 5px;
  border-radius: 999px;
  background: #e3eff2;
  overflow: hidden;
}
.bar i {
  display: block;
  height: 100%;
  border-radius: 999px;
  background: linear-gradient(90deg, #0e7490, #22b8cf);
  transition: width 0.3s ease-out;
}
.pct-row {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  margin-top: 18px;
  margin-bottom: 8px;
  font-size: 12px;
  color: #5b7780;
}
.pct {
  font-size: 15px;
  font-weight: 700;
  color: #0e7490;
  font-variant-numeric: tabular-nums;
}
.pct-row + .bar {
  margin-top: 0;
}

.done-box {
  display: none;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 6px;
  padding-top: 18px;
  margin-top: 18px;
  border-top: 1px solid #edf2f4;
  animation: cardIn 0.3s ease-out both;
}
.ov.finished .done-box {
  display: flex;
}
.done-box .big {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  background: #16a34a;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 8px 20px rgba(22, 163, 74, 0.3);
  animation: okPop2 0.45s cubic-bezier(0.3, 1.6, 0.5, 1) both;
}
.done-box .dt {
  font-size: 17px;
  font-weight: 700;
  color: #0b3b47;
  margin: 6px 0 0;
}
.done-box .dh {
  font-size: 12.5px;
  color: #5b7780;
  margin: 0;
}
.dismiss {
  margin-top: 14px;
  font-size: 12px;
  color: #5b7780;
  animation: blinkTxt 1.6s ease-in-out infinite;
}
@keyframes okPop2 {
  from {
    transform: scale(0);
  }
  to {
    transform: scale(1);
  }
}
@keyframes blinkTxt {
  0%,
  100% {
    opacity: 0.55;
  }
  50% {
    opacity: 1;
  }
}

@media (prefers-reduced-motion: reduce) {
  * {
    animation-duration: 0.01s !important;
    animation-iteration-count: 1 !important;
  }
}
</style>
