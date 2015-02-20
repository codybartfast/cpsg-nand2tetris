@echo off
SET project=%~dp0
SET assemble=%project%Assemble\bin\debug\assemble.exe
SET source=%project%%1
%assemble% %source%
