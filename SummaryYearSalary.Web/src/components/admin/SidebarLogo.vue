<template>
  <div class="sidebar-brand">
    <div class="sidebar-logo-mark">
      <el-icon size="16"><Wallet /></el-icon>
    </div>
    <div class="sidebar-brand-text">
      <div class="sidebar-brand-name">Family Expense</div>
      <div class="sidebar-brand-sub">expense tracker</div>
    </div>
  </div>
</template>

<style scoped>
.sidebar-brand {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}

.sidebar-logo-mark {
  width: 30px;
  height: 30px;
  flex-shrink: 0;
  border-radius: 8px;
  background: var(--primary);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
}

.sidebar-brand-name {
  font-size: 14px;
  font-weight: 700;
  line-height: 1.2;
  color: var(--text);
  white-space: nowrap;
}

.sidebar-brand-sub {
  font-family: var(--font-mono);
  font-size: 10.5px;
  line-height: 1.2;
  color: var(--text-muted);
  white-space: nowrap;
}
</style>
