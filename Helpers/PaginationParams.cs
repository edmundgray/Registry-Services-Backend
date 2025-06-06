namespace RegistryApi.Helpers;

/// <summary>
/// Parameters for pagination, generic search, and sorting.
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 5000;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : Math.Max(1, value);
    }

    /// <summary>
    /// Generic search term to filter results.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Field to sort by (e.g., "SpecificationName", "ModifiedDate").
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort order: "ASC" for ascending, "DESC" for descending.
    /// </summary>
    public string? SortOrder { get; set; }

    /// <summary>
    /// Type of specification to filter by (e.g., "Core", "Extension", "CountrySpecific").
    /// Intended max length for values is 50 characters.
    /// </summary>
    public string? SpecificationType { get; set; }

    /// <summary>
    /// Sector to filter by.
    /// </summary>
    public string? Sector { get; set; }

    /// <summary>
    /// Country to filter by.
    /// </summary>
    public string? Country { get; set; }
}
