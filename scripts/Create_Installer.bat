@echo off
cls
Title Creating MediaPortal MQTT Plugin Installer

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
    :: 64-bit
    set PROGS=%programfiles(x86)%
    goto CONT
:32BIT
    set PROGS=%ProgramFiles%
:CONT

IF NOT EXIST "%PROGS%\Team MediaPortal\MediaPortal\" SET PROGS=C:

:: Get version from DLL
FOR /F "tokens=*" %%i IN ('Tools\sigcheck.exe /accepteula /nobanner /n "..\MQTTPlugin\bin\Release\MQTTPlugin.dll"') DO (SET version=%%i)

:: Temp xmp2 file
copy /Y MQTTPlugin.xmp2 MQTTPluginTemp.xmp2

:: Sed "{VERSION}" from xmp2 file
Tools\sed.exe -i "s/{VERSION}/%version%/g" MQTTPluginTemp.xmp2

:: Build MPE1
"%PROGS%\Team MediaPortal\MediaPortal\MPEMaker.exe" MQTTPluginTemp.xmp2 /B /V=%version% /UpdateXML

:: Cleanup
del MQTTPluginTemp.xmp2

:: Parse version (Might be needed in the futute)
FOR /F "tokens=1-4 delims=." %%i IN ("%version%") DO ( 
    SET major=%%i
    SET minor=%%j
    SET build=%%k
    SET revision=%%l
)

:: Rename MPE1
if exist "..\builds\MQTTPlugin-%major%.%minor%.%build%.%revision%.mpe1" del "..\builds\MQTTPlugin-%major%.%minor%.%build%.%revision%.mpe1"
rename ..\builds\MQTTPlugin-MAJOR.MINOR.BUILD.REVISION.mpe1 "MQTTPlugin-%major%.%minor%.%build%.%revision%.mpe1"
