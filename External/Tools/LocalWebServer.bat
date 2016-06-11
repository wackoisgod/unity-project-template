@echo off

SET folder=%~1
IF %folder:~-1%==\ SET folder=%folder:~0,-1%

SET /a port=1234

start "IIS Express :%port%" cmd /C " "%ProgramFiles%\IIS Express\iisexpress.exe" /path:"%folder%" /port:%port% /systray:true "
