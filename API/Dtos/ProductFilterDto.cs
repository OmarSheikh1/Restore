namespace API.Dtos;

/// <summary>
/// [ADDED|1.0.0] DTO for filtering and querying products with pagination and sorting support
/// </summary>
public class ProductFilterDto
{
    /// <summary>
    /// Page number for pagination (1-based index)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Search term to filter by product name or description
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by product brand
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Filter by product type/category
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Minimum price filter (in cents)
    /// </summary>
    public long? MinPrice { get; set; }

    /// <summary>
    /// Maximum price filter (in cents)
    /// </summary>
    public long? MaxPrice { get; set; }

    /// <summary>
    /// Field to sort by: 'name', 'price', 'stock', 'created'
    /// </summary>
    public string? SortBy { get; set; } = "name";

    /// <summary>
    /// Sort direction: 'asc' or 'desc'
    /// </summary>
    public string? SortDirection { get; set; } = "asc";

    /// <summary>
    /// Validate filter parameters
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        if (Page < 1)
        {
            errorMessage = "Page must be greater than 0";
            return false;
        }

        if (PageSize < 1 || PageSize > 100)
        {
            errorMessage = "PageSize must be between 1 and 100";
            return false;
        }

        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
        {
            errorMessage = "MinPrice cannot be greater than MaxPrice";
            return false;
        }

        errorMessage = null;
        return true;
    }
}
