!include "nsDialogs.nsh"

; Runs before MUI_UNPAGE_INSTFILES, i.e. before any files are actually removed.
!macro customUnWelcomePage
  !insertmacro MUI_UNPAGE_WELCOME
  UninstPage custom un.DataChoicePageCreate un.DataChoicePageLeave
!macroend

; electron-builder !includes this whole file unconditionally into BOTH the
; main-installer build pass and the separate BUILD_UNINSTALLER pass (which is
; the only one that ever calls WriteUninstaller). Un-prefixed Function
; definitions compiled into the installer pass trip NSIS warning 6020
; ("uninstaller script code found but WriteUninstaller never used"), which
; electron-builder treats as a hard error — so these must only exist when
; actually building the uninstaller, matching electron-builder's own
; uninstaller.nsh, which is itself !ifdef BUILD_UNINSTALLER-guarded.
!ifdef BUILD_UNINSTALLER

Var DataChoiceCheckbox
Var DataChoiceState

Function un.DataChoicePageCreate
  ; Not using MUI_HEADER_TEXT here: electron-builder !includes this file into
  ; its "header" script, which runs before installer.nsi's own
  ; !include "MUI2.nsh" — so that macro isn't defined yet at this point.
  ; A plain bold label serves as the page title instead.
  nsDialogs::Create 1018
  Pop $0

  ${NSD_CreateLabel} 0 0 100% 12u "เลือกว่าจะเก็บหรือลบข้อมูลที่บันทึกไว้"
  Pop $1
  CreateFont $2 "$(^Font)" "10" "700"
  SendMessage $1 0x0030 $2 0 ; WM_SETFONT

  ${NSD_CreateLabel} 0 20u 100% 40u "ข้อมูลรายรับ-รายจ่ายของคุณถูกเก็บแยกจากตัวโปรแกรม ค่าเริ่มต้นคือเก็บข้อมูลไว้ เพื่อให้ติดตั้งใหม่ภายหลังได้โดยไม่เสียข้อมูล"
  Pop $0

  ${NSD_CreateCheckbox} 0 65u 100% 12u "ลบข้อมูลทั้งหมดออกจากเครื่องนี้ด้วย (ลบถาวร)"
  Pop $DataChoiceCheckbox
  ${NSD_SetState} $DataChoiceCheckbox ${BST_UNCHECKED}

  nsDialogs::Show
FunctionEnd

Function un.DataChoicePageLeave
  ${NSD_GetState} $DataChoiceCheckbox $DataChoiceState
FunctionEnd

!endif ; BUILD_UNINSTALLER

; Runs inside Section un.Uninstall, right before files are removed.
; ${isUpdated} is true when this uninstall is really "the new installer
; silently replacing the old version" — that pass always runs with /S, which
; skips custom nsDialogs pages entirely, so $DataChoiceState would never be
; meaningfully set anyway. The explicit guard here is a second, independent
; safety net so an in-place version upgrade can never delete user data.
!macro customUnInstall
  ${ifNot} ${isUpdated}
    ${If} $DataChoiceState == ${BST_CHECKED}
      RMDir /r "$APPDATA\${APP_FILENAME}"
    ${EndIf}
  ${endIf}
!macroend
