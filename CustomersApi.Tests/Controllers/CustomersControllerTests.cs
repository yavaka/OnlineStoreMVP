using CustomersApi.Common;
using CustomersApi.Controllers;
using CustomersApi.Data.Repositories;
using CustomersApi.Models;
using CustomersApi.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineStoreMVP.ServiceDefaults.Models;
using OnlineStoreMVP.TestUtilities.Helpers;

namespace CustomersApi.Tests.Controllers;

public class CustomersControllerTests
{
    private readonly Mock<ILogger<CustomersController>> _mockLogger = MockHelpers.CreateMockLogger<CustomersController>();
    private readonly Mock<IValidator<CustomerModel>> _mockValidator = ValidationHelpers.CreateValidValidator<CustomerModel>();
    private readonly Mock<ICustomerRepository> _mockRepository = new();
    private readonly CustomersController _controller;

    public CustomersControllerTests()
    {
        _controller = new(
            _mockLogger.Object,
            _mockRepository.Object,
            _mockValidator.Object
        );

        ControllerHelpers.SetupHttpContext(_controller);
    }

    public class CreateCustomerTests : CustomersControllerTests
    {
        [Fact]
        public async Task CreateCustomer_WithValidCustomer_Returns201Created()
        {
            // Arrange
            var customerToCreate = CustomerTestHelpers.CreateTestCustomer();

            _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<CustomerModel>()))
                .ReturnsAsync((CustomerModel customer) => customer);

            // Act
            var result = await _controller.CreateCustomer(customerToCreate);

            // Assert
            var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.StatusCode.Should().Be(201);
            createdAtActionResult.ActionName.Should().Be(nameof(CustomersController.CreateCustomer));

