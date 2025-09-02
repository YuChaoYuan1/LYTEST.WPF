
; -- Example2.iss --
; Same as Example1.iss, but creates its icon in the Programs folder of the
; Start Menu instead of in a subfolder, and also creates a desktop icon.

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!

; 安装源文件目录
#define srcDir "D:\LY_Meter_Code_M2"
; 安装包版本号应该与软件版本号一致
#define appVer "1.2"
;#define neice "-内测版"
#define neice ""


[Setup]
AppName=LY8001-M2
AppVersion={#appVer}
VersionInfoVersion={#appVer}
WizardStyle=modern
DefaultDirName=D:\LY8001-M2
; Since no icons will be created in "{group}", we don't need the wizard
; to ask for a Start Menu folder name:
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\LYTest.WPF.exe
;压缩相关
Compression=lzma2
SolidCompression=yes
; 输出文件夹路径
OutputDir="installation"
; 输出安装包名称
;OutputBaseFilename=LY8001-M2-V{#appVer}-install-x86
OutputBaseFilename=LY8001-M2-V{#appVer}{#neice}-install-x86
;OutputBaseFilename=LY8001-M2-V1.2.0.78-moen-install-x86
ChangesAssociations=yes

[Languages]
; 需要将ChineseSimplified.isl存放到Inno安装目录的Languages文件夹里面
Name: zh; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Files]
Source: "{#srcDir}\Resource\initialize.exe"; DestDir: "{app}"; Flags: ignoreversion 
Source: "{#srcDir}\Resource\LYbackup.exe"; DestDir: "{app}"; Flags: ignoreversion    
Source: "{#srcDir}\Resource\LYTest.WPF.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#srcDir}\Resource\LYTest.DataManager.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#srcDir}\Resource\LYTest.WordTemplate.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#srcDir}\Resource\*.dll"; DestDir: "{app}"; Flags: ignoreversion
;Source: "Resource\DataBase\InherentAppData.mdb"; DestDir: "{app}\DataBase"; Flags: uninsneveruninstall onlyifdoesntexist 
Source: "{#srcDir}\Resource\DataBase\AppData.mdb"; DestDir: "{app}\DataBase"; Flags: uninsneveruninstall onlyifdoesntexist
Source: "{#srcDir}\Resource\DataBase\空数据库\MeterData.mdb"; DestDir: "{app}\DataBase";  Flags: uninsneveruninstall onlyifdoesntexist
Source: "{#srcDir}\Resource\DataBase\SchemaData.mdb"; DestDir: "{app}\DataBase"; Flags: uninsneveruninstall onlyifdoesntexist
Source: "{#srcDir}\Resource\DataBase\空数据库\TmpMeterData.mdb"; DestDir: "{app}\DataBase"; Flags: uninsneveruninstall onlyifdoesntexist
Source: "{#srcDir}\Resource\System\*.xlsx"; DestDir: "{app}\System"; Flags: uninsneveruninstall onlyifdoesntexist
Source: "{#srcDir}\Resource\Devices\*.dll"; DestDir: "{app}\Devices"; Flags: ignoreversion
Source: "{#srcDir}\Resource\EncrypGW\*.dll"; DestDir: "{app}\EncrypGW"; Flags: uninsneveruninstall ignoreversion
Source: "{#srcDir}\Resource\images\*.png"; DestDir: "{app}\images"
Source: "{#srcDir}\Resource\images\*.jpg"; DestDir: "{app}\images"
Source: "{#srcDir}\Resource\images\*.ico"; DestDir: "{app}\images"
Source: "{#srcDir}\Resource\Ini\*.ini"; DestDir: "{app}\Ini"; Flags: uninsneveruninstall onlyifdoesntexist
;Source: "{#srcDir}\Resource\Log\Config\*.config"; DestDir: "{app}\Log\Config"; Flags: uninsneveruninstall onlyifdoesntexist
Source: "{#srcDir}\Resource\Res\Word\Tem\*.doc"; DestDir: "{app}\Res\Word\Tem"; Flags: uninsneveruninstall onlyifdoesntexist
Source: "{#srcDir}\Resource\Res\Word\*.doc"; DestDir: "{app}\Res\Word"; Flags: uninsneveruninstall onlyifdoesntexist skipifsourcedoesntexist
Source: "{#srcDir}\Resource\Res\Word\*.docx"; DestDir: "{app}\Res\Word"; Flags: skipifsourcedoesntexist onlyifdoesntexist 
Source: "{#srcDir}\Resource\Res\*.ocx"; DestDir: "{app}\Res";
Source: "{#srcDir}\Resource\Res\*.bat"; DestDir: "{app}\Res";
Source: "{#srcDir}\Resource\Skin\*.ini"; DestDir: "{app}\Skin"
Source: "{#srcDir}\Resource\Skin\*.xaml"; DestDir: "{app}\Skin"
Source: "{#srcDir}\Resource\Xml\AgreementConfig.xml"; DestDir: "{app}\Xml"; Flags: uninsneveruninstall onlyifdoesntexist
Source: "{#srcDir}\Resource\Xml\CarrierConfig.xml"; DestDir: "{app}\Xml"; Flags: uninsneveruninstall 
Source: "{#srcDir}\Resource\Xml\DataFlag.xml"; DestDir: "{app}\Xml"; Flags: ignoreversion
Source: "{#srcDir}\Resource\Xml\OadInfosConfig.xml"; DestDir: "{app}\Xml"; Flags: ignoreversion

;Source: "Resource\Readme.txt"; DestDir: "{app}"

[Icons]
; 开始菜单快捷方式
Name: "{autoprograms}\LY8001-M2"; Filename: "{app}\LYTest.WPF.exe"
; 桌面快捷方式
Name: "{autodesktop}\LY8001-M2"; Filename: "{app}\LYTest.WPF.exe"

[Run]
; 用来在程序安装完成后 在安装程序显示最终对话框之前执行程序 常用与运行主程序 显示自述文件 删除临时文件
;Filename: "{app}\README.TXT"; Description: "Readme.txt"; Flags: postinstall shellexec skipifsilent unchecked
Filename: "{app}\initialize.EXE"; Description: "初始化"; Flags: postinstall nowait skipifsilent 