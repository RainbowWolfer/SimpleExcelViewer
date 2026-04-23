; ================================================
; Common.iss - 公共配置（推荐把所有共享部分都放这里）
; ================================================

#include "CodeDependencies.iss"

; ==================== 公共宏定义 ====================
#define MyAppName "Simple Excel Viewer"
#define MyAppName2 "SimpleExcelViewer"
#define MyAppVersion "0.0.2"
#define MyAppPublisher "RainbowWolfer"
#define MyAppURL "https://github.com/RainbowWolfer/SimpleExcelViewer"
#define MyAppExeName "SimpleExcelViewer.exe"
#define MyAppAssocName "CSV File"
#define MyAppAssocExt ".csv"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt
#define MyPublishDir ".\..\bin"

; ==================== 公共 [Setup] 配置 ====================
[Setup]
AppId={{0FFB139D-E4D4-4AE9-BAA3-51F982817A5F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}

WizardStyle=modern dynamic
WizardResizable=yes
CloseApplications=yes
ChangesAssociations=yes
DisableProgramGroupPage=yes
PrivilegesRequiredOverridesAllowed=dialog

Compression=lzma2/ultra
SolidCompression=yes
OutputDir=.\SetupFiles

LicenseFile=License.txt
InfoAfterFile={#MyPublishDir}\README.md
ArchitecturesInstallIn64BitMode=x64

; ==================== 语言和自定义消息 ====================
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "indonesian"; MessagesFile: "compiler:Languages\Indonesian.isl"
Name: "malay"; MessagesFile: "compiler:Languages\Malaysian.isl"

[CustomMessages]
english.ContextMenuName=Open With SimpleExcelViewer
chinesesimplified.ContextMenuName=使用 SimpleExcelViewer 打开
french.ContextMenuName=Ouvrir avec SimpleExcelViewer
german.ContextMenuName=Mit SimpleExcelViewer öffnen
italian.ContextMenuName=Apri con SimpleExcelViewer
japanese.ContextMenuName=SimpleExcelViewer で開く
korean.ContextMenuName=SimpleExcelViewer로 열기
russian.ContextMenuName=Открыть с помощью SimpleExcelViewer
indonesian.ContextMenuName=Buka dengan SimpleExcelViewer
malay.ContextMenuName=Buka dengan SimpleExcelViewer



; 新增：卸载快捷方式相关多语言文字
english.CreateUninstallShortcut=Create uninstall shortcut in Start Menu
chinesesimplified.CreateUninstallShortcut=在开始菜单中创建卸载快捷方式
french.CreateUninstallShortcut=Créer un raccourci de désinstallation dans le menu Démarrer
german.CreateUninstallShortcut=Deinstallationsverknüpfung im Startmenü erstellen
italian.CreateUninstallShortcut=Crea collegamento di disinstallazione nel menu Start
japanese.CreateUninstallShortcut=スタートメニューにアンインストールショートカットを作成
korean.CreateUninstallShortcut=시작 메뉴에 제거 바로가기 만들기
russian.CreateUninstallShortcut=Создать ярлык удаления в меню Пуск
indonesian.CreateUninstallShortcut=Buat pintasan uninstall di Menu Start
malay.CreateUninstallShortcut=Buat pintasan nyahpasang dalam Menu Start

english.UninstallProgram=Uninstall {#MyAppName}
chinesesimplified.UninstallProgram=卸载 {#MyAppName}
french.UninstallProgram=Désinstaller {#MyAppName}
german.UninstallProgram={#MyAppName} deinstallieren
italian.UninstallProgram=Disinstalla {#MyAppName}
japanese.UninstallProgram={#MyAppName} をアンインストール
korean.UninstallProgram={#MyAppName} 제거
russian.UninstallProgram=Удалить {#MyAppName}
indonesian.UninstallProgram=Uninstall {#MyAppName}
malay.UninstallProgram=Nyahpasang {#MyAppName}

; ==================== 卸载向导多语言文字 ====================
english.UninstallWizardTitle=Uninstall Wizard - Simple Excel Viewer
chinesesimplified.UninstallWizardTitle=卸载向导 - Simple Excel Viewer
french.UninstallWizardTitle=Assistant de désinstallation - Simple Excel Viewer
german.UninstallWizardTitle=Deinstallations-Assistent - Simple Excel Viewer
italian.UninstallWizardTitle=Procedura guidata di disinstallazione - Simple Excel Viewer
japanese.UninstallWizardTitle=アンインストールウィザード - Simple Excel Viewer
korean.UninstallWizardTitle=제거 마법사 - Simple Excel Viewer
russian.UninstallWizardTitle=Мастер удаления - Simple Excel Viewer
indonesian.UninstallWizardTitle=Wizard Uninstall - Simple Excel Viewer
malay.UninstallWizardTitle=Wizard Nyahpasang - Simple Excel Viewer

english.UninstallProcessRunning=Warning: SimpleExcelViewer is detected to be running.%nClicking "Continue Uninstall" will force close the program.
chinesesimplified.UninstallProcessRunning=警告：检测到 SimpleExcelViewer 正在运行。%n点击“继续卸载”将会强制关闭该程序。
french.UninstallProcessRunning=Avertissement : SimpleExcelViewer est détecté en cours d'exécution.%nCliquer sur "Continuer la désinstallation" fermera de force le programme.
german.UninstallProcessRunning=Warnung: SimpleExcelViewer wird als laufend erkannt.%nDurch Klicken auf "Weiter deinstallieren" wird das Programm zwangsweise geschlossen.
italian.UninstallProcessRunning=Avviso: SimpleExcelViewer è in esecuzione.%nCliccando su "Continua disinstallazione" il programma verrà chiuso forzatamente.
japanese.UninstallProcessRunning=警告：SimpleExcelViewer が実行中であることが検出されました。%n「続行してアンインストール」をクリックするとプログラムが強制終了されます。
korean.UninstallProcessRunning=경고: SimpleExcelViewer가 실행 중인 것으로 감지되었습니다.%n"계속 제거"를 클릭하면 프로그램이 강제 종료됩니다.
russian.UninstallProcessRunning=Предупреждение: Обнаружено, что SimpleExcelViewer запущен.%nНажатие "Продолжить удаление" принудительно закроет программу.
indonesian.UninstallProcessRunning=Peringatan: SimpleExcelViewer terdeteksi sedang berjalan.%nMengklik "Lanjutkan Uninstall" akan memaksa menutup program.
malay.UninstallProcessRunning=Amaran: SimpleExcelViewer dikesan sedang berjalan.%nMengklik "Teruskan Nyahpasang" akan memaksa tutup program.

english.UninstallPreparing=You are about to uninstall SimpleExcelViewer.
chinesesimplified.UninstallPreparing=您正在准备卸载 SimpleExcelViewer。
french.UninstallPreparing=Vous êtes sur le point de désinstaller SimpleExcelViewer.
german.UninstallPreparing=Sie sind dabei, SimpleExcelViewer zu deinstallieren.
italian.UninstallPreparing=Stai per disinstallare SimpleExcelViewer.
japanese.UninstallPreparing=SimpleExcelViewer をアンインストールしようとしています。
korean.UninstallPreparing=SimpleExcelViewer를 제거하려고 합니다.
russian.UninstallPreparing=Вы собираетесь удалить SimpleExcelViewer.
indonesian.UninstallPreparing=Anda akan menghapus SimpleExcelViewer.
malay.UninstallPreparing=Anda akan menyahpasang SimpleExcelViewer.

english.KeepUserData=Keep user configuration data (located in AppData/Local directory)
chinesesimplified.KeepUserData=保留用户配置数据 (位于 AppData/Local 目录中)
french.KeepUserData=Conserver les données de configuration utilisateur (situées dans le répertoire AppData/Local)
german.KeepUserData=Benutzerkonfigurationsdaten beibehalten (im AppData/Local-Verzeichnis)
italian.KeepUserData=Mantieni i dati di configurazione utente (situati nella directory AppData/Local)
japanese.KeepUserData=ユーザー設定データを保持する (AppData/Local ディレクトリにあります)
korean.KeepUserData=사용자 구성 데이터 유지 (AppData/Local 디렉토리에 위치)
russian.KeepUserData=Сохранить данные конфигурации пользователя (расположены в каталоге AppData/Local)
indonesian.KeepUserData=Simpan data konfigurasi pengguna (berada di direktori AppData/Local)
malay.KeepUserData=Simpan data konfigurasi pengguna (terletak di direktori AppData/Local)

english.ContinueUninstall=Continue Uninstall
chinesesimplified.ContinueUninstall=继续卸载
french.ContinueUninstall=Continuer la désinstallation
german.ContinueUninstall=Weiter deinstallieren
italian.ContinueUninstall=Continua disinstallazione
japanese.ContinueUninstall=アンインストールを続行
korean.ContinueUninstall=계속 제거
russian.ContinueUninstall=Продолжить удаление
indonesian.ContinueUninstall=Lanjutkan Uninstall
malay.ContinueUninstall=Teruskan Nyahpasang

english.UninstallCancel=Cancel
chinesesimplified.UninstallCancel=取消
french.UninstallCancel=Annuler
german.UninstallCancel=Abbrechen
italian.UninstallCancel=Annulla
japanese.UninstallCancel=キャンセル
korean.UninstallCancel=취소
russian.UninstallCancel=Отмена
indonesian.UninstallCancel=Batal
malay.UninstallCancel=Batal


; ==================== 以下全部为两个脚本完全相同的部分 ====================
[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "uninstshortcut"; Description: "{cm:CreateUninstallShortcut}"; GroupDescription: "{cm:AdditionalIcons}"

[Registry]
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}"; ValueType: string; ValueName: "FriendlyTypeName"; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocExt}\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppAssocKey}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\SystemFileAssociations\{#MyAppAssocExt}\shell\OpenWithSimpleExcelViewer"; ValueType: string; ValueName: ""; ValueData: "{cm:ContextMenuName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\SystemFileAssociations\{#MyAppAssocExt}\shell\OpenWithSimpleExcelViewer"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\{#MyAppExeName},0"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\SystemFileAssociations\{#MyAppAssocExt}\shell\OpenWithSimpleExcelViewer\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Microsoft\Windows\CurrentVersion\App Paths\{#MyAppExeName}"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey

[Icons]
Name: "{autoprograms}\{#MyAppName}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{autoprograms}\{#MyAppName}\{cm:UninstallProgram}"; Filename: "{uninstallexe}"; Tasks: uninstshortcut

[UninstallDelete]
; 卸载时强行删除整个安装目录及其内部的所有文件和子文件夹
Type: filesandordirs; Name: "{app}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup: Boolean;
begin
  Dependency_AddDotNet80Desktop;
  Result := True;
end;

// 全局变量，用于记录用户是否选择删除配置数据
var
  ShouldDeleteAppData: Boolean;
  
  function PromptUninstallOptions(): Boolean;
var
  CustomForm: TForm;
  ProcessRunning: Boolean;
  ResultCode: Integer;
  InfoLabel: TLabel;
  KeepDataCheck: TNewCheckBox;
  BtnUninstall, BtnCancel: TNewButton;
  TempLabel: TLabel;
begin
	// MsgBox('Current Language: ' + ActiveLanguage + #13#10 + 'Raw CM: ' + ExpandConstant('{cm:UninstallWizardTitle}'), mbInformation, MB_OK);
  Result := False;
  ShouldDeleteAppData := False;
  // 检测进程是否运行
  ProcessRunning := False;
  Exec(ExpandConstant('{cmd}'), '/c tasklist | find /I "{#MyAppExeName}"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  if ResultCode = 0 then ProcessRunning := True;
  CustomForm := TForm.Create(nil);
  try
    CustomForm.ClientWidth := 500;
    CustomForm.ClientHeight := 270;
    CustomForm.Caption := ExpandConstant('{cm:UninstallWizardTitle}');
    CustomForm.Position := poScreenCenter;
    CustomForm.BorderStyle := bsDialog;
    InfoLabel := TLabel.Create(CustomForm);
    InfoLabel.Parent := CustomForm;
    InfoLabel.Left := 25;
    InfoLabel.Top := 20;
    InfoLabel.AutoSize := False;
    InfoLabel.Width := 450;
    InfoLabel.Height := 145;
    InfoLabel.WordWrap := True;
    if ProcessRunning then
      InfoLabel.Caption := ExpandConstant('{cm:UninstallProcessRunning}')
    else
      InfoLabel.Caption := ExpandConstant('{cm:UninstallPreparing}');
    // 复选框
    KeepDataCheck := TNewCheckBox.Create(CustomForm);
    KeepDataCheck.Parent := CustomForm;
    KeepDataCheck.Left := 25;
    KeepDataCheck.Top := 175;
    KeepDataCheck.Width := 450;
    KeepDataCheck.Caption := ExpandConstant('{cm:KeepUserData}');
    KeepDataCheck.Checked := False;

    // 按钮
    // --- 先创建按钮并赋值文字 ---
    
    BtnUninstall := TNewButton.Create(CustomForm);
    BtnUninstall.Parent := CustomForm;
    BtnUninstall.Caption := ExpandConstant('{cm:ContinueUninstall}');
    BtnUninstall.ModalResult := mrOk;
    BtnUninstall.Top := CustomForm.ClientHeight - 45;
    
    BtnCancel := TNewButton.Create(CustomForm);
    BtnCancel.Parent := CustomForm;
    BtnCancel.Caption := ExpandConstant('{cm:UninstallCancel}');
    BtnCancel.ModalResult := mrCancel;
    BtnCancel.Top := CustomForm.ClientHeight - 45;

    // --- 创建隐藏的 Label 作为尺子 ---
    TempLabel := TLabel.Create(CustomForm);
    TempLabel.Parent := CustomForm;
    TempLabel.AutoSize := True;
    TempLabel.Visible := False; // 必须隐藏，不让用户看到

    // --- 动态计算宽度 ---
    
    // 测算取消按钮
    TempLabel.Caption := BtnCancel.Caption;
    BtnCancel.Width := TempLabel.Width + 30; // 标签自动撑开后的宽度 + 左右各15px内边距
    if BtnCancel.Width < 85 then BtnCancel.Width := 85; // 保底宽度
    
    // 测算卸载按钮
    TempLabel.Caption := BtnUninstall.Caption;
    BtnUninstall.Width := TempLabel.Width + 30; // 标签自动撑开后的宽度 + 左右各15px内边距
    if BtnUninstall.Width < 90 then BtnUninstall.Width := 90; // 保底宽度
    
    // --- 动态计算位置 (从右向左排版) ---
    
    // 最右侧留白 10 像素
    BtnCancel.Left := CustomForm.ClientWidth - BtnCancel.Width - 10;
    
    // 卸载按钮放在取消按钮的左边，中间留白 10 像素
    BtnUninstall.Left := BtnCancel.Left - BtnUninstall.Width - 10;
	
	
    if CustomForm.ShowModal() = mrOk then
    begin
      if ProcessRunning then
      begin
        Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM {#MyAppExeName} /T', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
        Sleep(500);
      end;
      ShouldDeleteAppData := not KeepDataCheck.Checked;
      Result := True;
    end;
  finally
    CustomForm.Free;
  end;
end;

// 关键修改：把自定义窗口放在这里
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    // 显示自定义卸载选项窗口
    if not PromptUninstallOptions() then
    begin
      // 用户点击了取消，中止卸载
      Abort;
    end;
  end
  else if CurUninstallStep = usPostUninstall then
  begin
    // 卸载完成后清理配置数据
    if ShouldDeleteAppData then
    begin
      DelTree(ExpandConstant('{localappdata}\RainbowWolfer\SimpleExcelViewer'), True, True, True);
    end;
  end;
end;