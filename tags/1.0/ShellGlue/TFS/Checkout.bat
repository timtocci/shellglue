@echo off
%~d1
cd %~p1
@echo on
tf checkout %1 %2 %3 %4 %5 %6 %7 %8 %9 /recursive
@echo off
SET /P variable="Hit Enter to exit."