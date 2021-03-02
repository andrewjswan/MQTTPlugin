@echo off
cls
Title Building MediaPortal MQTT Plugin (RELEASE)
cd ..

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%
:CONT

: Prepare version
subwcrev . MQTTPlugin\Properties\AssemblyInfo.cs MQTTPlugin\Properties\AssemblyInfo.cs
	
:: Build
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /target:Rebuild /property:Configuration=RELEASE /fl /flp:logfile=MQTTPlugin.log;verbosity=diagnostic MQTTPlugin.sln

: Revert version
svn revert MQTTPlugin\Properties\AssemblyInfo.cs

cd scripts

pause

