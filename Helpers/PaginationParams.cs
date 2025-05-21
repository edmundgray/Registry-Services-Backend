namespace RegistryApi.Helpers;

/// <summary>
/// Base class for pagination query parameters.
/// No significant .NET 8 changes needed here.
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : Math.Max(1, value);
    }
}
