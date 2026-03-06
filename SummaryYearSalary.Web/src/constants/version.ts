// This file exposes the app version from package.json for use in the app
// Vite will replace import.meta.env.PACKAGE_VERSION with the version at build time
export const APP_VERSION = import.meta.env.PACKAGE_VERSION || '1.0.0-250125';
