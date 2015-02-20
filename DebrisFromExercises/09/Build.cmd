@echo off
echo.
echo.
if ""=="%1" goto End

set baseDir=%~dp0
set jackDir=%baseDir%%1
set osDir=%baseDir%OS
set osPlusDir=%baseDir%OSPlus

if not exist %jackDir% goto End
Echo Starting...

del %osPlusDir%\*.vm
xcopy %osDir%\*.vm %osPlusDir%\ > nul
JackCompiler.lnk %jackDir%
xcopy %jackDir%\*.vm %osPlusDir%\ > nul
Echo ...Done
echo.
:End 