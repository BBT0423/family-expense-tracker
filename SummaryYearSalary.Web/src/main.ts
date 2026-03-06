// import css
import './theme/layout.css';
import './theme/utilities.css';

import { createApp } from 'vue';
import { createPinia } from 'pinia';

import App from './App.vue';
import router from './router';
import versionCheck from './plugins/version-check';
import { head } from './plugins/head';

const app = createApp(App);

app.use(createPinia());
app.use(router);
app.use(head);
app.use(versionCheck, { interval: 1 * 60 * 1000 }); // Check every 1 minute

app.mount('#app');
