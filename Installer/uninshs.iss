; ======================================================================================================================
;    UninsHs 3
;
;    Author: HAN-SOFT
;    E-Mail: support@han-soft.com
;    WebURL: http://www.han-soft.com
;    Copyright (C) 2001, 2019 Han-soft Corporation. All rights reserved.
; ======================================================================================================================
;    $Rev: 337 $  $Id: uninshs.iss 337 2019-10-23 13:54:49Z hanjy $
; ======================================================================================================================

#ifndef UninsHs_BackupDir
  #define UninsHs_BackupDir "{commonappdata}\$UninsHs$"
#endif

#define BackupExe SetupSetting('OutputBaseFilename')
#if BackupExe == ""
  #define BackupExe "Setup.exe"
#endif
#define AppName  SetupSetting('AppName')
#define AppId  SetupSetting('AppId')
#if AppId == ""
  #define AppId AppName
#endif

#define Use_UninsHs

[Setup]
UsePreviousAppDir = Yes
UsePreviousGroup = Yes
UsePreviousSetupType = Yes
UsePreviousTasks = Yes
UsePreviousUserInfo = Yes
AppModifyPath = "{uninstallexe}" /MODIFY
Uninstallable = Yes
CreateUninstallRegKey = Yes

[Dirs]
Name: "{#UninsHs_BackupDir}"; Attribs: hidden; Flags: uninsalwaysuninstall;

[Files]
#ifndef UninsHs_ImageFilesDir
  #define UninsHs_ImageFilesDir RemoveBackslash(ExtractFileDir(__PATHFILENAME__))
#else
  #define UninsHs_ImageFilesDir RemoveBackslash(UninsHs_ImageFilesDir)
#endif
Source: "{#UninsHs_ImageFilesDir}\modify.bmp"; Flags: dontcopy noencryption
Source: "{#UninsHs_ImageFilesDir}\repair.bmp"; Flags: dontcopy noencryption
Source: "{#UninsHs_ImageFilesDir}\remove.bmp"; Flags: dontcopy noencryption
Source: "{srcexe}"; DestDir: "{#UninsHs_BackupDir}"; DestName: "{#BackupExe}.exe"; Flags: external replacesameversion; \
  Check: not IsMaintenance();

[UninstallDelete]
Type: files; Name: "{#UninsHs_BackupDir}\{#BackupExe}.exe";
Type: dirifempty; Name: "{#UninsHs_BackupDir}"

