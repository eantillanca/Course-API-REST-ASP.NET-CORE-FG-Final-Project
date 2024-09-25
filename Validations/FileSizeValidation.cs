using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Validations;

public class FileSizeValidation: ValidationAttribute
{
    private readonly int _maxSizeImageMb;

    public FileSizeValidation(int maxSizeImageMb)
    {
        _maxSizeImageMb = maxSizeImageMb;
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

        return formFile.Length > (_maxSizeImageMb * 1024 * 1024) 
            ? new ValidationResult($"The file size must not exceed {_maxSizeImageMb} MB") 
            : ValidationResult.Success;
    }
}