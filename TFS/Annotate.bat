@echo off
%~d1
cd %~p1
@echo on
tf annotate %1
@echo off
SET /P variable="Hit Enter to exit."