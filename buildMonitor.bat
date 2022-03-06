@echo off
setlocal DisableDelayedExpansion

for /F "tokens=*" %%a in ('findstr /n $') do (
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