<template>
  <el-tour
    :model-value="open"
    v-model:current="current"
    :z-index="2001"
    :mask="{ color: 'rgba(8, 40, 50, 0.45)' }"
    :scroll-into-view-options="{ block: 'center', behavior: 'instant' }"
    @update:model-value="onToggle"
    @change="goTo"
    @close="tour.finish()"
    @finish="tour.finish()"
  >
    <el-tour-step
      v-for="(step, i) in activeSteps"
      :key="i"
      :target="resolveTarget(step)"
      :title="step.title"
      :description="step.description"
      :placement="step.placement"
    />
  </el-tour>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { storeToRefs } from 'pinia';
import { useTourStore } from '@/stores/tourStore';
import { useLockScreenStore } from '@/stores/lockScreenStore';
import { tourSteps, type TourStepConfig } from '@/constants/tourSteps';

const router = useRouter();
const route = useRoute();
const tour = useTourStore();
const lockScreen = useLockScreenStore();
const { open } = storeToRefs(tour);

const current = ref(0);
// Bumped whenever a step's target may have just appeared (after navigation),
// so resolveTarget() re-runs and ElTour re-measures the element.
const tick = ref(0);
const compact = ref(false);
let navToken = 0;

const activeSteps = computed(() =>
  tourSteps.filter((s) => !(compact.value && s.desktopOnly))
);

const TALL_MARGIN_TOP = 90;
const isTall = (el: HTMLElement) =>
  el.getBoundingClientRect().height > globalThis.innerHeight - 160;

// A target taller than the viewport (a long list, or a form on a small
// window) would push the popover off-screen, so highlight only its visible
// part. ElTour keeps the previous step's position for target-less steps,
// so "no target" is not a usable fallback here.
function visiblePartOf(el: HTMLElement) {
  return () =>
    ({
      getBoundingClientRect: () => {
        const r = el.getBoundingClientRect();
        const top = Math.max(r.top, TALL_MARGIN_TOP);
        const bottom = Math.min(r.bottom, globalThis.innerHeight - 24);
        return DOMRect.fromRect({
          x: r.left,
          y: top,
          width: r.width,
          height: Math.max(bottom - top, 80),
        });
      },
      scrollIntoView: () => {},
    }) as unknown as HTMLElement;
}

function resolveTarget(step: TourStepConfig): HTMLElement | (() => HTMLElement) | undefined {
  void tick.value;
  if (!step.selector) return undefined;
  const el = document.querySelector<HTMLElement>(step.selector);
  if (!el) return undefined;
  return isTall(el) ? visiblePartOf(el) : el;
}

const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

function waitForElement(selector: string, timeoutMs: number): Promise<HTMLElement | null> {
  return new Promise((resolve) => {
    const startedAt = Date.now();
    const check = () => {
      const el = document.querySelector<HTMLElement>(selector);
      if (el && el.getBoundingClientRect().width > 0) return resolve(el);
      if (Date.now() - startedAt >= timeoutMs) return resolve(null);
      setTimeout(check, 100);
    };
    check();
  });
}

// ElTour advances `current` itself; this moves the app to the page the new
// step lives on and re-measures once its target has rendered. If the target
// never shows up the step simply stays centered, so the tour can't get stuck.
async function goTo(index: number) {
  const step = activeSteps.value[index];
  if (!step) return;
  const token = ++navToken;

  const needsNavigation = !!step.route && route.path !== step.route;
  if (needsNavigation) await router.push(step.route!);
  if (token !== navToken) return;

  if (step.selector) {
    const el = await waitForElement(step.selector, 5000);
    if (token !== navToken) return;
    // Let entrance animations / data loading settle before measuring.
    if (el && needsNavigation) await sleep(350);
    if (token !== navToken) return;
    // ElTour won't scroll for a tall target, so bring its top into view.
    if (el && isTall(el)) {
      el.scrollIntoView({ block: 'start' });
      globalThis.scrollBy(0, -TALL_MARGIN_TOP);
    }
  }
  await nextTick();
  tick.value++;
}

function onToggle(value: boolean) {
  if (!value) tour.finish();
}

watch(open, (isOpen) => {
  if (!isOpen) return;
  compact.value = globalThis.innerWidth <= 1024;
  current.value = 0;
  goTo(0);
});

// First run: show the tour once, after the dashboard has rendered.
onMounted(async () => {
  if (tour.completed || route.path !== '/pages/dashboard') return;
  const el = await waitForElement('[data-tour="dash-summary"]', 8000);
  if (!el) return;
  await sleep(900);
  if (!tour.completed && !tour.open && !lockScreen.isLocked) tour.start();
});
</script>
