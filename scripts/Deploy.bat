@echo off

Title Deploying MediaPortal FanartHandler (RELEASE)
cd ..

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%
:CONT
IF NOT EXIST "%PROGS%\Team MediaPortal\MediaPortal\" SET PROGS=C:

copy /y "MQTTPlugin\bin\Release\MQTTPlugin.dll" "%PROGS%\Team MediaPortal\MediaPortal\plugins\process\"
copy /y "MQTTPlugin\bin\Release\M2Mqtt.dll" "%PROGS%\Team MediaPortal\MediaPortal\plugins\process\"

cd scripts