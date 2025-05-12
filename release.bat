del /q build
dotnet build
dotnet test
rem del "%DERAIL_VALLEY_HOME%\Mods\DriverAssist.*"
rem copy /Y DriverAssist\bin\Debug\netframework4.8\DriverAssist.dll "%DERAIL_VALLEY_HOME%\Mods"
powershell -executionpolicy bypass .\package.ps1
