using System.ComponentModel.DataAnnotations;
using TanvirArjel.CustomValidation.Attributes;

namespace MCServerManager.Models;

[Serializable]
public class JvmConfigModel
{
    [Required]
    public string? JavaPath { get; set; }

    [Required]
    public int? MinRamMb { get; set; }
    
    [Required]
    [CompareTo(nameof(MinRamMb), ComparisonType.GreaterThanOrEqual)]
    public int? MaxRamMb { get; set; }
    public string? ExtraArgs { get; set; }

    public JvmConfigModel Copy() 
    {
        return new JvmConfigModel 
        {
            JavaPath = JavaPath,
            MinRamMb = MinRamMb,
            MaxRamMb = MaxRamMb,
            ExtraArgs = ExtraArgs
        };
    }
}