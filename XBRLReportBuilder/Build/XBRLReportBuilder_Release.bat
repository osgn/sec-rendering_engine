@echo off

del ..\..\BuildLog.txt

IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\Tools\vsvars32.bat" (
	CALL "C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\Tools\vsvars32.bat"
	SET compiler=devenv
) ELSE IF EXIST "C:\Program Files\Microsoft Visual Studio 9.0\Common7\Tools\vsvars32.bat" (
	CALL "C:\Program Files\Microsoft Visual Studio 9.0\Common7\Tools\vsvars32.bat"
	SET compiler=devenv
) ELSE IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\Tools\vsvars32.bat" (
	CALL "C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\Tools\vsvars32.bat"
	SET compiler=devenv
) ELSE IF EXIST "C:\Program Files\Microsoft Visual Studio 8\Common7\Tools\vsvars32.bat" (
	Call "C:\Program Files\Microsoft Visual Studio 8\Common7\Tools\vsvars32.bat"
	SET compiler=devenv
) ELSE IF EXIST "C:\Program Files\Microsoft Visual Studio 10.0\Common7\IDE\VCSExpress.exe" (
	SET compiler="C:\Program Files\Microsoft Visual Studio 10.0\Common7\IDE\VCSExpress.exe"
) ELSE IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\VCSExpress.exe" (
	SET compiler="C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\VCSExpress.exe"
) ELSE (
	ECHO "This build cannot continue. Press any key to exit."
	PAUSE
	GOTO :END
)

ECHO. >> ..\..\BuildLog.txt
ECHO. >> ..\..\BuildLog.txt
ECHO. >> ..\..\BuildLog.txt
ECHO ********Building Common******** >> ..\..\BuildLog.txt
%compiler% /out ..\..\BuildLog.txt /rebuild Release "..\..\Common\Common.sln"

ECHO. >> ..\..\BuildLog.txt
ECHO. >> ..\..\BuildLog.txt
ECHO. >> ..\..\BuildLog.txt
ECHO ********Building NXBRE******** >> ..\..\BuildLog.txt
%compiler% /out ..\..\BuildLog.txt /rebuild Release "..\..\XBRLReportBuilder\NxBRE\NxBRE.sln"

ECHO. >> ..\..\BuildLog.txt
ECHO. >> ..\..\BuildLog.txt
ECHO. >> ..\..\BuildLog.txt
ECHO ********Building FilingServices******** >> ..\..\BuildLog.txt
%compiler% /out ..\..\BuildLog.txt /rebuild Release "..\..\XBRLReportBuilder\FilingServices\FilingServices.sln"

ECHO. >> ..\..\BuildLog.txt
ECHO. >> ..\..\BuildLog.txt
ECHO. >> ..\..\BuildLog.txt
ECHO ********Building XBRLReportBuilder******** >> ..\..\BuildLog.txt
%compiler% /out ..\..\BuildLog.txt /rebuild Release "..\..\XBRLReportBuilder\XBRLReportBuilder\XBRLReportBuilder.sln"

del ..\..\*.projdata /S /A:H /F /Q >> S:\BuildLog.txt

start ..\..\BuildLog.txt

rem pause
