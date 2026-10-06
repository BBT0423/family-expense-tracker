import { ref, watch, onUnmounted, type Ref } from 'vue';

const DURATION = 650;
// Backstop in case rAF never fires (backgrounded/throttled tab) — snaps to
// the real target shortly after the animation should have finished.
const FALLBACK_DELAY = DURATION + 150;

function easeOutCubic(t: number) {
  return 1 - Math.pow(1 - t, 3);
}

// Tweens a number from its current displayed value to a new source value
// whenever the source changes, instead of jumping instantly — used for the
// dashboard's headline figures (balance/income/expense) so switching month
// or member reads as a transition rather than a flicker.
export function useAnimatedNumber(source: Ref<number>, duration = DURATION) {
  const display = ref(source.value);
  let raf = 0;
  let fallback: ReturnType<typeof setTimeout> | undefined;

  function animate(to: number) {
    cancelAnimationFrame(raf);
    clearTimeout(fallback);

    if (typeof document !== 'undefined' && document.hidden) {
      display.value = to;
      return;
    }

    const from = display.value;
    const start = performance.now();
    const tick = (now: number) => {
      const t = Math.min(1, (now - start) / duration);
      display.value = from + (to - from) * easeOutCubic(t);
      if (t < 1) {
        raf = requestAnimationFrame(tick);
      }
    };
    raf = requestAnimationFrame(tick);
    fallback = setTimeout(() => {
      display.value = to;
    }, FALLBACK_DELAY);
  }

  watch(source, (to) => animate(to));
  animate(source.value);

  onUnmounted(() => {
    cancelAnimationFrame(raf);
    clearTimeout(fallback);
  });

  return display;
}
