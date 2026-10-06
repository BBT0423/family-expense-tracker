const { app, BrowserWindow, dialog } = require('electron');
const { spawn } = require('child_process');
const path = require('path');
const http = require('http');

// package.json's "name" is "SummaryYearSalary-web" (the npm package name),
// but electron-builder's productName — and therefore the NSIS uninstaller's
// ${APP_FILENAME}, used to locate $APPDATA\<name> for the data-delete choice
// — is "SummaryYearSalary". Force Electron's app name to match, otherwise
// app.getPath('userData') and the uninstaller disagree on where the DB lives.
app.setName('SummaryYearSalary');

const API_URL = 'http://127.0.0.1:5231';

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

function startApi() {
  // The DB must live outside the install directory (userData survives both
  // uninstall and version upgrades, unlike anything next to the exe) — see
  // INNO_DB_CONNECTION handling in InfrastructureServiceRegistration.cs.
  const dbPath = path.join(app.getPath('userData'), 'family-expense.db');

  apiProcess = spawn(API_EXE_PATH, [], {
    cwd: path.dirname(API_EXE_PATH),
    env: { ...process.env, ASPNETCORE_URLS: API_URL, INNO_DB_CONNECTION: dbPath },
  });

  // Whatever the .NET process prints (Serilog console logs, errors, etc.)
  // shows up in THIS terminal — same as if you ran the exe yourself.
  apiProcess.stdout.on('data', (data) => console.log(`[api] ${data}`));
  apiProcess.stderr.on('data', (data) => console.error(`[api] ${data}`));
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
    icon: path.join(__dirname, 'icon.png'),
  });

  // Show a splash screen immediately instead of a blank white window while
  // the .NET API boots (DB connect, migrations) — then swap to the real app.
  win.loadFile(path.join(__dirname, 'splash.html'));
  win.once('ready-to-show', () => win.show());

  startApi();
  console.log('Waiting for API to become healthy...');
  try {
    await waitForApi();
    console.log('API is up, loading window.');
    win.loadURL(API_URL);
  } catch (err) {
    console.error(err);
    dialog.showErrorBox(
      'SummaryYearSalary ไม่สามารถเริ่มทำงานได้',
      'ไม่สามารถเชื่อมต่อกับระบบหลังบ้านได้ กรุณาลองเปิดโปรแกรมใหม่อีกครั้ง'
    );
    app.quit();
  }
}

app.whenReady().then(createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});

// Make sure the .NET process doesn't keep running after we close the window.
app.on('before-quit', () => {
  if (apiProcess) apiProcess.kill();
});
