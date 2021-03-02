@echo off
cls
Title Building MediaPortal MQTT Plugin (RELEASE)
cd ..

setlocal enabledelayedexpansion

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%
:CONT

: Prepare version
for /f "tokens=*" %%a in ('git rev-list HEAD --count') do set REVISION=%%a 
set REVISION=%REVISION: =%
"scripts\Tools\sed.exe" -i "s/\$WCREV\$/%REVISION%/g" MQTTPlugin\Properties\AssemblyInfo.cs

:: Build
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /target:Rebuild /property:Configuration=DEBUG /fl /flp:logfile=MQTTPlugin.log;verbosity=diagnostic MQTTPlugin.sln

:: Revert version
git checkout MQTTPlugin\Properties\AssemblyInfo.cs

cd scripts

pause

