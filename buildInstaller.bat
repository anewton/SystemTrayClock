set devenvPath=%1
set slnPath=%2
set projPath=%3
set config=%4
%devenvPath% %slnPath% /Project %projPath% /Build %config% /Out "installer.log" > nul if ERRORLEVEL 1 echo Error %ERRORLEVEL% else echo 0