@echo off
set devenvPath=%1
set slnPath=%2
set projPath=%3
set config=%4
set command=%devenvPath% %slnPath% /Project %projPath% /Rebuild %config%
echo %command%
setlocal DisableDelayedExpansion
FOR /F "tokens=*" %%a in ('%command% ^|findstr /n $') do (
  set "line=%%a"
  setlocal EnableDelayedExpansion
  set "line=!line:*:=!"
  echo(!line!
  echo %%(!line!|find "2 succeeded" >nul
  if NOT errorlevel 1 (
	  exit 0
  )
  endlocal
)
exit 0