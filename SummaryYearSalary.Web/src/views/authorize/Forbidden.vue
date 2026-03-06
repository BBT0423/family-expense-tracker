<template>
  <div id="forbiddenContainer" class="auth-login-container">
    <div class="auth-logo">
      <h1 class="auth-welcome-text">Access Denied</h1>
      <p class="auth-subtitle">You don't have permission to access this resource</p>
    </div>

    <div class="forbidden-content">
      <div class="forbidden-icon">
        <svg
          width="80"
          height="80"
          viewBox="0 0 24 24"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zM12 20c-4.41 0-8-3.59-8-8 0-1.85.63-3.55 1.69-4.9L16.9 18.31C15.55 19.37 13.85 20 12 20zm6.31-3.1L7.1 5.69C8.45 4.63 10.15 4 12 4c4.41 0 8 3.59 8 8 0 1.85-.63 3.55-1.69 4.9z"
            fill="currentColor"
          />
        </svg>
      </div>

      <div class="forbidden-message">
        <h2>403 - Forbidden</h2>
        <p>
          You don't have the necessary permissions to view this page. Please contact your
          administrator if you believe this is an error.
        </p>
      </div>

      <div class="forbidden-actions">
        <button type="button" class="btn btn-secondary" @click="goBack">
          <svg
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.42-1.41L7.83 13H20v-2z"
              fill="currentColor"
            />
          </svg>
          Go Back
        </button>

        <button type="button" class="btn btn-primary" @click="goHome">
          <svg
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path d="M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z" fill="currentColor" />
          </svg>
          Go to Home
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';

// Get theme state from layout
const router = useRouter();

const goBack = () => {
  if (globalThis.history.length > 1) {
    router.go(-1);
  } else {
    router.push('/');
  }
};

const goHome = () => {
  router.push('/');
};
</script>

<style scoped>
.forbidden-content {
  text-align: center;
  padding: 2rem 0;
  max-width: 500px;
  margin: 0 auto;
}

.forbidden-icon {
  color: var(--color-error);
  margin-bottom: 1.5rem;
  display: flex;
  justify-content: center;
}

.forbidden-message {
  margin-bottom: 2rem;
}

.forbidden-message h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--color-error);
  margin-bottom: 0.75rem;
}

.forbidden-message p {
  color: var(--color-text-secondary);
  line-height: 1.6;
  font-size: 0.95rem;
}

.forbidden-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
}

/* Animation for entrance */
#forbiddenContainer {
  animation: fadeInUp 0.6s ease-out;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Responsive design */
@media (max-width: 640px) {
  .forbidden-actions {
    flex-direction: column;
    align-items: stretch;
  }

  .btn {
    width: 100%;
  }
}
</style>
