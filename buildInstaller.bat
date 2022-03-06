rem echo off
set devenvPath=%1
set slnPath=%2
set projPath=%3
set config=%4
%devenvPath% %slnPath% /Project %projPath% /Rebuild %config%