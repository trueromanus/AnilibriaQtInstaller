using Installer;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

const string MetadataFileName = "aniqtmetadata";

if ( args.Length != 0 ) {
    var operation = args[0];
    switch ( operation ) {
        case "clearmeta":
            var clearMetadataFilePath = Path.Combine ( Environment.GetFolderPath ( Environment.SpecialFolder.LocalApplicationData ), MetadataFileName );
            File.Delete ( clearMetadataFilePath );
            break;
    }
}

Console.WriteLine ( "Installer started" );

try {
    foreach ( var process in Process.GetProcessesByName ( "AniLibria" ) ) {
        process.Kill ();
    }
} catch ( Exception ex ) {
    HandleError ( $"Can't kill instances of application: {ex.Message}" );
}

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

var windowsAsset = latestRelease.Assets.FirstOrDefault ( a => a.Name.Contains ( "windows" ) );
if ( windowsAsset == null ) {
    HandleError ( $"Can't find window asser for download!" );
}

var achiveFileName = latestRelease.TagName + ".zip";

Console.WriteLine ( $"Start download archive/Скачивание архива пожалуйста подождите" );

try {
    var assetResponse = await httpClient.GetAsync ( windowsAsset!.BrowserDownloadUrl );
    var stream = await assetResponse.Content.ReadAsStreamAsync ();
    var archiveFile = File.OpenWrite ( achiveFileName );
    await stream.CopyToAsync ( archiveFile );
    archiveFile.Close ();
} catch ( Exception ex ) {
    HandleError ( $"Error while downloading archive {ex.Message}" );
}

var targetDirectory = Path.Combine ( latestRelease.TagName + "/" );

Console.WriteLine ( $"Create directory for new version" );

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

try {
    File.Delete ( achiveFileName );
} catch ( Exception ex ) {
    HandleError ( $"Error while delete downloaded achive {achiveFileName}: {ex.Message}" );
}

try {
    File.WriteAllText ( metadataFilePath, latestRelease.TagName );
} catch ( Exception ex ) {
    HandleError ( $"Error while write metadata file {metadataFilePath}: {ex.Message}" );
}

RunAnilibriaApplication ( latestRelease.TagName );

Console.WriteLine ( "Installer completed sucessfully" );

void RunAnilibriaApplication ( string targetFolder ) {
    var executableFile = Path.Combine ( targetFolder, "AniLibria.exe" );
    if ( !File.Exists ( executableFile ) ) HandleError ( $"Файл AniLibria.exe с указанной выше версией не найден на диске! Он должен быть в папке {Path.GetFullPath(targetFolder)}" );

    Process.Start (
        new ProcessStartInfo {
            WorkingDirectory = targetFolder,
            FileName = Path.Combine ( targetFolder, "AniLibria.exe" ),
        }
    );
}

void HandleError ( string message ) {
    Console.WriteLine ( message );
    Console.WriteLine ( "Пожалуйста сообщите разработчику об ошибке в группу https://t.me/+Le_oNL4Tw745YWUy прислав скриншот этого экрана" );
    Console.ReadKey ();
    Environment.Exit ( 100 );
}


