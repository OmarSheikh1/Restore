namespace API.Dtos;

/// <summary>
/// [ADDED|1.0.0] DTO for transferring product data in API responses
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public long Price { get; set; }
    public required string PictureUrl { get; set; }
    public required string Type { get; set; }
    public required string Brand { get; set; }
    public int QuantityInStock { get; set; }
}

/// <summary>
/// [ADDED|1.0.0] DTO for creating a new product
/// </summary>
public class CreateProductDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public long Price { get; set; }
    public required string PictureUrl { get; set; }
    public required string Type { get; set; }
    public required string Brand { get; set; }
    public int QuantityInStock { get; set; }
}

/// <summary>
/// [ADDED|1.0.0] DTO for updating an existing product
/// </summary>
public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public long? Price { get; set; }
    public string? PictureUrl { get; set; }
    public string? Type { get; set; }
    public string? Brand { get; set; }
    public int? QuantityInStock { get; set; }
}
