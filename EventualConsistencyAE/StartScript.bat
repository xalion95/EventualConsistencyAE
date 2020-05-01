@echo off

IF "%1"=="" ( SET /A start = 60000) ELSE (SET /A start = %1)
IF "%2"=="" ( SET /A end   = 60002) ELSE (SET /A end   = %2)

SET /A diff = %end% - %start%

IF %diff% gtr 10 goto :cond
IF %diff% lss  0 goto :cond

goto :skip

:cond
SET /A end = %start% + 2
echo Zbyt du¾y przedziaˆ port¢w! 
echo Ustawiam przedziaˆ port¢w: %start% - %end%

:skip


FOR /l %%x IN (%start%, 1, %end%) DO start "" "bin\Debug\EventualConsistencyAE.exe" %%x %start% %end%
