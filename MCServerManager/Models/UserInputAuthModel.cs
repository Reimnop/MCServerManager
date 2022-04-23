using System.ComponentModel.DataAnnotations;

namespace MCServerManager.Models;

public class UserInputAuthModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; }
}