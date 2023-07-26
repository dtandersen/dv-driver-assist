@echo off
dotnet build
@REM del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\plugins\DriverAssist.dll"
@REM copy /Y DriverAssist\bin\Debug\netframework4.8\DriverAssist.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\scripts


@REM del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\scripts\DriverAssist.dll"
@REM copy /Y bin\Debug\DriverAssist.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\scripts

@REM del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\scripts\DriverAssistBepInEx.dll"
@REM copy /Y bin\Debug\DriverAssistBepInEx.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\scripts


@REM del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist"
@REM mkdir "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist"

mkdir "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist"

del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist\Info.json"
xcopy /Y /F Info.json "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist"

del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist\DriverAssist.dll"
xcopy /Y /F bin\Debug\DriverAssist.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist"

@rem del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist\DriverAssist.UMM.dll"
@rem copy /Y bin\Debug\DriverAssist.UMM.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist
