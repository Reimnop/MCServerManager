using System.ComponentModel.DataAnnotations;

namespace MCServerManager.Models;

[Serializable]
public class ServerConfigModel
{
    [Required] 
    public string? ServerDirectory { get; set; }

    [Required] 
    public string? JarFile { get; set; }

    public bool NoGui { get; set; }

    public string? ExtraArgs { get; set; }

    public ServerConfigModel Copy() 
    {
        return new ServerConfigModel 
        {
            ServerDirectory = ServerDirectory,
            JarFile = JarFile,
            NoGui = NoGui,
            ExtraArgs = ExtraArgs
        };
    }
}