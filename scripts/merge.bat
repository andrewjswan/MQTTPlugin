@echo off

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
    :: 64-bit
    set PROGS=%programfiles(x86)%
    goto CONT
:32BIT
    set PROGS=%ProgramFiles%
:CONT

if exist MQTTPlugin_UNMERGED.dll del MQTTPlugin_UNMERGED.dll
ren MQTTPlugin.dll MQTTPlugin_UNMERGED.dll 
ilmerge.exe /out:MQTTPlugin.dll MQTTPlugin_UNMERGED.dll Nlog.dll /target:dll /targetplatform:"v4,%PROGS%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /wildcards
