#define AppName "SoundFlux"
#define AppVersion "0.1.0"
#define AppPublisher "ihy123"
#define AppExeName "SoundFlux.exe"

[Setup]
AppId={{BCB8F8EC-724F-48C5-A90B-6E949B2295FB}
OutputBaseFilename={#AppName} v{#AppVersion}
OutputDir=.\installer\
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} v{#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "firewall.bat"; DestDir: "{tmp}"
Source: "*"; Excludes: "firewall.bat,installer,make_installer.iss,soundflux.settings.xml"; \
  DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Run]
Filename: "{tmp}\firewall.bat"; Parameters: """{app}\{#AppExeName}"""; Flags: runhidden
  
[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon
