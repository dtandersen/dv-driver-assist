del /q build
dotnet build
dotnet test
del "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\scripts\DriverAssist.*
copy /Y DriverAssist\bin\Debug\netframework4.8\DriverAssist.dll "c:\Program Files (x86)\Steam\steamapps\common\Derail Valley\BepInEx\plugins
powershell -executionpolicy bypass .\package.ps1