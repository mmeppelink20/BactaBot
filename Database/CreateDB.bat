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

REM create DB
echo Executing CreateDB.sql
%SqlCmdPath% -S %ServerName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i "CreateDB.sql" >> DB.log 2>&1
call :CheckErrors "CreateDB.sql"

echo  **************************
echo     Creating %DatabaseName% Tables.
echo  **************************

REM Execute SQL scripts
echo Executing Guilds.sql
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\Tables\Guilds.sql" >> DB.log 2>&1
call :CheckErrors "Guilds.sql"
echo Executing ChannelTypes.sql
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\Tables\ChannelTypes.sql" >> DB.log 2>&1
call :CheckErrors "ChannelTypes.sql"
echo Executing Channels.sql
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\Tables\Channels.sql" >> DB.log 2>&1
call :CheckErrors "Channels.sql"
echo Executing Users.sql
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\Tables\Users.sql" >> DB.log 2>&1
call :CheckErrors "Users.sql"
echo Executing UserConfiguration.sql
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\Tables\UserConfiguration.sql" >> DB.log 2>&1
call :CheckErrors "UserConfiguration.sql"
echo Executing DiscordMessages.sql
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\Tables\DiscordMessages.sql" >> DB.log 2>&1
call :CheckErrors "DiscordMessages.sql"
echo Executing Configuration.sql
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\Tables\Configuration.sql" >> DB.log 2>&1
call :CheckErrors "Configuration.sql"
echo Executing UserStats.sql
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\Tables\UserStats.sql" >> DB.log 2>&1
call :CheckErrors "UserStats.sql"
echo Executing GuildUsers.sql
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\Tables\GuildUsers.sql" >> DB.log 2>&1
call :CheckErrors "GuildUsers.sql"
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\Tables\EditedMessages.sql" >> DB.log 2>&1
call :CheckErrors "EditedMessages.sql"

echo.

echo  **************************
echo     Creating %DatabaseName% Types.
echo  **************************

REM Execute all type definitions BEFORE stored procedures
set TypesPath=.\Types
for %%F in (%TypesPath%\*.sql) do (
    echo Executing "%%F"
    %SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i "%%F" >> DB.log 2>&1
    call :CheckErrors "%%F"
)

echo.

echo  **************************
echo     Creating %DatabaseName% Stored Procedures.
echo  **************************

REM Execute all stored procedures AFTER types
set StoredProcPath=.\StoredProcedures
for %%F in (%StoredProcPath%\*.sql) do (
    echo Executing "%%F"
    %SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i "%%F" >> DB.log 2>&1
    call :CheckErrors "%%F"
)

echo.

echo  **************************
echo     Inserting sample data into %DatabaseName%
echo  **************************

REM Execute the sample data script
echo Executing SampleData.sql
%SqlCmdPath% -S %ServerName% -d %DatabaseName% -U %SQLUSERNAME% -P %SQLPASSWORD% -i ".\SampleData\SampleData.sql" >> DB.log 2>&1
call :CheckErrors "SampleData.sql"

echo.

pause
exit /b

:CheckErrors
REM Check for errors in the log file and include the specific script name
set ScriptName=%1
findstr /i "error" DB.log >nul
if %errorlevel% equ 0 (
    echo Errors detected during execution of "%ScriptName". Opening log file...
    notepad DB.log
    exit /b 1
)
exit /b 0
