mkdir converted
FOR %%F in (*.iso) do (
extract-xiso -r "%%F"
echo %%F>>rebuild.log
if exist ".\%%~nF.iso" (del ".\%%~nF.old")
move "%%F" ".\converted\%%~nF.iso"
)
pause