            var returnedCustomer = createdAtActionResult.Value.Should().BeOfType<CustomerModel>().Subject;
            returnedCustomer.Id.Should().NotBeEmpty();
            returnedCustomer.Id.Should().NotBe(Guid.Empty);
            returnedCustomer.Name.Should().Be(customerToCreate.Name);
            returnedCustomer.Email.Should().Be(customerToCreate.Email);
            returnedCustomer.Address.Should().Be(customerToCreate.Address);

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<CustomerModel>()),
                Times.Once);

            _mockValidator.Verify(
                v => v.Validate(It.Is<CustomerModel>(c => c == customerToCreate)),
                Times.Once);
        }

        [Fact]
        public async Task CreateCustomer_WhenIdIsEmpty_GeneratesNewGuid()
        {
            // Arrange
            var customerToCreate = CustomerTestHelpers.CreateTestCustomer(id: Guid.Empty);

            var generatedId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<CustomerModel>()))
                .ReturnsAsync((CustomerModel customer) =>
                {
                    customer.Id = generatedId;
                    return customer;
                });

            // Act
            var result = await _controller.CreateCustomer(customerToCreate);

            // Assert
            var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.StatusCode.Should().Be(201);

            var createdCustomer = createdAtActionResult.Value.Should().BeOfType<CustomerModel>().Subject;
            createdCustomer.Id.Should().NotBeEmpty();
            createdCustomer.Id.Should().NotBe(Guid.Empty);
            createdCustomer.Id.Should().Be(generatedId);

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<CustomerModel>()),
                Times.Once);

            _mockValidator.Verify(
                v => v.Validate(It.IsAny<CustomerModel>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateCustomer_WithInvalidCustomer_Returns400BadRequest()
        {
            // Arrange
            var invalidCustomer = CustomerTestHelpers.CreateInvalidCustomer();

            var mockInvalidValidator = ValidationHelpers.CreateInvalidValidator<CustomerModel>(new Dictionary<string, string[]>
            {
                { nameof(CustomerModel.Name), new[] { Constants.CustomerNameRequired } },
                { nameof(CustomerModel.Email), new[] { Constants.CustomerEmailInvalid } },
                { nameof(CustomerModel.Address), new[] { Constants.CustomerAddressRequired } }
            });

            var controller = new CustomersController(
                _mockLogger.Object,
                _mockRepository.Object,
                mockInvalidValidator.Object);

            ControllerHelpers.SetupHttpContext(controller);

            // Act
            var result = await controller.CreateCustomer(invalidCustomer);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().BeOfType<ErrorResponse>();

            var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Errors.Should().NotBeNull();

            // Name
            errorResponse.Errors.Should().ContainKey(nameof(CustomerModel.Name));
            errorResponse.Errors[nameof(CustomerModel.Name)].Should().Contain(Constants.CustomerNameRequired);

            // Email
            errorResponse.Errors.Should().ContainKey(nameof(CustomerModel.Email));
            errorResponse.Errors[nameof(CustomerModel.Email)].Should().Contain(Constants.CustomerEmailInvalid);

            // Address
            errorResponse.Errors.Should().ContainKey(nameof(CustomerModel.Address));
            errorResponse.Errors[nameof(CustomerModel.Address)].Should().Contain(Constants.CustomerAddressRequired);

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<CustomerModel>()),
                Times.Never);

            mockInvalidValidator.Verify(
                v => v.Validate(It.Is<CustomerModel>(c => c == invalidCustomer)),
                Times.Once);
        }

        [Fact]
        public async Task CreateCustomer_WithNameExceedingMaxLength_Returns400BadRequest()
        {
            // Arrange
            var invalidCustomer = CustomerTestHelpers.CreateTestCustomer();
            invalidCustomer.Name = new string('A', Constants.CustomerNameMaxLength + 1);

            var mockInvalidValidator = ValidationHelpers.CreateInvalidValidator<CustomerModel>(new Dictionary<string, string[]>
            {
                { nameof(CustomerModel.Name), new string[] { Constants.CustomerNameTooLong } }
            });

            var controller = new CustomersController(
                _mockLogger.Object,
                _mockRepository.Object,
                mockInvalidValidator.Object);

            ControllerHelpers.SetupHttpContext(controller);

            // Act
            var result = await controller.CreateCustomer(invalidCustomer);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().BeOfType<ErrorResponse>();

            var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Errors.Should().NotBeNull();

            errorResponse.Errors.Should().ContainKey(nameof(CustomerModel.Name));
            errorResponse.Errors[nameof(CustomerModel.Name)].Should().Contain(Constants.CustomerNameTooLong);

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<CustomerModel>()),
                Times.Never);

            mockInvalidValidator.Verify(
                v => v.Validate(It.Is<CustomerModel>(p => p == invalidCustomer)),
                Times.Once);
        }

        [Fact]
        public async Task CreateCustomer_WithAddressExceedingMaxLength_Returns400BadRequest()
        {
            // Arrange
            var invalidCustomer = CustomerTestHelpers.CreateTestCustomer();
            invalidCustomer.Address = new string('A', Constants.CustomerAddressMaxLength + 1);

            var mockInvalidValidator = ValidationHelpers.CreateInvalidValidator<CustomerModel>(new Dictionary<string, string[]>
            {
                { nameof(CustomerModel.Address), new string[] { Constants.CustomerAddressTooLong } }
            });

            var controller = new CustomersController(
                _mockLogger.Object,
                _mockRepository.Object,
                mockInvalidValidator.Object);

            ControllerHelpers.SetupHttpContext(controller);

            // Act
            var result = await controller.CreateCustomer(invalidCustomer);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().BeOfType<ErrorResponse>();

            var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Errors.Should().NotBeNull();

            errorResponse.Errors.Should().ContainKey(nameof(CustomerModel.Address));
            errorResponse.Errors[nameof(CustomerModel.Address)].Should().Contain(Constants.CustomerAddressTooLong);

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<CustomerModel>()),
                Times.Never);

            mockInvalidValidator.Verify(
                v => v.Validate(It.Is<CustomerModel>(p => p == invalidCustomer)),
                Times.Once);
        }

        [Fact]
        public async Task CreateCustomer_WithEmptyEmail_Returns400BadRequest()
        {
            // Arrange
            var invalidCustomer = CustomerTestHelpers.CreateTestCustomer();
            invalidCustomer.Email = string.Empty;

            var mockInvalidValidator = ValidationHelpers.CreateInvalidValidator<CustomerModel>(new Dictionary<string, string[]>
            {
                { nameof(CustomerModel.Email), new string[] { Constants.CustomerEmailRequired } }
            });

            // Create controller with the invalid validator
            var controller = new CustomersController(
                _mockLogger.Object,
                _mockRepository.Object,
                mockInvalidValidator.Object);

            ControllerHelpers.SetupHttpContext(controller);

            // Act
            var result = await controller.CreateCustomer(invalidCustomer);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().BeOfType<ErrorResponse>();

            var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Errors.Should().NotBeNull();

            errorResponse.Errors.Should().ContainKey(nameof(CustomerModel.Email));
            errorResponse.Errors[nameof(CustomerModel.Email)].Should().Contain(Constants.CustomerEmailRequired);

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<CustomerModel>()),
                Times.Never);

            mockInvalidValidator.Verify(
                v => v.Validate(It.Is<CustomerModel>(p => p == invalidCustomer)),
                Times.Once);
        }

        [Fact]
        public async Task CreateCustomer_WhenRepositoryThrowsException_Returns500InternalServerError()
        {
            // Arrange
            var customerToCreate = CustomerTestHelpers.CreateTestCustomer();

            _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<CustomerModel>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateCustomer(customerToCreate);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeOfType<ErrorResponse>();

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<CustomerModel>()),
                Times.Once);
        }
    }

    public class UpdateCustomerTests : CustomersControllerTests
    {
        [Fact]
        public async Task UpdateCustomer_WithValidCustomer_Returns204NoContent()
        {
            // Arrange
            var existingCustomerId = Guid.NewGuid();
            var updatedCustomer = CustomerTestHelpers.CreateTestCustomer(id: existingCustomerId);

            _mockRepository.Setup(repo => repo.UpdateAsync(existingCustomerId, updatedCustomer))
                .ReturnsAsync(updatedCustomer);
            
            // Act
            var result = await _controller.UpdateCustomer(existingCustomerId, updatedCustomer);

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
            noContentResult!.StatusCode.Should().Be(204);

            _mockRepository.Verify(
                r => r.UpdateAsync(existingCustomerId, It.Is<CustomerModel>(p => p == updatedCustomer)),
                Times.Once);

            _mockValidator.Verify(
                v => v.Validate(It.Is<CustomerModel>(p => p == updatedCustomer)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCustomer_WhenCustomerDoesNotExist_Returns404NotFound()
        {
            // Arrange
            var nonExistentCustomerId = Guid.NewGuid();
            var updatedCustomer = CustomerTestHelpers.CreateTestCustomer();

            _mockRepository.Setup(r => r.UpdateAsync(nonExistentCustomerId, It.IsAny<CustomerModel>()))
                           .ReturnsAsync((CustomerModel?)null);

            // Act
            var result = await _controller.UpdateCustomer(nonExistentCustomerId, updatedCustomer);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(404);

            _mockRepository.Verify(
                r => r.UpdateAsync(nonExistentCustomerId, It.IsAny<CustomerModel>()),
                Times.Once);

            _mockValidator.Verify(
                v => v.Validate(It.Is<CustomerModel>(p => p == updatedCustomer)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCustomer_WithInvalidCustomer_Returns400BadRequest()
        {
            // Arrange
            var existingCustomerId = Guid.NewGuid();
            var invalidCustomer = CustomerTestHelpers.CreateInvalidCustomer();

            var mockInvalidValidator = ValidationHelpers.CreateInvalidValidator<CustomerModel>(new Dictionary<string, string[]>
            {
                { nameof(CustomerModel.Name), new[] { Constants.CustomerNameRequired } },
                { nameof(CustomerModel.Email), new[] { Constants.CustomerEmailInvalid } },
                { nameof(CustomerModel.Address), new[] { Constants.CustomerAddressRequired } }
            });

            var controller = new CustomersController(
                _mockLogger.Object,
                _mockRepository.Object,
                mockInvalidValidator.Object);

            ControllerHelpers.SetupHttpContext(controller);

            // Act
            var result = await controller.UpdateCustomer(existingCustomerId, invalidCustomer);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);

            var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Errors.Should().NotBeNull();

            // Name
            errorResponse.Errors.Should().ContainKey(nameof(CustomerModel.Name));
            errorResponse.Errors[nameof(CustomerModel.Name)].Should().Contain(Constants.CustomerNameRequired);

            // Email
            errorResponse.Errors.Should().ContainKey(nameof(CustomerModel.Email));
            errorResponse.Errors[nameof(CustomerModel.Email)].Should().Contain(Constants.CustomerEmailInvalid);

            // Address
            errorResponse.Errors.Should().ContainKey(nameof(CustomerModel.Address));
            errorResponse.Errors[nameof(CustomerModel.Address)].Should().Contain(Constants.CustomerAddressRequired);

            _mockRepository.Verify(
                r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<CustomerModel>()),
                Times.Never);

            mockInvalidValidator.Verify(
                v => v.Validate(It.Is<CustomerModel>(c => c == invalidCustomer)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCustomer_WhenRepositoryThrowsException_Returns500InternalServerError()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var updatedCustomer = CustomerTestHelpers.CreateTestCustomer();
            _mockRepository.Setup(r => r.UpdateAsync(customerId, It.IsAny<CustomerModel>()))
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateCustomer(customerId, updatedCustomer);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeOfType<ErrorResponse>();

            _mockRepository.Verify(
                r => r.UpdateAsync(customerId, It.IsAny<CustomerModel>()),
                Times.Once);
        }
    }

    public class GetCustomersTests : CustomersControllerTests
    {
        [Fact]
        public async Task GetCustomers_Returns200OkWithListOfCustomers()
        {
            // Arrange
            var customers = CustomerTestHelpers.CreateTestCustomers(3);

            _mockRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(customers);

            // Act
            var result = await _controller.GetCustomers();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(customers);

            _mockRepository.Verify(
                r => r.GetAllAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetCustomers_WhenNoCustomersExist_Returns200OkWithEmptyList()
        {
            // Arrange
            var emptyCustomers = new List<CustomerModel>();

            _mockRepository.Setup(r => r.GetAllAsync())
                           .ReturnsAsync(emptyCustomers);

            // Act
            var result = await _controller.GetCustomers();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(emptyCustomers);

            _mockRepository.Verify(
                r => r.GetAllAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetCustomers_WhenRepositoryThrowsException_Returns500InternalServerError()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetCustomers();

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeOfType<ErrorResponse>();

            _mockRepository.Verify(
                r => r.GetAllAsync(),
                Times.Once);
        }
    }

    public class GetCustomerByIdTests : CustomersControllerTests
    {
        [Fact]
        public async Task GetCustomer_WithValidId_Returns200OkWithCustomer()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = CustomerTestHelpers.CreateTestCustomer(customerId);
            _mockRepository.Setup(r => r.GetByIdAsync(customerId))
                           .ReturnsAsync(customer);

            // Act
            var result = await _controller.GetCustomer(customerId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(customer);

            _mockRepository.Verify(
                r => r.GetByIdAsync(customerId),
                Times.Once);
        }

        [Fact]
        public async Task GetCustomer_WhenCustomerDoesNotExist_Returns404NotFound()
        {
            // Arrange
            var nonExistentCustomerId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(nonExistentCustomerId))
                           .ReturnsAsync((CustomerModel?)null);

            // Act
            var result = await _controller.GetCustomer(nonExistentCustomerId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(404);

            _mockRepository.Verify(
                r => r.GetByIdAsync(nonExistentCustomerId),
                Times.Once);
        }

        [Fact]
        public async Task GetCustomer_WhenRepositoryThrowsException_Returns500InternalServerError()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(customerId))
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetCustomer(customerId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeOfType<ErrorResponse>();

            _mockRepository.Verify(
                r => r.GetByIdAsync(customerId),
                Times.Once);
        }
    }

    public class DeleteCustomerTests : CustomersControllerTests
    {
        [Fact]
        public async Task DeleteCustomer_WhenCustomerExists_Returns204NoContent()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteAsync(customerId))
                           .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCustomer(customerId);

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
            noContentResult.StatusCode.Should().Be(204);

            _mockRepository.Verify(
                r => r.DeleteAsync(customerId),
                Times.Once);
        }

        [Fact]
        public async Task DeleteCustomer_WhenCustomerDoesNotExist_Returns404NotFound()
        {
            // Arrange
            var nonExistentCustomerId = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteAsync(nonExistentCustomerId))
                           .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteCustomer(nonExistentCustomerId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(404);

            _mockRepository.Verify(
                r => r.DeleteAsync(nonExistentCustomerId),
                Times.Once);
        }

        [Fact]
        public async Task DeleteCustomer_WhenRepositoryThrowsException_Returns500InternalServerError()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteAsync(customerId))
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteCustomer(customerId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeOfType<ErrorResponse>();

            _mockRepository.Verify(
                r => r.DeleteAsync(customerId),
                Times.Once);
        }
    }
}
