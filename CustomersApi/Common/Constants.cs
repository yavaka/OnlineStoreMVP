namespace CustomersApi.Common;

public class Constants
{
    // Name
    public const string CustomerNameRequired = "Name is required";
    public const int CustomerNameMaxLength = 100;
    public static readonly string CustomerNameTooLong = $"Name must not exceed {CustomerNameMaxLength} characters";

    // Email
    public const string CustomerEmailRequired = "Email is required";
    public const string CustomerEmailInvalid = "Email is invalid"; 
    
    // Address
    public const string CustomerAddressRequired = "Address is required";
    public const int CustomerAddressMaxLength = 200;
    public static readonly string CustomerAddressTooLong = $"Address must not exceed {CustomerAddressMaxLength} characters";
}
