using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Validations;

public class FileTypeValidation: ValidationAttribute
{
    private readonly string[] _validTypes;

    public FileTypeValidation(string[] validTypes)
    {
        _validTypes = validTypes;
    }
    
    public FileTypeValidation(FileTypeGroup fileTypeGroup)
    {
        if (fileTypeGroup == FileTypeGroup.Image)
        {
            _validTypes = new string[]
            {
                "image/jpeg", "image/jpg", "image/png", "image/gif",
                "image/JPEG", "image/JPG", "image/PNG", "image/GIF"
            };
        }
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is not IFormFile formFile)
        {
            return ValidationResult.Success;
        }

        return !_validTypes.Contains(formFile.ContentType) 
            ? new ValidationResult($"File Type no valid. Available formats: {string.Join(", ", _validTypes)}") 
            : ValidationResult.Success;
    }
}