@echo off
set devenvPath=%1
set slnPath=%2
set projPath=%3
set config=%4
setlocal DisableDelayedExpansion
FOR /F "tokens=*" %%a in ('%devenvPath% %slnPath% /Project %projPath% /Rebuild %config% ^|findstr /n $') do (
  set "line=%%a"
  setlocal EnableDelayedExpansion
  set "line=!line:*:=!"
  echo(!line!
  echo %%(!line!|find "2 succeeded, 0 failed, 0 skipped" >nul
  if NOT errorlevel 1 (
	  taskkill /f /im "devenv.com"
	  exit 0
  )
  endlocal
)