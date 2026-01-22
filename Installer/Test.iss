#include <idp.iss>

// Other parts of installer file go here

[CustomMessages]
IDP_DownloadFailed=Download of .NET Framework 4.7.2 failed. .NET Framework 4.7 is required to run VidCoder.
IDP_RetryCancel=Click 'Retry' to try downloading the files again, or click 'Cancel' to terminate setup.
InstallingDotNetFramework=Installing .NET Framework 4.7.2. This might take a few minutes...
DotNetFrameworkFailedToLaunch=Failed to launch .NET Framework Installer with error "%1". Please fix the error then run this installer again.
DotNetFrameworkFailed1602=.NET Framework installation was cancelled. This installation can continue, but be aware that this application may not run unless the .NET Framework installation is completed successfully.
DotNetFrameworkFailed1603=A fatal error occurred while installing the .NET Framework. Please fix the error, then run the installer again.
DotNetFrameworkFailed5100=Your computer does not meet the requirements of the .NET Framework. Please consult the documentation.
DotNetFrameworkFailedOther=The .NET Framework installer exited with an unexpected status code "%1". Please review any other messages shown by the installer to determine whether the installation completed successfully, and abort this installation and fix the problem if it did not.

[Code]

var
  requiresRestart: boolean;

function NetFrameworkIsMissing(): Boolean;
var
  bSuccess: Boolean;
  regVersion: Cardinal;
begin
  Result := True;

  bSuccess := RegQueryDWordValue(HKLM, 'Software\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', regVersion);
  if (True = bSuccess) and (regVersion >= 461308) then begin
    Result := False;
  end;
end;

procedure InitializeWizard;
begin
  if NetFrameworkIsMissing() then
  begin
    idpAddFile('http://go.microsoft.com/fwlink/?LinkId=863262', ExpandConstant('{tmp}\NetFrameworkInstaller.exe'));
    idpDownloadAfter(wpReady);
  end;
end;

function InstallFramework(): String;
var
  StatusText: string;
  ResultCode: Integer;
begin
  StatusText := WizardForm.StatusLabel.Caption;
  WizardForm.StatusLabel.Caption := CustomMessage('InstallingDotNetFramework');
  WizardForm.ProgressGauge.Style := npbstMarquee;
  try
    if not Exec(ExpandConstant('{tmp}\NetFrameworkInstaller.exe'), '/passive /norestart /showrmui /showfinalerror', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
    begin
      Result := FmtMessage(CustomMessage('DotNetFrameworkFailedToLaunch'), [SysErrorMessage(resultCode)]);
    end
    else
    begin
      // See https://msdn.microsoft.com/en-us/library/ee942965(v=vs.110).aspx#return_codes
      case resultCode of
        0: begin
          // Successful
        end;
        1602 : begin
          MsgBox(CustomMessage('DotNetFrameworkFailed1602'), mbInformation, MB_OK);
        end;
        1603: begin
          Result := CustomMessage('DotNetFrameworkFailed1603');
        end;
        1641: begin
          requiresRestart := True;
        end;
        3010: begin
          requiresRestart := True;
        end;
        5100: begin
          Result := CustomMessage('DotNetFrameworkFailed5100');
        end;
        else begin
          MsgBox(FmtMessage(CustomMessage('DotNetFrameworkFailedOther'), [IntToStr(resultCode)]), mbError, MB_OK);
        end;
      end;
    end;
  finally
    WizardForm.StatusLabel.Caption := StatusText;
    WizardForm.ProgressGauge.Style := npbstNormal;
    
    DeleteFile(ExpandConstant('{tmp}\NetFrameworkInstaller.exe'));
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  // 'NeedsRestart' only has an effect if we return a non-empty string, thus aborting the installation.
  // If the installers indicate that they want a restart, this should be done at the end of installation.
  // Therefore we set the global 'restartRequired' if a restart is needed, and return this from NeedRestart()

  if NetFrameworkIsMissing() then
  begin
    Result := InstallFramework();
  end;
end;

function NeedRestart(): Boolean;
begin
  Result := requiresRestart;
end;