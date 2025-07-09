@echo off
setlocal
set "db=database.sqlite"
set "cxb=gamesettings_c1380_d873_s6285.cxb"
set "debug86=ACB RDV\bin\x86\Debug"
set "release86=ACB RDV\bin\x86\Release"
set "debug64=ACB RDV\bin\x64\Debug"
set "release64=ACB RDV\bin\x64\Release"

where sqlite3 >nul 2>&1
IF ERRORLEVEL 1 (
    echo sqlite3 is not installed or not in PATH.
    echo Please install SQLite from https://sqlite.org and ensure it's in your PATH.
    exit /b 1
)

IF EXIST "%debug64%" (
    sqlite3 "%debug64%\%db%" < db_init.sql
	IF EXIST "%debug64%\%db%" (
		echo %debug64%\%db%
	) ELSE (
		echo [ERROR] Failed to create "%debug64%\%db%"
	)
)

IF EXIST "%release64%" (
    copy "%debug64%\%db%" "%release64%\%db%" >nul
	IF EXIST "%release64%\%db%" (
		echo %release64%\%db%
	) ELSE (
		echo [ERROR] Failed to create "%release64%\%db%"
	)
)

IF EXIST "%debug86%" (
    copy "%debug64%\%db%" "%debug86%\%db%" >nul
	IF EXIST "%debug86%\%db%" (
		echo %debug86%\%db%
	) ELSE (
		echo [ERROR] Failed to create "%debug86%\%db%"
	)
)

IF EXIST "%release86%" (
    copy "%debug64%\%db%" "%release86%\%db%" >nul
	IF EXIST "%release86%\%db%" (
		echo %release86%\%db%
	) ELSE (
		echo [ERROR] Failed to create "%release86%\%db%"
	)
)

IF EXIST "%release86%\%cxb%" (
	IF EXIST "%debug86%" (
		copy "%release86%\%cxb%" "%debug86%\%cxb%" >nul
		IF EXIST "%debug86%\%cxb%" (
			echo %debug86%\%cxb%
		) ELSE (
			echo [ERROR] Failed to create "%debug86%\%cxb%"
		)
	)
	
	IF EXIST "%debug64%" (
		copy "%release86%\%cxb%" "%debug64%\%cxb%" >nul
		IF EXIST "%debug64%\%cxb%" (
			echo %debug64%\%cxb%
		) ELSE (
			echo [ERROR] Failed to create "%debug64%\%cxb%"
		)
	)
	
	IF EXIST "%release64%" (
		copy "%release86%\%cxb%" "%release64%\%cxb%" >nul
		IF EXIST "%release64%\%cxb%" (
			echo %release64%\%cxb%
		) ELSE (
			echo [ERROR] Failed to create "%release64%\%cxb%"
		)
	)
) ELSE (
	echo [ERROR] Missing %release86%\%cxb%
)
