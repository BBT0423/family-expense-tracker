<template>
  <div class="welcome-container">
    <div class="background-decoration">
      <div class="circle circle-1"></div>
      <div class="circle circle-2"></div>
      <div class="circle circle-3"></div>
    </div>

    <div class="welcome-content">
      <div :class="['logo-container', { 'animate-in': moved }]">
        <div class="logo-icon">
          <svg viewBox="0 0 100 100" class="logo-svg">
            <circle cx="50" cy="50" r="45" stroke="url(#gradient)" stroke-width="3" fill="none" />
            <path
              d="M30 35 L70 35 M30 50 L70 50 M30 65 L70 65"
              stroke="url(#gradient)"
              stroke-width="2.5"
              stroke-linecap="round"
            />
            <defs>
              <linearGradient id="gradient" x1="0%" y1="0%" x2="100%" y2="100%">
                <stop offset="0%" style="stop-color: #667eea; stop-opacity: 1" />
                <stop offset="100%" style="stop-color: #764ba2; stop-opacity: 1" />
              </linearGradient>
            </defs>
          </svg>
        </div>
        <h1 class="app-title">SummaryYearSalary</h1>
        <p class="app-subtitle">{{ getGreeting() }}!, Welcome to SummaryYearSalary</p>
      </div>

      <transition name="fade-slide">
        <div v-if="moved" class="welcome-info">
          <div class="loading-container">
            <div class="loading-spinner"></div>
            <p class="loading-text">Loading SummaryYearSalary...</p>
          </div>
        </div>
      </transition>
    </div>

    <div class="footer-info">
      <p>
        &copy; {{ new Date().getFullYear() }} SummaryYearSalary Corporation. All rights reserved.
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { getGreeting } from '@/utils/utilities';
import { ref, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const moved = ref(false);

// Store timeout IDs for cleanup
let animationTimeout: NodeJS.Timeout | null = null;
let redirectTimeout: NodeJS.Timeout | null = null;

onMounted(async () => {
  try {
    // Trigger animations after component mount
    animationTimeout = setTimeout(() => {
      moved.value = true;
    }, 500);

    redirectTimeout = setTimeout(() => {
      router.replace('/pages');
    }, 1500);
  } catch (error) {
    console.error('❌ Error initializing welcome page:', error);
    redirectTimeout = setTimeout(() => {
      router.replace('/pages');
    }, 1500);
  }
});

// Cleanup timeouts when component is unmounted
onUnmounted(() => {
  if (animationTimeout) {
    clearTimeout(animationTimeout);
  }
  if (redirectTimeout) {
    clearTimeout(redirectTimeout);
  }
});
</script>

<style scoped>
.welcome-container {
  position: fixed;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  width: 100vw;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  overflow: hidden;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

@keyframes float {
  0%,
  100% {
    transform: translateY(0px) scale(1);
    opacity: 0.7;
  }

  50% {
    transform: translateY(-20px) scale(1.05);
    opacity: 0.9;
  }
}

/* Main content */
.welcome-content {
  position: relative;
  z-index: 2;
  text-align: center;
  color: white;
  max-width: 700px;
  padding: 1.5rem;
  height: 100vh;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.logo-container {
  margin-bottom: 2rem;
  transform: translateY(30px);
  opacity: 0;
  transition: all 1s cubic-bezier(0.4, 0, 0.2, 1);
}

.logo-container.animate-in {
  transform: translateY(0);
  opacity: 1;
}

.logo-icon {
  width: 100px;
  height: 100px;
  margin: 0 auto 1rem;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(10px);
  border: 2px solid rgba(255, 255, 255, 0.2);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s ease;
}

.logo-icon:hover {
  transform: scale(1.05);
}

.logo-svg {
  width: 50px;
  height: 50px;
}

.app-title {
  font-size: 2.5rem;
  font-weight: 700;
  margin: 0 0 0.5rem 0;
  letter-spacing: 0.1em;
  text-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
  background: linear-gradient(45deg, #ffffff, #f0f8ff);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.app-subtitle {
  font-size: 1rem;
  font-weight: 300;
  margin: 0;
  opacity: 0.9;
  letter-spacing: 0.05em;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
}

/* Welcome info section */
.welcome-info {
  margin-top: 1.5rem;
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.loading-container {
  margin-bottom: 1.5rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.8rem;
}

.loading-spinner {
  width: 32px;
  height: 32px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top: 2px solid #ffffff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
}

.loading-text {
  font-size: 0.9rem;
  font-weight: 400;
  margin: 0;
  opacity: 0.8;
  animation: pulse 2s ease-in-out infinite;
}

@keyframes pulse {
  0%,
  100% {
    opacity: 0.8;
  }

  50% {
    opacity: 1;
  }
}

/* Features preview */
.features-preview {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  margin-top: 1rem;
  max-width: 600px;
  width: 100%;
  margin-left: auto;
  margin-right: auto;
}

.feature-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.3rem;
  padding: 0.8rem 0.5rem;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  transition: all 0.3s ease;
  min-height: 85px;
  cursor: pointer;
}

.feature-item:hover {
  transform: translateY(-4px) scale(1.02);
  background: rgba(255, 255, 255, 0.18);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
}

.feature-icon {
  font-size: 1.8rem;
  margin-bottom: 0.2rem;
  filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.1));
}

.feature-item span {
  font-size: 0.75rem;
  font-weight: 500;
  text-align: center;
  opacity: 0.95;
  line-height: 1.2;
  letter-spacing: 0.02em;
}

/* Footer */
.footer-info {
  position: absolute;
  bottom: 1.5rem;
  left: 50%;
  transform: translateX(-50%);
  z-index: 2;
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.8rem;
  text-align: center;
}

/* Transitions */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: all 0.8s cubic-bezier(0.4, 0, 0.2, 1);
}

.fade-slide-enter-from {
  opacity: 0;
  transform: translateY(30px);
}

.fade-slide-enter-to {
  opacity: 1;
  transform: translateY(0);
}

.fade-slide-leave-from {
  opacity: 1;
  transform: translateY(0);
}

.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-30px);
}

