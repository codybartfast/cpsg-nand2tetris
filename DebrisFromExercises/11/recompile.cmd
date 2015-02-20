@echo off
set dir=%~dp0
set exe=%dir%\..\10\JackCompiler\bin\debug\JackCompiler.exe
for %%d in (average, complexArrays, convertToBin, Pong, Seven, Square) do cls & %exe% %%d
pause