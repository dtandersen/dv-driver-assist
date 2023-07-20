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

# Remove-Item -Force -Path ($copy) -Recurse -ErrorAction SilentlyContinue
# New-Item -ItemType Directory -Force -Path ($copy)
# Copy-Item -Path ($BIEdir) -Destination ($copy) -Recurse -Force
# Copy-Item -Path ($Coredir+"*") -Destination ($copy + "BepInEx\plugins\RuntimeUnityEditor") -Recurse -Force
# Compress-Archive -Path ($copy + "BepInEx") -Force -CompressionLevel "Optimal" -DestinationPath ($out + "RuntimeUnityEditor_BepInEx5_v" + $ver + ".zip")

Remove-Item -Force -Path ($copy) -Recurse -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path ($copy)
Copy-Item -Path ("Info.json") -Destination ($copy) -Recurse -Force


Copy-Item -Path ($UMMdir + "DriverAssist.dll") -Destination ($copy) -Recurse -Force
Copy-Item -Path ($Coredir + "DriverAssistUMM.dll") -Destination ($copy) -Recurse -Force
$info = Get-Content ($copy + "Info.json") -raw | ConvertFrom-Json
$info.Version = $ver
$info | ConvertTo-Json | Set-Content ($copy + "Info.json")

Compress-Archive -Path ($copy + "*") -Force -CompressionLevel "Optimal" -DestinationPath ($out + "DriverAssist_UMM_v" + $ver + ".zip")

Remove-Item -Force -Path ($copy) -Recurse -ErrorAction SilentlyContinue