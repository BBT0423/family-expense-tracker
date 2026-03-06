<template>
  <div class="access-denied-container">
    <div class="icon">
      <svg width="80" height="80" viewBox="0 0 24 24" fill="none">
        <circle cx="12" cy="12" r="10" fill="#f8d7da" />
        <path d="M8 8l8 8M16 8l-8 8" stroke="#721c24" stroke-width="2" stroke-linecap="round" />
      </svg>
    </div>
    <h2>Access Denied</h2>
    <p>You do not have permission to view this page.</p>
    <router-link to="/" class="home-link"> Go to initial page </router-link>
  </div>
</template>

<script lang="ts" setup>
console.log('Access Denied View Loaded');
</script>

<style scoped>
.access-denied-container {
  min-height: 60vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: color(--bg);
  border-radius: var(--radius);
  padding: 40px 24px;
  margin: 40px auto;
  max-width: 400px;
}

.icon {
  margin-bottom: 20px;
}

h2 {
  margin-bottom: 12px;
  font-size: 2rem;
  font-weight: 600;
}

p {
  margin-bottom: 24px;
  text-align: center;
  color: var(--text-light);
}

.home-link {
  display: inline-block;
  padding: 10px 24px;
  border-radius: 6px;
  text-decoration: none;
  font-weight: 500;
  transition: background 0.2s;
}

.home-link:hover {
  background: var(--color-danger-dark);
  border: 1px solid var(--secondary);
}
</style>
