dotnet build
@REM del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\plugins\DriverAssist.dll"
@REM copy /Y DriverAssist\bin\Debug\netframework4.8\DriverAssist.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\scripts


del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\scripts\DriverAssist.dll"
copy /Y bin\Debug\DriverAssist.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\scripts

del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\scripts\DriverAssistBepInEx.dll"
copy /Y bin\Debug\DriverAssistBepInEx.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\scripts



del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist\Info.json"
copy /Y Info.json "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist

del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist\DriverAssist.dll"
copy /Y bin\Debug\DriverAssist.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist

del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist\DriverAssist.UMM.dll"
copy /Y bin\Debug\DriverAssist.UMM.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist
