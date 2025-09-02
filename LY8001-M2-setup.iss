
; -- Example2.iss --
; Same as Example1.iss, but creates its icon in the Programs folder of the
; Start Menu instead of in a subfolder, and also creates a desktop icon.

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!

; ��װԴ�ļ�Ŀ¼
#define srcDir "D:\LY_Meter_Code_M2"
; ��װ���汾��Ӧ��������汾��һ��
#define appVer "1.2"
;#define neice "-�ڲ��"
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
;ѹ�����
Compression=lzma2
SolidCompression=yes
; ����ļ���·��
OutputDir="installation"
; �����װ������
;OutputBaseFilename=LY8001-M2-V{#appVer}-install-x86
OutputBaseFilename=LY8001-M2-V{#appVer}{#neice}-install-x86
;OutputBaseFilename=LY8001-M2-V1.2.0.78-moen-install-x86
ChangesAssociations=yes

[Languages]
; ��Ҫ��ChineseSimplified.isl��ŵ�Inno��װĿ¼��Languages�ļ�������
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
Source: "{#srcDir}\Resource\DataBase\�����ݿ�\MeterData.mdb"; DestDir: "{app}\DataBase";  Flags: uninsneveruninstall onlyifdoesntexist
Source: "{#srcDir}\Resource\DataBase\SchemaData.mdb"; DestDir: "{app}\DataBase"; Flags: uninsneveruninstall onlyifdoesntexist
Source: "{#srcDir}\Resource\DataBase\�����ݿ�\TmpMeterData.mdb"; DestDir: "{app}\DataBase"; Flags: uninsneveruninstall onlyifdoesntexist
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
; ��ʼ�˵���ݷ�ʽ
Name: "{autoprograms}\LY8001-M2"; Filename: "{app}\LYTest.WPF.exe"
; �����ݷ�ʽ
Name: "{autodesktop}\LY8001-M2"; Filename: "{app}\LYTest.WPF.exe"

[Run]
; �����ڳ���װ��ɺ� �ڰ�װ������ʾ���նԻ���֮ǰִ�г��� ���������������� ��ʾ�����ļ� ɾ����ʱ�ļ�
;Filename: "{app}\README.TXT"; Description: "Readme.txt"; Flags: postinstall shellexec skipifsilent unchecked
Filename: "{app}\initialize.EXE"; Description: "��ʼ��"; Flags: postinstall nowait skipifsilent 