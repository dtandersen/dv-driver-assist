@REM You can listen to 91X on the boombox.

@REM C:\Program Files (x86)\Steam\steamapps\common\Derail Valley\DerailValley_Data\StreamingAssets\music\Radio.pls
@REM [playlist]
@REM File3=http://localhost:3690/radio.ogg
@REM Title3=91X

:start
ffmpeg -i http://14613.live.streamtheworld.com:3690/XTRAFMAAC_SC -filter:a "volume=5dB" -listen 1 -f ogg http://localhost:3690/radio.ogg
goto start
