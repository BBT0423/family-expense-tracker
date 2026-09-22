import type { App } from 'vue';

/**
 * Element Plus renders the calendar header / year grid with Gregorian years and has no
 * Buddhist-era option. This shifts any 19xx/20xx year shown inside a picker panel by +543.
 * Idempotent: converted years (25xx) no longer match, so re-runs are no-ops.
 */
const YEAR_RE = /\b(19|20)\d{2}\b/g;
const TARGETS =
  '.el-date-picker__header-label, .el-year-table, .el-month-table, .el-date-range-picker__header';

function toBuddhist(text: string) {
  return (
    text
      .replace(YEAR_RE, (y) => String(Number(y) + 543))
      // EP appends its locale's "ปี" to every year label: "2569 ปี" -> "พ.ศ. 2569", "2563 ปี - 2572 ปี" -> "2563 - 2572"
      .replace(/^\s*(\d{4})\s*ปี\s*-\s*(\d{4})\s*ปี\s*$/, '$1 - $2')
      .replace(/^\s*(\d{4})\s*ปี\s*$/, 'พ.ศ. $1')
  );
}

function patchPanel(root: Element) {
  const nodes = root.matches(TARGETS) ? [root] : Array.from(root.querySelectorAll(TARGETS));
  for (const el of nodes) {
    const walker = document.createTreeWalker(el, NodeFilter.SHOW_TEXT);
    let n = walker.nextNode();
    while (n) {
      const next = toBuddhist(n.nodeValue ?? '');
      if (next !== n.nodeValue) n.nodeValue = next;
      n = walker.nextNode();
    }
  }
}

export default {
  install(_app: App) {
    let scheduled = false;
    const run = () => {
      scheduled = false;
      document.querySelectorAll('.el-picker-panel').forEach(patchPanel);
    };
    new MutationObserver(() => {
      if (scheduled) return;
      scheduled = true;
      requestAnimationFrame(run);
    }).observe(document.body, { childList: true, subtree: true, characterData: true });
  },
};
