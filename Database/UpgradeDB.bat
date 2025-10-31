@echo off

REM Load database credentials from .env file
for /f "tokens=1,2 delims==" %%A in (.env) do (
    set %%A=%%B
)

REM Map loaded environment variables to corresponding placeholders
set ServerName=%DB_SERVER%
set DatabaseName=%DB_NAME%
set SQLUSERNAME=%DB_USER%
set SQLPASSWORD=%DB_PASSWORD%

REM Set path to sqlcmd executable
set SqlCmdPath="C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe"

echo  **************************
echo     Upgrading %DatabaseName% Database
echo  **************************

REM Execute upgrade scripts for table changes first
echo Executing DiscordMessage table upgrades
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\Tables\TableUpgradeScripts\DiscordMessage.sql" >> DBUpgrade.log 2>&1
call :CheckErrors "DiscordMessage.sql"

echo.

echo  **************************
echo     Upgrading %DatabaseName% Stored Procedures
echo  **************************

REM Execute specific upgrade scripts in order
echo Executing sp_delete_discord_message
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\StoredProcedures\StoredProcedureUpgradeScripts\sp_delete_discord_message.sql" >> DBUpgrade.log 2>&1
call :CheckErrors "sp_delete_discord_message.sql"

echo Executing sp_get_recent_discord_messages
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\StoredProcedures\StoredProcedureUpgradeScripts\sp_get_recent_discord_messages.sql" >> DBUpgrade.log 2>&1
call :CheckErrors "sp_get_recent_discord_messages.sql"

echo Executing sp_insert_discord_message
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\StoredProcedures\StoredProcedureUpgradeScripts\sp_insert_discord_message.sql" >> DBUpgrade.log 2>&1
call :CheckErrors "sp_insert_discord_message.sql"

REM Execute all remaining stored procedure upgrade scripts
set StoredProcUpgradePath=.\StoredProcedures\StoredProcedureUpgradeScripts
for %%F in (%StoredProcUpgradePath%\*.sql) do (
    set FileName=%%~nxF
    if not "%%~nxF"=="sp_delete_discord_message.sql" (
        if not "%%~nxF"=="sp_get_recent_discord_messages.sql" (
            if not "%%~nxF"=="sp_insert_discord_message.sql" (
                echo Executing "%%F"
                %SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i "%%F" >> DBUpgrade.log 2>&1
                call :CheckErrors "%%F"
            )
        )
    )
)

echo.

echo  **************************
echo     %DatabaseName% upgrade completed successfully!
echo  **************************

pause
exit /b

:CheckErrors
REM Check for errors in the log file and include the specific script name
set ScriptName=%1
findstr /i "error" DBUpgrade.log >nul
if %errorlevel% equ 0 (
    echo Errors detected during execution of "%ScriptName%". Opening log file...
    notepad DBUpgrade.log
    exit /b 1
)
exit /b 0