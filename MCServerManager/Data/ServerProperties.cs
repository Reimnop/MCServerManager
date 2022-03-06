using System.Text.RegularExpressions;

namespace MCServerManager.Data;

public class ServerProperties
{
    private readonly Dictionary<string, string> values = new();

    public ServerProperties(string text)
    {
        string[] lines = Regex.Split(text, "\n|\r\n");

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            int i = 0;
            
            string key = string.Empty;
            string value = string.Empty;
            
            while (line[i] != '=')
            {
                key += line[i];
                i++;
            }
            i++; // skip the '='
            while (i < line.Length)
            {
                value += line[i];
                i++;
            }
            
            values.Add(key, value);
        }
    }

    public static ServerProperties FromFile(string path)
    {
        return new ServerProperties(File.ReadAllText(path));
    }
    
    public string this[string key] => values[key];
}