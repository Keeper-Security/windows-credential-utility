using System.CommandLine;

namespace WinCredentialManager
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var initCommand = new Command(
                name: "init",
                description: "Add a KSM Config to Windows Credential Manager."
            );

            var getCommand = new Command(
                name: "get",
                description: "Get a KSM Config from Windows Credential Manager."
            );

            var appName = new Argument<string>(
                name: "name",
                description: "The name of the application for the stored config."
            );

            var configArg = new Argument<string>(
                name: "config",
                description: "The KSM Configuration to be added to the Windows Credential Manager. Accepts BASE64 string, JSON string, or a path to a JSON configuration."
            );

            var rootCommand = new RootCommand("KSM Windows Credential Manager");

            rootCommand.AddCommand(initCommand);
            rootCommand.AddCommand(getCommand);

            initCommand.AddArgument(appName);
            initCommand.AddArgument(configArg);
            initCommand.SetHandler((appName, configArg) =>
            {
                CredentialManager.WriteCredential(appName, Environment.UserName, Parsing.ParseConfig(configArg));
                // Exit with success code
                Environment.Exit(0);
            }, appName, configArg);

            getCommand.AddArgument(appName);
            getCommand.SetHandler((appName) =>
            {
                var cred = CredentialManager.ReadCredential(appName);
                // Output cred to stdout and exit with success code 
                if (cred == null)
                {
                    Console.WriteLine("No KSM Config found for the given application name.");
                    Environment.Exit(1);
                }
                Console.WriteLine(cred.Password);
                Environment.Exit(0);
            }, appName);

            return await rootCommand.InvokeAsync(args);
        }
    }
}