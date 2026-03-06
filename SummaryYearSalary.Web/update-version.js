import { readFileSync, writeFileSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const packagePath = join(__dirname, 'package.json');
const packageJson = JSON.parse(readFileSync(packagePath, 'utf8'));

const date = new Date();
const day = String(date.getDate()).padStart(2, '0');
const month = String(date.getMonth() + 1).padStart(2, '0');
const year = date.getFullYear();
const dateStr = `${day}${month}${year}`;

// Keep the base version (1.0.0) and update only the date suffix
const baseVersion = packageJson.version.split('-')[0] || '1.0.0';
packageJson.version = `${baseVersion}-${dateStr}`;

writeFileSync(packagePath, JSON.stringify(packageJson, null, 2) + '\n');
console.log(`✓ Version updated to: ${packageJson.version}`);
