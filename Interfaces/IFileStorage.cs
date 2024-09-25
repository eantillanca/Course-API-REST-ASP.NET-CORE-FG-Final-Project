namespace MoviesAPI.Interfaces;

public interface IFileStorage
{
    Task<string> SaveFile(byte[] content, string extension, string container, string contentType);
    Task<string> UpdateFile(byte[] content, string extension, string container, string route, string contentType);
    Task<int> DeleteFile(string route, string container);
}