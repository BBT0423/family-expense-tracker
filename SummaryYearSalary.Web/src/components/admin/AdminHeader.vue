<template>
  <header class="header">
    <div class="header-left">
      <button type="button" class="menu-btn" @click="$emit('toggle-sidebar')">☰</button>
      <h2>SummaryYearSalary</h2>
    </div>
    <div class="header-right">
      <UserMenu />
    </div>
  </header>
  <NetworkStatusBanner />
</template>
