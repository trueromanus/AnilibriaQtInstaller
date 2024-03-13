Expand-Archive -Path $env:USERPROFILE\Downloads\macos64.zip
Expand-Archive -Path $env:USERPROFILE\Downloads\macosarm64.zip
Expand-Archive -Path $env:USERPROFILE\Downloads\linux64.zip
Expand-Archive -Path $env:USERPROFILE\Downloads\linuxarm64.zip

&dotnet "publish -r win-x64 -c Release --self-contained true src/Installer.csproj"
&dotnet "publish -r win-arm64 -c Release --self-contained true src/Installer.csproj"

New-Item -Name "deploy" -ItemType "directory"

Copy-Item -Path src/bin/Release/net8.0/win-x64/native/AniLibriaQtLauncher.exe -Destination deploy/windows64.exe
Copy-Item -Path src/bin/Release/net8.0/win-arm64/native/AniLibriaQtLauncher.exe -Destination deploy/windowsarm64.exe
Copy-Item -Path linux64/AniLibriaQtLauncher -Destination deploy/linux64
Copy-Item -Path linuxarm64/AniLibriaQtLauncher -Destination deploy/linuxarm64
Copy-Item -Path macos64/AniLibriaQtLauncher -Destination deploy/macos64
Copy-Item -Path macosarm64/AniLibriaQtLauncher -Destination deploy/macosarm64

Remove-Item -Path linux64 -Recurse -Force
Remove-Item -Path linuxarm64 -Recurse -Force
Remove-Item -Path macos64 -Recurse -Force
Remove-Item -Path macosarm64 -Recurse -Force