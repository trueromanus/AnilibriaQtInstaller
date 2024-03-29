﻿using Installer;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Json;
using static Helpers;

const string MetadataFileName = "aniqtmetadata";

if ( args.Length != 0 ) {
    var operation = args[0];
    switch ( operation ) {
        case "clearmeta":
            var clearMetadataFilePath = Path.Combine ( Environment.GetFolderPath ( Environment.SpecialFolder.LocalApplicationData ), MetadataFileName );
            File.Delete ( clearMetadataFilePath );
            Environment.Exit ( 0 );
            break;
    }
}

Console.WriteLine ( "AnilibriaQtInstaller version 0.0.1 started" );

var httpClient = new HttpClient ();
httpClient.DefaultRequestHeaders.Add ( "User-Agent", "Anilibria Installer" );
var data = await httpClient.GetStringAsync ( "https://api.github.com/repos/anilibria/anilibria-winmaclinux/releases/latest" );
var latestRelease = JsonSerializer.Deserialize ( data, typeof ( VersionResponse ), ResponseContext.Default ) as VersionResponse;
if ( latestRelease == null ) {
    HandleError ( $"Can't parsing response from GitHub" );
}

var folder = Environment.GetFolderPath ( Environment.SpecialFolder.LocalApplicationData );
var metadataFilePath = Path.Combine ( folder, MetadataFileName );
var installedVersion = "";
if ( File.Exists ( metadataFilePath ) ) installedVersion = File.ReadAllText ( metadataFilePath );

if ( installedVersion == latestRelease!.TagName ) {
    Console.WriteLine ( $"Installed latest version {installedVersion}, not need to action" );
    RunAnilibriaApplication ( installedVersion );
    return;
}

Console.WriteLine ( $"Start to install version {latestRelease.TagName}" );

var achiveFileName = "";
VersionAsset? downloadAsset = null;

if ( OperatingSystem.IsMacOS () ) {
    downloadAsset = latestRelease.Assets.FirstOrDefault ( a => a.Name.Contains ( "macos" ) );

    achiveFileName = latestRelease.TagName + ".dmg";
}
if ( OperatingSystem.IsLinux () ) {
    var isArm64 = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;
    downloadAsset = latestRelease.Assets.FirstOrDefault ( a => a.Name.Contains ( "flatpak" ) && a.Name.Contains ( isArm64 ? "aarch64" : "x86_64" ) );

    achiveFileName = latestRelease.TagName + ".flatpak";
}
if ( OperatingSystem.IsWindows () ) {
    downloadAsset = latestRelease.Assets.FirstOrDefault ( a => a.Name.Contains ( "windows" ) );

    achiveFileName = latestRelease.TagName + ".zip";
}

if ( downloadAsset == null ) HandleError ( $"Can't find asset for download!" );

Console.WriteLine ( $"Start download archive/Скачивание архива пожалуйста подождите" );

try {
    var assetResponse = await httpClient.GetAsync ( downloadAsset!.BrowserDownloadUrl );
    var stream = await assetResponse.Content.ReadAsStreamAsync ();
    var archiveFile = File.OpenWrite ( achiveFileName );
    await stream.CopyToAsync ( archiveFile );
    archiveFile.Close ();
} catch ( Exception ex ) {
    HandleError ( $"Error while downloading version {ex.Message}" );
}

var targetDirectory = Path.Combine ( latestRelease.TagName + "/" );

if ( OperatingSystem.IsWindows () ) {
    Console.WriteLine ( $"Creating directory for new version" );
    try {
        if ( !Directory.Exists ( targetDirectory ) ) Directory.CreateDirectory ( targetDirectory );
    } catch ( Exception ex ) {
        HandleError ( $"Error while creating directory {targetDirectory}: {ex.Message}" );
    }

    Console.WriteLine ( $"Extract archive to new version directory" );
    try {
        using var target = File.OpenRead ( achiveFileName );
        using var newArchive = new ZipArchive ( target, ZipArchiveMode.Read );
        newArchive.ExtractToDirectory ( targetDirectory, overwriteFiles: true );
    } catch ( Exception ex ) {
        HandleError ( $"Error while extracting achive {targetDirectory}: {ex.Message}" );
    }
}
if ( OperatingSystem.IsMacOS () ) {
    Console.WriteLine ( $"Mount DMG file as virtual disk" );
    await RunCommandInConsoleAndWait ( "xattr", $"-d com.apple.quarantine {achiveFileName}" );
    //unmount curren virtual disk if it was mounted
    var listOutput = await RunCommandInConsoleAndWaitOutput ( "diskutil", "list" );
    var mountedDiskLine = listOutput.FirstOrDefault ( a => a.Contains ( "AniLibria" ) );
    if ( mountedDiskLine != null ) {
        mountedDiskLine = mountedDiskLine.Substring ( mountedDiskLine.LastIndexOf ( " " ) ).Trim ();
        await RunCommandInConsoleAndWait ( "hdiutil", $"detach {mountedDiskLine}" );
    }
    await RunCommandInConsoleAndWait ( "hdiutil", $"attach {achiveFileName}" );
}
if ( OperatingSystem.IsLinux () ) await RunCommandInConsoleAndWait ( "flatpak", $"install --user {achiveFileName}" );

if ( OperatingSystem.IsWindows () || OperatingSystem.IsLinux () ) {
    try {
        File.Delete ( achiveFileName );
    } catch ( Exception ex ) {
        HandleError ( $"Error while delete downloaded achive {achiveFileName}: {ex.Message}" );
    }
}

try {
    File.WriteAllText ( metadataFilePath, latestRelease.TagName );
} catch ( Exception ex ) {
    HandleError ( $"Error while write metadata file {metadataFilePath}: {ex.Message}" );
}

RunAnilibriaApplication ( latestRelease.TagName );

Console.WriteLine ( "Installer completed sucessfully" );

static void RunAnilibriaApplication ( string targetFolder ) {
    if ( OperatingSystem.IsWindows () ) {
        var executableFile = Path.Combine ( targetFolder, "AniLibria.exe" );
        if ( !File.Exists ( executableFile ) ) HandleError ( $"Файл AniLibria.exe с указанной выше версией не найден на диске! Он должен быть в папке {Path.GetFullPath ( targetFolder )}" );

        Process.Start (
            new ProcessStartInfo {
                WorkingDirectory = targetFolder,
                FileName = Path.Combine ( targetFolder, "AniLibria.exe" ),
            }
        );
    }
    if ( OperatingSystem.IsLinux () ) RunCommandInConsole ( "flatpak", "run tv.anilibria.anilibria" );
    if ( OperatingSystem.IsMacOS () ) RunCommandInConsole ( "/Volumes/AniLibria/AniLibria.app/Contents/MacOS/Anilibria", "" );
}