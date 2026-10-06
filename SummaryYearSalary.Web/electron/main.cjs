const { app, BrowserWindow, dialog, shell } = require('electron');
const { spawn, execFile } = require('child_process');
const path = require('path');
const http = require('http');
const net = require('net');
const fs = require('fs');

// package.json's "name" is "SummaryYearSalary-web" (the npm package name),
// but electron-builder's productName — and therefore the NSIS uninstaller's
// ${APP_FILENAME}, used to locate $APPDATA\<name> for the data-delete choice
// — is "SummaryYearSalary". Force Electron's app name to match, otherwise
// app.getPath('userData') and the uninstaller disagree on where the DB lives.
app.setName('SummaryYearSalary');

const API_URL = 'http://127.0.0.1:5231';
const API_PORT = 5231;

// Packaged builds ship the published API under resources/api (see the
// electron-builder "extraResources" config in package.json); dev mode reads
// it straight out of the sibling .NET project's publish/ output instead.
const API_EXE_PATH = app.isPackaged
  ? path.join(process.resourcesPath, 'api', 'SummaryYearSalary.Api.exe')
  : path.join(
      __dirname,
      '..',
      '..',
      'SummaryYearSalary.Api',
      'publish',
      'SummaryYearSalary.Api.exe'
    );

let apiProcess;
let logStream;

function getLogPath() {
  return path.join(app.getPath('userData'), 'electron-api.log');
}

// A packaged (NSIS) build has no console — anything we only console.log
// disappears the moment the process exits, which made every past "backend
// won't start" report unactionable (no way to know WHY it failed). Persist
// the same lines to a file in userData so a real diagnosis is possible.
function initLogger() {
  logStream = fs.createWriteStream(getLogPath(), { flags: 'a' });
  log(`===== App start (packaged=${app.isPackaged}, exe=${API_EXE_PATH}) =====`);
}

function log(line) {
  const stamped = `[${new Date().toISOString()}] ${line}`;
  console.log(stamped);
  if (logStream) logStream.write(stamped + '\n');
}

function isPortInUse(port) {
  return new Promise((resolve) => {
    const socket = net.createConnection({ port, host: '127.0.0.1' });
    socket.once('connect', () => {
      socket.destroy();
      resolve(true);
    });
    socket.once('error', () => resolve(false));
    socket.setTimeout(500, () => {
      socket.destroy();
      resolve(false);
    });
  });
}

// If a previous run's backend got orphaned (e.g. the main process was force-
// killed before its before-quit handler ran), it can still be holding
// API_PORT and silently block the new spawn from ever binding — the visible
// symptom is just an indefinite health-check timeout with zero explanation.
// Best-effort clear it before starting our own copy.
function killOrphanedApiProcess() {
  return new Promise((resolve) => {
    if (process.platform !== 'win32') return resolve();
    execFile('taskkill', ['/F', '/IM', 'SummaryYearSalary.Api.exe'], () => resolve());
  });
}

async function startApi() {
  // The DB must live outside the install directory (userData survives both
  // uninstall and version upgrades, unlike anything next to the exe) — see
  // INNO_DB_CONNECTION handling in InfrastructureServiceRegistration.cs.
  const dbPath = path.join(app.getPath('userData'), 'family-expense.db');

  if (await isPortInUse(API_PORT)) {
    log(`Port ${API_PORT} already in use before spawn — clearing any orphaned API process.`);
    await killOrphanedApiProcess();
    await new Promise((resolve) => setTimeout(resolve, 500));
  }

  log(`Spawning API at ${API_EXE_PATH}`);
  apiProcess = spawn(API_EXE_PATH, [], {
    cwd: path.dirname(API_EXE_PATH),
    env: { ...process.env, ASPNETCORE_URLS: API_URL, INNO_DB_CONNECTION: dbPath },
  });

  // Whatever the .NET process prints (Serilog console logs, errors, etc.)
  // now goes to the persistent log file too, not just this terminal.
  apiProcess.stdout.on('data', (data) => log(`[api] ${data.toString().trimEnd()}`));
  apiProcess.stderr.on('data', (data) => log(`[api:err] ${data.toString().trimEnd()}`));
  apiProcess.on('error', (err) => log(`[api] failed to spawn: ${err.message}`));
  apiProcess.on('exit', (code, signal) => log(`[api] process exited (code=${code}, signal=${signal})`));
}