[Registry]
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{#AppId}_is1"; ValueType: string; \
  ValueName: "Inno Setup: Language"; ValueData: "{language}"; Flags: uninsdeletekey

[CustomMessages]

#ifndef Disable_UninsHs_Default_CustomMessages

MaintenancePageCaption =%1 Program maintenance
MaintenancePageDescription =Modify, repair, or remove the %1 program.
Modify =&Modify
Repair =&Repair
Remove =Re&move
ModifyInfo =Change which program features are installed. This option displays the Custom Selection dialog where you \
            can change the way features are installed.
RepairInfo =Repair errors in the program. This option fixes missing or corrupt files, shortcuts, and registry entries.
RemoveInfo =Remove the program from your computer.
ReadyToRepair =Click on the <next> button to repair the program.
ReadyToRemove =Click on the <next> button to remove the program from your computer.

#endif

[Code]

function CreateImage(OwnerPage: TWizardPage; Filename: string; Left, Top, Width, Height: Integer): TBitmapImage;
var
  BitmapFileName: string;
begin
  BitmapFileName := ExpandConstant('{tmp}\') + Filename;
  ExtractTemporaryFile(ExtractFileName(BitmapFileName));
  Sleep(500);
  Result := TBitmapImage.Create(OwnerPage);
  Result.AutoSize := True;
  Result.Stretch := True;
  Result.BackColor := OwnerPage.Surface.Color;
  Result.ReplaceColor := $FFFFFF;
  Result.ReplaceWithColor := OwnerPage.Surface.Color;
  Result.Bitmap.LoadFromFile(BitmapFileName);
  Result.Left := Left;
  Result.Height := Height;
  Result.Width := Width;
  Result.Top := Top;
  Result.Parent := OwnerPage.Surface;
end;

#ifndef Hs_CreateLabel
#define Hs_CreateLabel
function CreateLabel(OwnerControl: TComponent; Caption: string; Left, Top, Width: Integer; Wrap,
  RightAlignment: Boolean): TLabel;
begin
  Result := TLabel.Create(OwnerControl);
  if OwnerControl = WizardForm then
    Result.Parent := WizardForm
  else
    Result.Parent := TWizardPage(OwnerControl).Surface;
  Result.Caption := Caption;
  Result.Left := Left;
  Result.Top := Top;
  if Wrap or RightAlignment then Result.Width := Width;
  if RightAlignment then Result.Alignment := taRightJustify;
  Result.WordWrap := Wrap;
  Result.AutoSize := True;
end;
#endif

function CreateRadioButton(OwnerPage: TWizardPage; Caption: string; Checked: Boolean; Left, Top: Integer): TRadioButton;
begin
  Result := TRadioButton.Create(OwnerPage);
  Result.Parent := OwnerPage.Surface;
  Result.Caption := Caption;
  Result.Checked := Checked;
  Result.Left := Left;
  Result.Top := Top;
  Result.Width := OwnerPage.SurfaceWidth;
  Result.Height := WizardForm.LICENSEACCEPTEDRADIO.Height;
end;

#ifndef Hs_ClickButton
#define Hs_ClickButton

const
  WM_LBUTTONDOWN = 513;
  WM_LBUTTONUP = 514;

procedure ClickNext();
begin
  PostMessage(WizardForm.NextButton.Handle,WM_LBUTTONDOWN, 0, 0);
  PostMessage(WizardForm.NextButton.Handle,WM_LBUTTONUP, 0, 0);
end;

procedure ClickCancel();
begin
  PostMessage(WizardForm.CancelButton.Handle,WM_LBUTTONDOWN, 0, 0);
  PostMessage(WizardForm.CancelButton.Handle,WM_LBUTTONUP, 0, 0);
end;

#endif

const
  OP_MODIFY = 1;
  OP_REPAIR = 2;
  OP_REMOVE = 3;

var
  MaintenancePage: TWizardPage;
  PrepareMainPage: TOutputMsgWizardPage;
  rdbModify, rdbRepair, rdbRemove: TRadioButton;
  Operate: Integer;

function IsMaintenance(): Boolean;
begin
  Result := Pos('/MAINTEN', UpperCase(GetCmdTail)) > 0;
end;

function IsModify(): Boolean;
begin
  Result := IsMaintenance() and (Pos('/MODIFY', UpperCase(GetCmdTail)) > 0);
end;

function IsRepair(): Boolean;
begin
  Result := IsMaintenance() and (Pos('/REPAIR', UpperCase(GetCmdTail)) > 0);
end;

function IsRemove(): Boolean;
begin
  Result := IsMaintenance() and (Pos('/REMOVE', UpperCase(GetCmdTail)) > 0);
end;

procedure OperatingSelected(Sender: TObject);
begin
  WizardForm.NextButton.Enabled := rdbModify.Checked or rdbRepair.Checked or rdbRemove.Checked;
end;

procedure CreateMaintenance();
#if Ver >= EncodeVer(6, 0, 0)
var
  labModify, labRepair, labRemove: TLabel;
#endif
begin
  MaintenancePage := CreateCustomPage(wpWelcome, ExpandConstant('{cm:MaintenancePageCaption, {#AppName}}'),
    ExpandConstant('{cm:MaintenancePageDescription, {#AppName}}'));
  CreateImage(MaintenancePage, 'modify.bmp', ScaleX(16), ScaleY(42), 32, 32);
  CreateImage(MaintenancePage, 'repair.bmp', ScaleX(16), ScaleY(106), 32, 32);
  CreateImage(MaintenancePage, 'remove.bmp', ScaleX(16), ScaleY(170), 32, 32);
  #if Ver >= EncodeVer(6, 0, 0)
  labModify := 
  #endif
  CreateLabel(MaintenancePage, ExpandConstant('{cm:ModifyInfo}'), ScaleX(70), ScaleY(42),
    MaintenancePage.SurfaceWidth - ScaleX(70), True, False);
  #if Ver >= EncodeVer(6, 0, 0)
  labModify.Anchors := [akTop, akLeft, akRight];
  labRepair := 
  #endif
  CreateLabel(MaintenancePage, ExpandConstant('{cm:RepairInfo}'), ScaleX(70), ScaleY(106),
    MaintenancePage.SurfaceWidth - ScaleX(70), True, False);
  #if Ver >= EncodeVer(6, 0, 0)
  labRepair.Anchors := [akTop, akLeft, akRight];
  labRemove := 
  #endif
  CreateLabel(MaintenancePage, ExpandConstant('{cm:RemoveInfo}'), ScaleX(70), ScaleY(170),
    MaintenancePage.SurfaceWidth - ScaleX(70), True, False);
  #if Ver >= EncodeVer(6, 0, 0)
  labRemove.Anchors := [akTop, akLeft, akRight];
  #endif
  rdbModify := CreateRadioButton(MaintenancePage, ExpandConstant('{cm:Modify}'), IsModify(), ScaleX(0), ScaleY(22));
  rdbModify.Font.Style := [fsBold];
  rdbRepair := CreateRadioButton(MaintenancePage, ExpandConstant('{cm:Repair}'), IsRepair(), ScaleX(0), ScaleY(86));
  rdbRepair.Font.Style := [fsBold];
  rdbRemove := CreateRadioButton(MaintenancePage, ExpandConstant('{cm:Remove}'), IsRemove(), ScaleX(0), ScaleY(150));
  rdbRemove.Font.Style := [fsBold];
  rdbModify.OnClick := @OperatingSelected;
  rdbRepair.OnClick := @OperatingSelected;
  rdbRemove.OnClick := @OperatingSelected;
  PrepareMainPage := CreateOutputMsgPage(MaintenancePage.Id, ExpandConstant('{cm:MaintenancePageCaption, {#AppName}}'),
    ExpandConstant('{cm:MaintenancePageDescription, {#AppName}}'), '');
end;

function RegPathUninstall(): String;
begin
  if IsWin64 then
    Result := 'SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\'
  else
    Result := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\';
end;

// =====================================================================================================================

function UninsHs_IsInstallation(): Boolean;
begin
  Result := not IsMaintenance();
end;

procedure UninsHs_CancelButtonClick(CurPageID: Integer; var Cancel, Confirm: Boolean);
begin
  if IsMaintenance() then Confirm := False;
end;

procedure UninsHs_InitializeWizard();
begin
  if IsMaintenance() then
  begin
    CreateMaintenance();
    #if !YesNo(SetupSetting("DisableWelcomePage"))
    ClickNext();
    #endif
  end;
end;

procedure UninsHs_CurPageChanged(CurPageID: Integer);
begin
  if IsMaintenance() then
  begin
    if CurPageID = MaintenancePage.Id then
    begin
      WizardForm.BackButton.Visible := False;
      OperatingSelected(nil);
    end;
    if CurPageID = PrepareMainPage.Id then
    begin
      case Operate of
        OP_MODIFY: PrepareMainPage.MsgLabel.Caption := '';
        OP_REPAIR: PrepareMainPage.MsgLabel.Caption := ExpandConstant('{cm:ReadyToRepair}');
        OP_REMOVE: PrepareMainPage.MsgLabel.Caption := ExpandConstant('{cm:ReadyToRemove}');
      end;
    end;
    if CurPageID = wpInstalling then
    begin
      // Change the labels caption
    end;
    if CurPageID = wpFinished then
    begin
      // Change the labels caption
    end;
  end;
end;

procedure UninsHs_ShouldSkipPage(CurPageId: Integer; var ChangeResult: Boolean);
begin
  if IsMaintenance() then
  begin
    if CurPageId <> MaintenancePage.Id then
      case Operate of
        OP_MODIFY:
          case CurPageId of
            wpSelectComponents, wpSelectTasks, wpReady, wpPreparing, wpInstalling, wpFinished: ;
          else
            ChangeResult := True;
          end;
        OP_REPAIR:
          case CurPageId of
            PrepareMainPage.Id, wpPreparing, wpInstalling, wpFinished: ;
          else
            ChangeResult := True;
          end;
        OP_REMOVE:
          case CurPageId of
            PrepareMainPage.Id: ;
          else
            ChangeResult := True;
          end;
      end;
  end;
end;

procedure UninsHs_NextButtonClick(CurPageID: Integer; var ChangeResult: Boolean);
var
  ExitCode: Integer;
  Uninstall: string;
  Language: string;
begin
  if IsMaintenance() then
  begin
    if CurPageID = MaintenancePage.Id then
    begin
      if rdbModify.Checked then Operate := OP_MODIFY;
      if rdbRepair.Checked then Operate := OP_REPAIR;
      if rdbRemove.Checked then Operate := OP_REMOVE;
    end
    else if CurPageID = PrepareMainPage.Id then
      case Operate of
        OP_MODIFY:;
        OP_REPAIR:;
        OP_REMOVE:
        begin
          ChangeResult := False;
          RegQueryStringValue(HKEY_LOCAL_MACHINE, RegPathUninstall() + '{#AppId}_is1', 'UninstallString', Uninstall);
          RegQueryStringValue(HKEY_LOCAL_MACHINE, RegPathUninstall() + '{#AppId}_is1', 'Inno Setup: Language',
            Language);
          Exec(RemoveQuotes(Uninstall), ' /RECAll /SILENT /lang=' + Language , '', SW_SHOW, ewWaitUntilIdle, ExitCode);
          ClickCancel();
        end;
      end;
  end;
end;

procedure UninsHs_InitializeUninstall(var ChangeResult: Boolean);
var
  ExitCode: Integer;
  ParamStr: string;
  Language: string;
begin
  if FileExists(ExpandConstant('{#UninsHs_BackupDir}\{#BackupExe}.exe')) and
     (Pos('/RECALL', UpperCase(GetCmdTail)) <= 0) then
  begin
    ParamStr := '';
    if (Pos('/REMOVE', UpperCase(GetCmdTail)) > 0) then ParamStr := '/REMOVE';
    if (Pos('/MODIFY', UpperCase(GetCmdTail)) > 0) then ParamStr := '/MODIFY';
    if (Pos('/REPAIR', UpperCase(GetCmdTail)) > 0) then ParamStr := '/REPAIR';
    if ParamStr = '' then ParamStr := '/REMOVE';
    RegQueryStringValue(HKEY_LOCAL_MACHINE, RegPathUninstall() + '{#AppId}_is1', 'Inno Setup: Language', Language);
    ParamStr := ParamStr + ' /SP-  /lang=' + Language;
    Exec(ExpandConstant('{#UninsHs_BackupDir}\{#BackupExe}.exe'), '/MAINTEN ' + ParamStr, '', SW_SHOW, ewNoWait,
      ExitCode);
    ChangeResult := False;
  end;
end;

[_End]
