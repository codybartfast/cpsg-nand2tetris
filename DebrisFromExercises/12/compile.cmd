@echo off
set dir=%1
if "%1" == "" set dir=ScreenTest

call C:\STUFF\Nand2Tetris\Suite\tools\jackCompiler.bat %~dp0\%dir%
cd %~dp0
