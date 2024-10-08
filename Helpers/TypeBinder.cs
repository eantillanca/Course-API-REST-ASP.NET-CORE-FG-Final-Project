using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace MoviesAPI.Helpers;

public class TypeBinder<T>: IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var propertyName = bindingContext.ModelName;
        var valuesProvider = bindingContext.ValueProvider.GetValue(propertyName);

        if (valuesProvider == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        try
        {
            var deserializedValue = JsonConvert.DeserializeObject<T>(valuesProvider.FirstValue!);
            bindingContext.Result = ModelBindingResult.Success(deserializedValue);
        }
        catch
        {
            bindingContext.ModelState.AddModelError(propertyName, "Invalid value type");
        }
        
        return Task.CompletedTask;
    }
}