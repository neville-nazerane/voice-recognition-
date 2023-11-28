@echo off

SET IP=%1
SET USERNAME=%2
SET DEST_ROOT=%3
SET PROJECTNAME=%4

echo IP: %IP%
echo Username: %USERNAME%
echo Destination Root: %DEST_ROOT%
echo Project Name: %PROJECTNAME%

SET TEMPDIR=TempOutput

dotnet publish ../src/"%PROJECTNAME%" -o "%TEMPDIR%"

echo dotnet "%PROJECTNAME%.dll" > "%TEMPDIR%\run.sh"

echo rm -rf %DEST_ROOT%/%PROJECTNAME% > tempCommand.sh
echo mkdir -p %DEST_ROOT%/%PROJECTNAME% >> tempCommand.sh

plink %USERNAME%@%IP% -batch -m tempCommand.sh

@REM pscp -r "%TEMPDIR%\*" %USERNAME%@%IP%:%DEST_ROOT%/%PROJECTNAME%
scp -r "%TEMPDIR%\*" %USERNAME%@%IP%:%DEST_ROOT%/%PROJECTNAME%/


rmdir /s /q "%TEMPDIR%"

del tempCommand.sh

echo Done
