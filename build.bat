@echo off
dotnet build
@REM del "%DERAIL_VALLEY_HOME%\BepInEx\plugins\DriverAssist.dll"
@REM copy /Y DriverAssist\bin\Debug\netframework4.8\DriverAssist.dll "%DERAIL_VALLEY_HOME%\BepInEx\scripts


@REM del "%DERAIL_VALLEY_HOME%\BepInEx\scripts\DriverAssist.dll"
@REM copy /Y bin\Debug\DriverAssist.dll "%DERAIL_VALLEY_HOME%\BepInEx\scripts

@REM del "%DERAIL_VALLEY_HOME%\BepInEx\scripts\DriverAssistBepInEx.dll"
@REM copy /Y bin\Debug\DriverAssistBepInEx.dll "%DERAIL_VALLEY_HOME%\BepInEx\scripts


@REM del "%DERAIL_VALLEY_HOME%\Mods\DriverAssist"
@REM mkdir "%DERAIL_VALLEY_HOME%\Mods\DriverAssist"

mkdir "%DERAIL_VALLEY_HOME%\Mods\DriverAssist"

del "%DERAIL_VALLEY_HOME%\Mods\DriverAssist\Info.json"
xcopy /Y /F Info.json "%DERAIL_VALLEY_HOME%\Mods\DriverAssist"

del "%DERAIL_VALLEY_HOME%\Mods\DriverAssist\DriverAssist.dll"
xcopy /Y /F bin\Debug\DriverAssist.dll "%DERAIL_VALLEY_HOME%\Mods\DriverAssist"

@rem del "%DERAIL_VALLEY_HOME%\Mods\DriverAssist\DriverAssist.UMM.dll"
@rem copy /Y bin\Debug\DriverAssist.UMM.dll "%DERAIL_VALLEY_HOME%\Mods\DriverAssist
