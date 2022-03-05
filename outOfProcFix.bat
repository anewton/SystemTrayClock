set vsIDEPath=%1
SET current_path="%CD%"
ECHO Setting the current path to the DisableOutOfProcBuild.exe installation folder.
CD "%vsIDEPath%\CommonExtensions\Microsoft\VSI\DisableOutOfProcBuild"
ECHO:
START /d DisableOutOfProcBuild.exe
ECHO: & ECHO Revert the previous current directory.
CD %current_path%
ECHO CD is set to %CD%