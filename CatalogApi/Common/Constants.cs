namespace CatalogApi.Common;

public class Constants
{
    // Name constants
    public const string ProductNameRequired = "Name is required";
    public const int ProductNameMaxLength = 100;
    public static readonly string ProductNameTooLong = $"Name cannot exceed {ProductNameMaxLength} characters";

    // Description constants
    public const string ProductDescriptionRequired = "Description is required";
    public const int ProductDescriptionMaxLength = 500;
    public static readonly string ProductDescriptionTooLong = $"Description cannot exceed {ProductDescriptionMaxLength} characters.";

    // Price constants
    public const string ProductPriceMustBeGreaterThanZero = "Price must be greater than 0";

    // Stock constants
    public const string ProductStockMustBeNonNegative = "Stock must be non-negative";
}
