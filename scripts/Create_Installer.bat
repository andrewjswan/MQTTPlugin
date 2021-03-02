@echo off
cls
Title Creating MediaPortal MQTT Plugin Installer

:: Check for modification
REM svn status ..\source | findstr "^M"
REM if ERRORLEVEL 1 (
REM    echo No modifications in source folder.
REM ) else (
REM    echo There are modifications in source folder. Aborting.
REM    pause
REM    exit 1
REM )

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
    :: 64-bit
    set PROGS=%programfiles(x86)%
    goto CONT
:32BIT
    set PROGS=%ProgramFiles%
:CONT

IF NOT EXIST "%PROGS%\Team MediaPortal\MediaPortal\" SET PROGS=C:

:: Get version from DLL
FOR /F "tokens=1-3" %%i IN ('Tools\sigcheck.exe "..\MQTTPlugin\bin\Release\MQTTPlugin.dll"') DO ( IF "%%i %%j"=="File version:" SET version=%%k )

:: trim version
SET version=%version:~0,-1%

:: Temp xmp2 file
copy MQTTPlugin.xmp2 MQTTPluginTemp.xmp2

:: Sed "MQTTPlugin-{VERSION}.xml" from xmp2 file
Tools\sed.exe -i "s/MQTTPlugin-{VERSION}.xml/MQTTPlugin-%version%.xml/g" MQTTPluginTemp.xmp2

:: Build MPE1
"%PROGS%\Team MediaPortal\MediaPortal\MPEMaker.exe" MQTTPluginTemp.xmp2 /B /V=%version% /UpdateXML

:: Cleanup
del MQTTPluginTemp.xmp2

:: Sed "MQTTPlugin-{VERSION}.mpe1" from MQTTPlugin.xml
Tools\sed.exe -i "s/MQTTPlugin-{VERSION}.MPE1/MQTTPlugin-%version%.mpe1/g" MQTTPlugin-%version%.xml

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


