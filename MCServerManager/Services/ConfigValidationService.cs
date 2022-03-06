using MCServerManager.Models;
using MCServerManager.Data;

namespace MCServerManager.Services;

public class ConfigValidationService
{
    public bool ValidateConfig(out string validationMessage) 
    {
        if (!ValidateJvmConfig(out validationMessage)) 
        {
            return false;
        }

        if (!ValidateJvmConfig(out validationMessage)) 
        {
            return false;
        }

        validationMessage = string.Empty;
        return true;
    }

    public bool ValidateJvmConfig(out string validationMessage)
    {
        JvmConfigModel jvmConfig = ConfigManager.JvmConfig;

        if (string.IsNullOrWhiteSpace(jvmConfig.JavaPath))
        {
            validationMessage = "Java path is empty";
            return false;
        }
        
        if (!File.Exists(jvmConfig.JavaPath))
        {
            validationMessage = $"Java not found at {jvmConfig.JavaPath}";
            return false;
        }

        if (jvmConfig.MaxRamMb < jvmConfig.MinRamMb)
        {
            validationMessage = "Maximum RAM must be greater than or equal to minimum RAM";
            return false;
        }

        if (jvmConfig.MaxRamMb == 0)
        {
            validationMessage = "Maximum RAM must be greater than 0";
            return false;
        }

        validationMessage = string.Empty;
        return true;
    }

    public bool ValidateServerConfig(out string validationMessage)
    {
        ServerConfigModel serverConfig = ConfigManager.ServerConfig;

        if (string.IsNullOrWhiteSpace(serverConfig.ServerDirectory))
        {
            validationMessage = "Server directory is empty";
            return false;
        }

        if (!Directory.Exists(serverConfig.ServerDirectory))
        {
            validationMessage = "Server directory does not exist";
            return false;
        }

        if (string.IsNullOrWhiteSpace(serverConfig.JarFile))
        {
            validationMessage = "Jar file is empty";
            return false;
        }
        
        if (!File.Exists(Path.Combine(serverConfig.ServerDirectory, serverConfig.JarFile)))
        {
            validationMessage = "Jar file does not exist";
            return false;
        }

        validationMessage = string.Empty;
        return true;
    }
}