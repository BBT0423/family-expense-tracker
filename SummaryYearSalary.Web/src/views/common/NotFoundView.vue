<template>
  <div class="not-found-container">
    <div class="not-found-content">
      <h1>404</h1>
      <h2>Page Not Found</h2>
      <p>Sorry, the page you are looking for does not exist or has been moved.</p>
      <router-link to="/" class="home-link"> Go to Homepage </router-link>
    </div>
  </div>
</template>

<script lang="ts" setup>
console.log('Not Found');
</script>

<style scoped>
.not-found-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f7f8fa;
}

.not-found-content {
  text-align: center;
  background: #fff;
  padding: 48px 32px;
  border-radius: 12px;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.07);
}

.not-found-content h1 {
  font-size: 6rem;
  font-weight: 700;
  color: #1976d2;
  margin-bottom: 0.5rem;
}

.not-found-content h2 {
  font-size: 2rem;
  margin-bottom: 1rem;
  color: #333;
}

.not-found-content p {
  color: #666;
  margin-bottom: 2rem;
}

.home-link {
  display: inline-block;
  padding: 0.75em 2em;
  background: #1976d2;
  color: #fff;
  border-radius: 6px;
  text-decoration: none;
  font-weight: 500;
  transition: background 0.2s;
}

.home-link:hover {
  background: #125ea2;
}
</style>
