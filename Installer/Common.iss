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

AppMutex=SimpleExcelViewerMutex

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
  
  
  // 自定义卸载选项页面
function PromptUninstallOptions(): Boolean;
var
  CustomForm: TForm;
  ProcessRunning: Boolean;
  ResultCode: Integer;
  InfoLabel: TLabel;
  KeepDataCheck: TNewCheckBox;
  BtnUninstall, BtnCancel: TNewButton;
begin
  Result := False; // 默认返回 False (取消卸载)
  ShouldDeleteAppData := False; 

  // 1. 静默检测进程是否在运行
  ProcessRunning := False;
  Exec(ExpandConstant('{cmd}'), '/c tasklist | find /I "SimpleExcelViewer.exe"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  if ResultCode = 0 then ProcessRunning := True;

  // 2. 创建自定义普通窗体
  CustomForm := TForm.Create(nil); 
  try
    CustomForm.ClientWidth := 420;
    CustomForm.ClientHeight := 180;
    CustomForm.Caption := '卸载向导 - SimpleExcelViewer'; // 修改标题更正式
    CustomForm.Position := poScreenCenter; 
    CustomForm.BorderStyle := bsDialog;    

    // 3. 添加提示文本 (Label)
    InfoLabel := TLabel.Create(CustomForm);
    InfoLabel.Parent := CustomForm;
    InfoLabel.Left := 20;
    InfoLabel.Top := 20;
    InfoLabel.AutoSize := False; // 【关键修改】：关闭自动缩放
    InfoLabel.Width := 380;
    InfoLabel.Height := 60;      // 【关键修改】：给定固定高度，防止文字挤压
    InfoLabel.WordWrap := True;
    
    if ProcessRunning then
    begin
      InfoLabel.Caption := '警告：检测到 SimpleExcelViewer 正在运行。' + #13#10 + '点击“继续卸载”将会强制关闭该程序。在此之前，请选择是否保留您的个人配置数据。';
      InfoLabel.Font.Color := clRed; 
    end
    else
    begin
      InfoLabel.Caption := '您正在准备卸载 SimpleExcelViewer。' + #13#10 + '在继续之前，请选择是否保留您的个人配置数据。';
    end;

    // 4. 添加复选框 (Checkbox)
    KeepDataCheck := TNewCheckBox.Create(CustomForm);
    KeepDataCheck.Parent := CustomForm;
    KeepDataCheck.Left := 20;
    KeepDataCheck.Top := 85;     // 【关键修改】：往下移，给上面的文字留足空间
    KeepDataCheck.Width := 380;
    KeepDataCheck.Caption := '保留用户配置数据 (位于 AppData 目录中)';
    KeepDataCheck.Checked := True; // 默认勾选“保留”

    // 5. 添加“继续卸载”按钮
    BtnUninstall := TNewButton.Create(CustomForm);
    BtnUninstall.Parent := CustomForm;
    BtnUninstall.Left := CustomForm.ClientWidth - 190;
    BtnUninstall.Top := CustomForm.ClientHeight - 45;
    BtnUninstall.Width := 80;
    BtnUninstall.Caption := '继续卸载';
    BtnUninstall.ModalResult := mrOk; 

    // 6. 添加“取消”按钮
    BtnCancel := TNewButton.Create(CustomForm);
    BtnCancel.Parent := CustomForm;
    BtnCancel.Left := CustomForm.ClientWidth - 100;
    BtnCancel.Top := CustomForm.ClientHeight - 45;
    BtnCancel.Width := 80;
    BtnCancel.Caption := '取消';
    BtnCancel.ModalResult := mrCancel; 

    // 7. 显示窗体并等待用户点击
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

// 卸载初始化函数
function InitializeUninstall(): Boolean;
begin
  // 调用自定义界面，用户点击了“取消”或者直接关闭了窗口，就中止卸载
  Result := PromptUninstallOptions();
end;

// 卸载过程状态改变时的回调函数（用于在卸载完文件后清理 AppData）
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  // 当标准卸载步骤完成 (usPostUninstall) 时执行
  if CurUninstallStep = usPostUninstall then
  begin
    if ShouldDeleteAppData then
    begin
      // 使用 DelTree 删除指定的 AppData 文件夹。
      // 请根据你 C# 程序实际存储的路径修改下面这行代码！
      // {userappdata} 代表 C:\Users\用户名\AppData\Roaming
      // {localappdata} 代表 C:\Users\用户名\AppData\Local
      DelTree(ExpandConstant('{userappdata}\SimpleExcelViewer'), True, True, True);
    end;
  end;
end;