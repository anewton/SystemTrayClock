rem echo off
set devenvPath=%1
set slnPath=%2
set projPath=%3
set config=%4
%devenvPath% %slnPath% /Project %projPath% /Rebuild %config% > log.txt
more log.txt
find "========== Rebuild All: 2 succeeded, 0 failed, 0 skipped ==========" log.txt && (
  echo "Success"
  exit 0
) || (
  echo "Error"
  exit 1
)