// Poll /health every 500ms until the API answers, then resolve; reject after
// ~30s so a broken packaged path fails loudly instead of hanging on splash.
function waitForApi(timeoutMs = 30000) {
  return new Promise((resolve, reject) => {
    const deadline = Date.now() + timeoutMs;
    const check = () => {
      http
        .get(`${API_URL}/health`, (res) => {
          if (res.statusCode === 200) resolve();
          else if (Date.now() > deadline) reject(new Error('API health check timed out'));
          else setTimeout(check, 500);
        })
        .on('error', () => {
          if (Date.now() > deadline) reject(new Error('API health check timed out'));
          else setTimeout(check, 500);
        });
    };
    check();
  });
}

async function createWindow() {
  const win = new BrowserWindow({
    width: 1280,
    height: 800,
    show: false,
    icon: path.join(__dirname, 'icon.ico'),
  });

  // Show a splash screen immediately instead of a blank white window while
  // the .NET API boots (DB connect, migrations) — then swap to the real app.
  win.loadFile(path.join(__dirname, 'splash.html'));
  win.once('ready-to-show', () => win.show());

  // The renderer sets window.onbeforeunload while a save/import/transaction
  // write is in flight (see App.vue + httpClient's pendingWrites). Electron
  // honors that by default (blocking the close silently) and tells us via
  // this event instead of just closing — ask the user explicitly rather
  // than leaving them stuck with an unexplained refusal to quit, or letting
  // a save get killed mid-request with no warning.
  win.webContents.on('will-prevent-unload', (event) => {
    const choice = dialog.showMessageBoxSync(win, {
      type: 'warning',
      title: 'ยังบันทึกข้อมูลไม่เสร็จ',
      message: 'มีการบันทึกหรือนำเข้าข้อมูลที่ยังทำงานอยู่ ถ้าปิดตอนนี้ข้อมูลอาจไม่ครบถ้วน',
      detail: 'แนะนำให้รอจนกว่าจะเสร็จก่อนปิดโปรแกรม',
      buttons: ['ยกเลิก', 'ปิดโปรแกรมเลย'],
      defaultId: 0,
      cancelId: 0,
    });
    // Electron blocks the close by default when the page's beforeunload
    // handler asks to — calling preventDefault() here means "ignore that
    // and close anyway", which is what we want only if the user confirmed.
    if (choice === 1) event.preventDefault();
  });

  await startApi();
  log('Waiting for API to become healthy...');
  try {
    await waitForApi();
    log('API is up, loading window.');
    win.loadURL(API_URL);
  } catch (err) {
    log(`API failed to become healthy: ${err.message}`);
    const logPath = getLogPath();
    const choice = dialog.showMessageBoxSync({
      type: 'error',
      title: 'SummaryYearSalary ไม่สามารถเริ่มทำงานได้',
      message: 'ไม่สามารถเชื่อมต่อกับระบบหลังบ้านได้ กรุณาลองเปิดโปรแกรมใหม่อีกครั้ง',
      detail: `ถ้ายังพบปัญหานี้ซ้ำ กรุณาส่งไฟล์ log นี้ให้ผู้พัฒนา:\n${logPath}`,
      buttons: ['เปิดโฟลเดอร์ log', 'ปิด'],
      defaultId: 1,
      cancelId: 1,
    });
    if (choice === 0) shell.showItemInFolder(logPath);
    app.quit();
  }
}

// Backend is bound to a single fixed port (5231) — a second running instance
// would collide with the first's spawn instead of reusing it, producing the
// exact same "cannot connect to backend" dialog with no obvious cause.
const gotLock = app.requestSingleInstanceLock();
if (!gotLock) {
  app.quit();
} else {
  app.on('second-instance', () => {
    const win = BrowserWindow.getAllWindows()[0];
    if (win) {
      if (win.isMinimized()) win.restore();
      win.focus();
    }
  });

  app.whenReady().then(() => {
    initLogger();
    createWindow();
  });

  app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') app.quit();
  });

  // Make sure the .NET process doesn't keep running after we close the window.
  app.on('before-quit', () => {
    if (apiProcess) apiProcess.kill();
    if (logStream) logStream.end();
  });
}
