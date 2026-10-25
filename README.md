# Family Expense Tracker

เว็บแอปบันทึกรายรับ-รายจ่ายของครอบครัว (โปรเจกต์ `SummaryYearSalary`) — backend .NET + frontend Vue 3

## ฟีเจอร์

- **Dashboard** — มี 2 แท็บ: *ภาพรวม* (สรุปรายเดือน กราฟรายรับ-รายจ่าย 12 เดือน สัดส่วนรายจ่ายตามหมวดหมู่) และ *รายละเอียด*
  เลือกเดือนได้ และเลือกขอบเขตข้อมูลที่ดู (ของตัวเอง / ผู้ใช้อื่น / ทั้งหมด)
- **รายการธุรกรรม** — ดู กรองตามหมวดหมู่ ค้นหาจากหมายเหตุ แก้ไข/ลบ ส่งออกเป็น Excel และ **เพิ่มรายการ** ใหม่
- **นำเข้าจาก Excel** — wizard 4 ขั้น: อัปโหลดไฟล์ → แมปคอลัมน์ → ตรวจสอบข้อมูล → นำเข้าสำเร็จ
- **ตั้งค่า** — จัดการหมวดหมู่ และผู้ใช้ในระบบ (ผู้ดูแลระบบ)
- **ข้อเสนอรายการ (Proposals)** — เสนอรายการให้ผู้ใช้อื่นยืนยัน พร้อมการแจ้งเตือน
- **ระบบบัญชี** — login ด้วยอีเมล หรือ Google, สมัครสมาชิก, ยืนยันตัวตนสองชั้น (OTP), เปลี่ยนรหัสผ่าน, โปรไฟล์

## โครงสร้างโปรเจกต์

| โฟลเดอร์ | หน้าที่ |
|---|---|
| `SummaryYearSalary.Api` | ASP.NET Core Web API (controllers, middleware, `Program.cs`) |
| `SummaryYearSalary.Application` | logic ของระบบ (MediatR commands/queries, FluentValidation) |
| `SummaryYearSalary.Domain` | entities และกฎพื้นฐาน |
| `SummaryYearSalary.Infrastructure` | EF Core + SQL Server, repositories, Identity/JWT, อีเมล, export Excel, migrations |
| `SummaryYearSalary.Tests` | unit tests (xUnit + Moq) |
| `SummaryYearSalary.Web` | frontend Vue 3 + TypeScript + Vite |

**Stack** — Backend: .NET 10, EF Core + SQL Server, ASP.NET Identity + JWT, MediatR, Mapster, Serilog, ClosedXML ·
Frontend: Vue 3, Pinia, Vue Router, Element Plus, ECharts, Vite

## สิ่งที่ต้องมี

- .NET SDK 10.0 ขึ้นไป
- Node.js 20 ขึ้นไป
- SQL Server (หรือ SQL Server LocalDB)

## เริ่มใช้งาน (development)

### 1. ติดตั้ง dependency

```bash
dotnet restore
cd SummaryYearSalary.Web
npm ci
```

### 2. รัน backend

connection string ใน `SummaryYearSalary.Api/appsettings.json` ชี้ไปที่เครื่องของผู้พัฒนาเดิม (`Server=BBT0423`) ให้เขียนทับด้วย
environment variable ของเครื่องตัวเอง และตั้ง secret ของ JWT:

```powershell
$env:ConnectionStrings__SummaryYearSalaryConnection = "Server=(localdb)\MSSQLLocalDB;Database=SummaryYearSalaryDb;Trusted_Connection=True;TrustServerCertificate=True"
$env:JwtSettings__Secret = "<ข้อความสุ่มยาวอย่างน้อย 32 ตัวอักษร>"
dotnet run --project SummaryYearSalary.Api
```

- แอปสร้าง database และตารางเองด้วย migration ตอนเริ่มทำงาน (พร้อมหมวดหมู่ตั้งต้น, role และบัญชีผู้ดูแล)
- API ทำงานที่ `http://localhost:5001` (ตาม `launchSettings.json`)
- คำสั่ง migration อยู่ใน [Command.md](Command.md)

