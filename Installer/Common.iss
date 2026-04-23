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
begin
  Result := False;
  ShouldDeleteAppData := False;

  // 检测进程是否运行
  ProcessRunning := False;
  Exec(ExpandConstant('{cmd}'), '/c tasklist | find /I "SimpleExcelViewer.exe"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  if ResultCode = 0 then ProcessRunning := True;

  CustomForm := TForm.Create(nil);
  try
    CustomForm.ClientWidth := 500;
    CustomForm.ClientHeight := 270;
    CustomForm.Caption := '卸载向导 - Simple Excel Viewer';
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
      InfoLabel.Caption := '警告：检测到 SimpleExcelViewer 正在运行。' + #13#10#13#10 +
                           '点击“继续卸载”将会强制关闭该程序。' + #13#10#13#10 +
                           '请先选择是否保留您的个人配置数据。'
    else
      InfoLabel.Caption := '您正在准备卸载 SimpleExcelViewer。' + #13#10#13#10 +
                           '在继续之前，请选择是否保留您的个人配置数据。';

    // 复选框（下移）
    KeepDataCheck := TNewCheckBox.Create(CustomForm);
    KeepDataCheck.Parent := CustomForm;
    KeepDataCheck.Left := 25;
    KeepDataCheck.Top := 175;
    KeepDataCheck.Width := 450;
    KeepDataCheck.Caption := '保留用户配置数据 (位于 AppData/Local 目录中)';
    KeepDataCheck.Checked := False;

    // 按钮
    BtnUninstall := TNewButton.Create(CustomForm);
    BtnUninstall.Parent := CustomForm;
    BtnUninstall.Left := CustomForm.ClientWidth - 195;
    BtnUninstall.Top := CustomForm.ClientHeight - 45;
    BtnUninstall.Width := 90;
    BtnUninstall.Caption := '继续卸载';
    BtnUninstall.ModalResult := mrOk;

    BtnCancel := TNewButton.Create(CustomForm);
    BtnCancel.Parent := CustomForm;
    BtnCancel.Left := CustomForm.ClientWidth - 95;
    BtnCancel.Top := CustomForm.ClientHeight - 45;
    BtnCancel.Width := 85;
    BtnCancel.Caption := '取消';
    BtnCancel.ModalResult := mrCancel;

    if CustomForm.ShowModal() = mrOk then
    begin
      if ProcessRunning then
      begin
        Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM SimpleExcelViewer.exe /T', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
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