using MoviesAPI.Interfaces;

namespace MoviesAPI.Services;

public class LocalStorageService: IFileStorage
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _mainFolder = "localStorage/";

    public LocalStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> SaveFile(byte[] content, string extension, string container, string contentType)
    {
        var fileName = $"{Guid.NewGuid()}{extension}";
        string folder = Path.Combine(_env.WebRootPath, _mainFolder,container);

        if (!Directory.Exists(_mainFolder))
        {
            Directory.CreateDirectory(_mainFolder);
        }
        
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        string route = Path.Combine(folder, fileName);
        await File.WriteAllBytesAsync(route, content);

        var actualUrl =
            $"{_httpContextAccessor.HttpContext!.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
        
        var dbUrl = Path.Combine(actualUrl,  _mainFolder, container, fileName).Replace("\\", "/");
        
        return dbUrl;
    }

    public async Task<string> UpdateFile(byte[] content, string extension, string container, string route, string contentType)
    {
        await DeleteFile(route, container);
        return await SaveFile(content, extension, container, contentType);
    }

    public Task<int> DeleteFile(string route, string container)
    {
        if (route != null)
        {
            var fileName = Path.GetFileName(route);
            string file = Path.Combine(_env.WebRootPath, _mainFolder, container, fileName);

            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        return Task.FromResult(0);
    }
}