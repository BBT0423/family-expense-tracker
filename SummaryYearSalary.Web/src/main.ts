// import css
import '@fontsource/plus-jakarta-sans/400.css';
import '@fontsource/plus-jakarta-sans/500.css';
import '@fontsource/plus-jakarta-sans/600.css';
import '@fontsource/plus-jakarta-sans/700.css';
import '@fontsource/ibm-plex-mono/400.css';
import '@fontsource/ibm-plex-mono/600.css';
import 'element-plus/dist/index.css';
import './theme/layout.css';
import './theme/shell.css';
import './theme/utilities.css';

import { createApp } from 'vue';
import { createPinia } from 'pinia';
import ElementPlus from 'element-plus';
import th from 'element-plus/es/locale/lang/th';
import dayjs from 'dayjs';
import buddhistEra from 'dayjs/plugin/buddhistEra';
import 'dayjs/locale/th';

// Configure Day.js for Thai Buddhist Era
dayjs.extend(buddhistEra);
dayjs.locale('th');

import * as ElementPlusIconsVue from '@element-plus/icons-vue';
import App from './App.vue';
import router from './router';
import versionCheck from './plugins/version-check';
import buddhistDatepicker from './plugins/buddhist-datepicker';
import { head } from './plugins/head';

const app = createApp(App);

for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component);
}

app.use(createPinia());
app.use(router);
app.use(head);
app.use(ElementPlus, { locale: th });
app.use(buddhistDatepicker);
app.use(versionCheck, { interval: 1 * 60 * 1000 }); // Check every 1 minute

app.mount('#app');
