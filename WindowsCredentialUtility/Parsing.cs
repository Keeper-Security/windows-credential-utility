using System.Text;
using System.Text.Json;

namespace WindowsCredentialUtility;

public class Parsing
{
    /// <summary>
    /// Parses the given KSM config string into a base64 encoded string.
    /// <para>
    /// Accepts either: <br/>
    ///     1. A path to a JSON configuration file.<br/>
    ///     2. A JSON configuration string.<br/>
    ///     3. A Base64 encoded string.
    /// </para>
    /// </summary>
    /// <param name="config">The string to parse.</param>
    /// <returns>Base64 String</returns>
    /// <exception cref="ArgumentException">Thrown when the input is not a valid JSON string, file path, or base64 string.</exception>"
    public static string ParseConfig(string config)
    {
        if (Path.Exists(config))
        {
            try
            {
                using StreamReader reader = File.OpenText(config);
                string fileContents = reader.ReadToEnd();
                if (!IsJson(fileContents))
                {
                    throw new ArgumentException("Invalid JSON file contents");
                }
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContents));
            }
            catch (FileNotFoundException)
            {
                throw new ArgumentException("Invalid config file path");
            }
        }
        else if (IsJson(config))
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(config));
        }
        else
        {
            try
            {
                // Test the string can be parsed as valid Base64.
                // Will raise FormatException if not a valid Base64 string.
                Convert.FromBase64String(config);

                // Return original base64 string if valid.
                return config;
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid config string");
            }
        }
    }

    private static bool IsJson(string source)
    {
        if (source == null) return false;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(source);
            doc.Dispose();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
