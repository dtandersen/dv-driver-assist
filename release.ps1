if ($PSScriptRoot -match '.+?\\bin\\?') {
  $dir = $PSScriptRoot + "\Debug\"
}
else {
  $dir = $PSScriptRoot + "\bin\Debug\"
}


$out = $PSScriptRoot + "\dist\"
$copy = $PSScriptRoot + "\temp\"
$BIEdir = $dir + "BepInEx\"
$Coredir = $dir + ""
$UMMdir = $dir + ""

Write-Output $out
Write-Output $copy

$ver = (Get-Item ($Coredir + "DriverAssist.dll")).VersionInfo.FileVersion.ToString()

New-Item -ItemType Directory -Force -Path ($out)  


Remove-Item -Force -Path ($copy) -Recurse -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path ($copy)
Copy-Item -Path ("Info.json") -Destination ($copy) -Recurse -Force


Copy-Item -Path ($UMMdir + "DriverAssist.dll") -Destination ($copy) -Recurse -Force
# Copy-Item -Path ($Coredir + "DriverAssist.BepInEx.dll") -Destination ($copy) -Recurse -Force
# $info = Get-Content ($copy + "Info.json") -raw | ConvertFrom-Json
# $info.Version = $ver
# $info | ConvertTo-Json | Set-Content ($copy + "Info.json")

Compress-Archive -Path ($copy + "*") -Force -CompressionLevel "Optimal" -DestinationPath ($out + "DriverAssist_BepInEx_v" + $ver + ".zip")

# UMM
Remove-Item -Force -Path ($copy) -Recurse -ErrorAction SilentlyContinue

$base = $copy + "DriverAssist\"
Remove-Item -Force -Path ($copy) -Recurse -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path ($base)

Copy-Item -Path ($UMMdir + "DriverAssist.dll") -Destination ($base) -Recurse -Force
# Copy-Item -Path ($Coredir + "DriverAssistUMM.dll") -Destination ($copy) -Recurse -Force

$ver = (Select-String -Pattern '<Version>([0-9]+\.[0-9]+\.[0-9]+)</Version>' -Path Directory.Build.props).Matches.Groups[1]
# $info = Get-Content ("Info.json") -raw | ConvertFrom-Json
# $info.Version = $ver
# $info | ConvertTo-Json | Set-Content ("Info.json")
Copy-Item -Path ("Info.json") -Destination ($base) -Recurse -Force

Compress-Archive -Path ($copy + "*") -Force -CompressionLevel "Optimal" -DestinationPath ($out + "DriverAssist_v" + $ver + ".zip")

#Remove-Item -Force -Path ($copy) -Recurse -ErrorAction SilentlyContinue