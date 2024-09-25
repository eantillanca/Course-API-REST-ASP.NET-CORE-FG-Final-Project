namespace MoviesAPI.Dtos;

public class PaginationDto
{
    private int _elementsPerPage = 10;
    private readonly int _maxElementsPerPage = 50;

    public int Page { get; set; } = 1;
    public int ElementsPerPage
    {
        get => _elementsPerPage;
        set => _elementsPerPage = value > _maxElementsPerPage ? _maxElementsPerPage : value;
    }
}