@echo off
%~d1
cd %~p1
@echo on
tf rollback
@echo off
SET /P variable="Hit Enter to exit."