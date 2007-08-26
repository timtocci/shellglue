@echo off
set PATH=%PATH%;%windir%\Microsoft.NET\Framework\v2.0.50727
@echo on

FOR /F "tokens=1 delims=" %%i in (%~fs1) do msbuild "%%i"

@echo off
SET /P variable="Hit Enter to exit."