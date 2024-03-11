using System.Diagnostics;

internal static class Helpers {

    public static async Task RunCommandInConsoleAndWait ( string targetFolder, string command ) {
        var process = Process.Start (
            new ProcessStartInfo {
                WorkingDirectory = targetFolder,
                FileName = command,
            }
        );
        if ( process == null ) HandleError ( "Can't run command: " + command );

        await process!.WaitForExitAsync ();
    }

    public static void RunCommandInConsole ( string command, string targetFolder = "" ) {
        var process = Process.Start (
            new ProcessStartInfo {
                WorkingDirectory = targetFolder,
                FileName = command,
            }
        );
        if ( process == null ) HandleError ( "Can't run command: " + command );
    }

    public static void HandleError ( string message ) {
        Console.WriteLine ( message );
        Console.WriteLine ( "Пожалуйста сообщите разработчику об ошибке в группу https://t.me/+Le_oNL4Tw745YWUy прислав скриншот этого экрана" );
        Console.ReadKey ();
        Environment.Exit ( 100 );
    }

}
