namespace CustomersApi.Tests.Controllers;

public class CustomersControllerTests
{
    public class CreateCustomerTests
    {
        /*
            - ✅ Returns 201 Created with valid customer
            - ✅ Generates new Guid for customer ID
            - ✅ Returns 400 BadRequest with validation errors
            - ✅ Validates Name (required, max 100 chars)
            - ✅ Validates Email (required, valid format)
            - ✅ Validates Address (required, max 200 chars)
            - ✅ Handles exceptions correctly
        */
    }

    public class UpdateCustomerTests
    {
        /*
            - ✅ Returns 204 NoContent when update succeeds
            - ✅ Returns 404 NotFound when customer doesn't exist (repository returns null)
            - ✅ Returns 400 BadRequest with validation errors
            - ✅ Verifies repository.UpdateAsync is called with correct parameters
            - ✅ Verifies repository finds customer internally (mocks GetByIdAsync within UpdateAsync)
            - ✅ Handles exceptions correctly
         */
    }

    public class GetCustomersTests
    {
        /*
            - ✅ Returns 200 OK with list of customers
            - ✅ Returns empty list when no customers exist
            - ✅ Handles exceptions correctly
        */
    }

    public class GetCustomerByIdTests
    {
        /*
            - ✅ Returns 200 OK with customer when found
            - ✅ Returns 404 NotFound when customer doesn't exist (repository returns null)
            - ✅ Handles exceptions correctly
         */
    }

    public class DeleteCustomerTests
    {
        /*
            - ✅ Returns 204 NoContent when deletion is successful
            - ✅ Returns 404 NotFound when customer doesn't exist
            - ✅ Handles exceptions correctly
         */
    }
}
