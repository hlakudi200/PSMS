using System.ComponentModel.DataAnnotations;

namespace psms.Users.Dto;

public class ChangeUserLanguageDto
{
    [Required]
    public string LanguageName { get; set; }
}