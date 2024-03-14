using System.Diagnostics;

internal static class Helpers {

    public static async Task RunCommandInConsoleAndWait ( string command, string arguments, string targetFolder = "" ) {
        var process = Process.Start (
            new ProcessStartInfo {
                WorkingDirectory = targetFolder,
                FileName = command,
                Arguments = arguments,
            }
        );
        if ( process == null ) HandleError ( "Can't run command: " + command );

        //process.OutputDataReceived

        await process!.WaitForExitAsync ();
    }

    public static async Task<List<string>> RunCommandInConsoleAndWaitOutput ( string command, string arguments, string targetFolder = "" ) {
        var process = Process.Start (
            new ProcessStartInfo {
                WorkingDirectory = targetFolder,
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true
            }
        );
        if ( process == null ) HandleError ( "Can't run command: " + command );

        var result = new List<string> ();
        if ( process != null && process.StandardOutput != null ) {
            while ( !process.StandardOutput.EndOfStream ) {
                string? line = process.StandardOutput.ReadLine ();
                if ( line == null ) continue;

                result.Add ( line );
            }

            await process!.WaitForExitAsync ();
        }

        return result;
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