/* Responsive design - optimized for single page view */
@media (max-width: 768px) {
  .welcome-content {
    padding: 1rem;
    max-width: 500px;
  }

  .app-title {
    font-size: 2rem;
  }

  .app-subtitle {
    font-size: 0.9rem;
  }

  .logo-container {
    margin-bottom: 1.5rem;
  }

  .logo-icon {
    width: 80px;
    height: 80px;
  }

  .logo-svg {
    width: 40px;
    height: 40px;
  }

  .features-preview {
    grid-template-columns: repeat(2, 1fr);
    gap: 0.8rem;
    max-width: 400px;
  }

  .feature-item {
    min-height: 75px;
    padding: 0.7rem 0.4rem;
  }

  .feature-icon {
    font-size: 1.5rem;
  }

  .feature-item span {
    font-size: 0.7rem;
  }

  .loading-container {
    margin-bottom: 1rem;
  }

  .loading-spinner {
    width: 28px;
    height: 28px;
  }

  .loading-text {
    font-size: 0.8rem;
  }

  .circle-1,
  .circle-2,
  .circle-3 {
    opacity: 0.3;
    width: 80px !important;
    height: 80px !important;
  }
}

@media (max-width: 480px) {
  .welcome-content {
    padding: 0.8rem;
    max-width: 350px;
  }

  .app-title {
    font-size: 1.7rem;
  }

  .app-subtitle {
    font-size: 0.8rem;
  }

  .logo-container {
    margin-bottom: 1rem;
  }

  .logo-icon {
    width: 70px;
    height: 70px;
  }

  .logo-svg {
    width: 35px;
    height: 35px;
  }

  .features-preview {
    grid-template-columns: repeat(2, 1fr);
    gap: 0.6rem;
    max-width: 300px;
  }

  .feature-item {
    padding: 0.6rem 0.3rem;
    min-height: 65px;
  }

  .feature-icon {
    font-size: 1.3rem;
  }

  .feature-item span {
    font-size: 0.65rem;
    line-height: 1.1;
  }

  .loading-container {
    margin-bottom: 0.8rem;
  }

  .footer-info {
    bottom: 1rem;
    font-size: 0.7rem;
  }
}

@media (max-height: 700px) {
  .welcome-content {
    padding: 1rem;
  }

  .logo-container {
    margin-bottom: 1rem;
  }

  .welcome-info {
    margin-top: 1rem;
  }

  .loading-container {
    margin-bottom: 1rem;
  }

  .features-preview {
    margin-top: 0.5rem;
  }

  .feature-item {
    min-height: 70px;
    padding: 0.6rem 0.4rem;
  }

  .footer-info {
    bottom: 0.8rem;
  }
}
</style>
