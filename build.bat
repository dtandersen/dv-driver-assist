dotnet build
@REM del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\plugins\DriverAssist.dll"
@REM copy /Y DriverAssist\bin\Debug\netframework4.8\DriverAssist.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\scripts

del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist\info.json"
copy /Y info.json "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist


del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist\DriverAssist.dll"
copy /Y DriverAssist\bin\Debug\netframework4.8\DriverAssist.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\Mods\DriverAssist
