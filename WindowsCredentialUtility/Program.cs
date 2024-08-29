using System.CommandLine;

namespace WindowsCredentialUtility;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var setCommand = new Command(
            name: "set",
            description: "Store credential data in the Windows Credential Manager."
        );

        var getCommand = new Command(
            name: "get",
            description: "Get credential data from Windows Credential Manager."
        );

        var appName = new Argument<string>(
            name: "name",
            description: "The name of the application storing the credential data."
        );

        var credentialData = new Argument<string>(
            name: "data",
            description: "The credential data as a string."
        );

        var rootCommand = new RootCommand("Windows Credential Utility");

        rootCommand.AddCommand(setCommand);
        rootCommand.AddCommand(getCommand);

        setCommand.AddArgument(appName);
        setCommand.AddArgument(credentialData);
        setCommand.SetHandler((appName, credentialData) =>
        {
            try
            {
                var secret = Parsing.ParseConfig(credentialData);
                CredentialManager.WriteCredential(appName, Environment.UserName, secret);
                // Exit with success code
                Environment.Exit(0);
            }
            catch (ArgumentException e)
            {
                Console.Error.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }, appName, credentialData);

        getCommand.AddArgument(appName);
        getCommand.SetHandler((appName) =>
        {
            var cred = CredentialManager.ReadCredential(appName);
            // Output cred to stdout and exit with success code 
            if (cred == null)
            {
                Console.Error.WriteLine("The credential data is neither base64 nor JSON.");
                Environment.Exit(1);
            }
            Console.WriteLine(cred.Password);
            Environment.Exit(0);
        }, appName);

        return await rootCommand.InvokeAsync(args);
    }
}