### 3. รัน frontend

```bash
cd SummaryYearSalary.Web
npx vite
```

เปิด `http://localhost:5173` — Vite proxy ส่ง `/api` ไปที่ backend (ปรับปลายทางด้วย `VITE_API_BASE_URL`)

> `npm run dev` จะรัน `eslint --fix` และ `prettier --write` ทั้ง `src` ก่อนเสมอ (`predev`) ซึ่งแก้ไฟล์ทั้งโปรเจกต์
> ถ้าแค่ต้องการเปิดดู ให้ใช้ `npx vite` ตามด้านบน

## ตัวแปรสภาพแวดล้อม

secret ทั้งหมดใน `appsettings.json` เป็น placeholder `{INNO_...}` ที่ถูกแทนด้วยค่าจาก environment variable

| ตัวแปร | ใช้ทำอะไร |
|---|---|
| `ConnectionStrings__SummaryYearSalaryConnection` | connection string ของ SQL Server (เขียนทับค่าใน `appsettings.json`) |
| `JwtSettings__Secret` | secret สำหรับเซ็น/ตรวจ JWT (≥ 32 ตัวอักษร) |
| `INNO_SMTP_PASSWORD` | รหัสผ่าน SMTP สำหรับส่งอีเมล (ลืมรหัสผ่าน, OTP) |
| `INNO_GOOGLE_CLIENT_ID` / `INNO_GOOGLE_CLIENT_SECRET` | login ด้วย Google |

> ตั้ง `JwtSettings__Secret` ตรง ๆ แทน `INNO_JWT_SECRET`: ตอนสร้าง token โค้ดแทนที่ placeholder `{INNO_JWT_SECRET}` ให้ แต่ฝั่ง
> ตรวจ token อ่านค่าจาก config โดยไม่แทนที่ ถ้าตั้งแค่ `INNO_JWT_SECRET` ทุก request ที่ต้อง login จะได้ 401

## ทดสอบและตรวจคุณภาพ

```bash
# backend
dotnet build
dotnet test

# frontend (ในโฟลเดอร์ SummaryYearSalary.Web)
npx vue-tsc --noEmit
npm run lint:check
npm run format:check
```

## CI/CD (Bitbucket Pipelines)

ไฟล์ `bitbucket-pipelines.yml` มาจากเทมเพลตและ **ยังไม่ได้ปรับให้ตรงกับโปรเจกต์นี้** — ใช้เป็นแนวทางได้ แต่ต้องแก้ก่อนเปิดใช้งานจริง:

- ขั้นตอนที่มี: ตรวจชื่อ branch, ตรวจชื่อ Pull Request, lint + test, test พร้อม coverage, วิเคราะห์ด้วย SonarQube, build frontend/backend
  (ไม่มีขั้นตอน deploy และไม่ได้ใช้ตัวแปร Azure ใด ๆ)
- สิ่งที่ยังเป็นค่าเดิมของเทมเพลต:
  - ขั้นตอน Sonar อ้างโฟลเดอร์ `Stackapp.Web` และ `Stackapp.*` ซึ่งไม่มีใน repo นี้ (ของจริงคือ `SummaryYearSalary.*`)
  - `{{SONAR_PROJECT_KEY}}` / `{{SONAR_PROJECT_NAME}}` ยังไม่ถูกแทนที่ และชี้ไปที่เซิร์ฟเวอร์ Sonar `https://sonar.innofinite.dev` ต้องใส่ `SONAR_TOKEN` ใน Repository Settings → Pipelines → Environment variables
- กฎที่ pipeline ตรวจอยู่:
  - **ชื่อ branch:** `feature|bugfix|hotfix|release/KEY-###-description` (ตัวพิมพ์เล็ก ตัวเลข และ `-`)
  - **ชื่อ Pull Request:** `[KEY-###] - Description`
