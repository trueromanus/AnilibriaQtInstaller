Expand-Archive -Path $env:USERPROFILE\Downloads\windows64.zip
Expand-Archive -Path $env:USERPROFILE\Downloads\windowsarm64.zip
Expand-Archive -Path $env:USERPROFILE\Downloads\macos64.zip
Expand-Archive -Path $env:USERPROFILE\Downloads\macosarm64.zip
Expand-Archive -Path $env:USERPROFILE\Downloads\linux64.zip
Expand-Archive -Path $env:USERPROFILE\Downloads\linuxarm64.zip

New-Item -Name "deploy" -ItemType "directory"

Copy-Item -Path windows64/AniLibriaQtLauncher.exe -Destination deploy/windows64.exe
Copy-Item -Path windowsarm64/AniLibriaQtLauncher.exe -Destination deploy/windowsarm64.exe
Copy-Item -Path linux64/AniLibriaQtLauncher -Destination deploy/linux64
Copy-Item -Path linuxarm64/AniLibriaQtLauncher -Destination deploy/linuxarm64
Copy-Item -Path macos64/AniLibriaQtLauncher -Destination deploy/macos64
Copy-Item -Path macosarm64/AniLibriaQtLauncher -Destination deploy/macosarm64

Remove-Item -Path windows64 -Recurse -Force
Remove-Item -Path windowsarm64 -Recurse -Force
Remove-Item -Path linux64 -Recurse -Force
Remove-Item -Path linuxarm64 -Recurse -Force
Remove-Item -Path macos64 -Recurse -Force
Remove-Item -Path macosarm64 -Recurse -Force