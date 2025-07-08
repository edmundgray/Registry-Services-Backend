namespace RegistryApi.Helpers;

/// <summary>
/// Parameters for pagination, generic search, and sorting.
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 5000; // Maximum allowed page size
    private int _pageSize = 1000; // if no value is set, default to 1000

    public int PageNumber { get; set; } = 1; // Default to the first page

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : Math.Max(1, value); // Ensure page size is at least 1 and does not exceed MaxPageSize
    }

    /// <summary>
    /// Generic search term to filter results.
    /// </summary>
    public string? SearchTerm { get; set; } // This can be used to search across multiple fields like SpecificationName, Purpose, Sector, etc.

    /// <summary>
    /// Field to sort by (e.g., "SpecificationName", "ModifiedDate").
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort order: "ASC" for ascending, "DESC" for descending.
    /// </summary>
    public string? SortOrder { get; set; }

    /// <summary>
    /// Type of specification to filter by (e.g., "Core", "Extension").
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

    /// <summary>
    /// BusinessTermID from an extension component to filter by.
    /// </summary>
    public string? ExtensionBusinessTermId { get; set; }
    
    /// <summary>
    /// BusinessTermID from a core component to filter by.
    /// </summary>
    public string? CoreBusinessTermId { get; set; }
    /// <summary>
    /// BusinessTermID from an additional requirement to filter by.
    /// </summary>
    public string? AddReqBusinessTermID { get; set; }
}