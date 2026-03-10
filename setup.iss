[Setup]
AppName=Wii Converter Pro
AppVersion=1.0
DefaultDirName={autopf}\Wii Converter Pro
DefaultGroupName=Wii Converter Pro
OutputDir=bin\Installer
OutputBaseFilename=WiiConverterSetup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
DisableDirPage=no

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\Release\net10.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Wii Converter Pro"; Filename: "{app}\WiiConverterDesktop.exe"
Name: "{commondesktop}\Wii Converter Pro"; Filename: "{app}\WiiConverterDesktop.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\WiiConverterDesktop.exe"; Description: "{cm:LaunchProgram,Wii Converter Pro}"; Flags: nowait postinstall skipifsilent
