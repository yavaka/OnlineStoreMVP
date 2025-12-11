using CatalogApi.Common;
using CatalogApi.Controllers;
using CatalogApi.Data.Repositories;
using CatalogApi.Models;
using CatalogApi.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineStoreMVP.ServiceDefaults.Models;
using OnlineStoreMVP.TestUtilities.Helpers;

namespace CatalogApi.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<ILogger<ProductsController>> _mockLogger = MockHelpers.CreateMockLogger<ProductsController>();
    private readonly Mock<IValidator<ProductModel>> _mockValidator = ValidationHelpers.CreateValidValidator<ProductModel>();
    private readonly Mock<IProductRepository> _mockRepository = new();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _controller = new(
            _mockLogger.Object,
            _mockRepository.Object,
            _mockValidator.Object);

        ControllerHelpers.SetupHttpContext(_controller);
    }

    public class CreateProductTests : ProductsControllerTests
    {
        [Fact]
        public async Task CreateProduct_WithValidProduct_Returns201Created()
        {
            // Arrange
            var productToCreate = ProductTestHelpers.CreateTestProduct();

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ProductModel>()))
                           .ReturnsAsync((ProductModel p) => p);

            // Act
            var result = await _controller.CreateProduct(productToCreate);

            // Assert
            var createdAtResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtResult.StatusCode.Should().Be(201);
            createdAtResult.ActionName.Should().Be(nameof(ProductsController.CreateProduct));

            var returnedProduct = createdAtResult.Value.Should().BeOfType<ProductModel>().Subject;
            returnedProduct.Name.Should().Be(productToCreate.Name);
            returnedProduct.Description.Should().Be(productToCreate.Description);
            returnedProduct.Price.Should().Be(productToCreate.Price);
            returnedProduct.Stock.Should().Be(productToCreate.Stock);

            // Verify that a new Guid was generated (ID should not be empty)
            returnedProduct.Id.Should().NotBeEmpty();
            returnedProduct.Id.Should().NotBe(Guid.Empty);

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<ProductModel>()),
                Times.Once);

            _mockValidator.Verify(
                v => v.Validate(It.Is<ProductModel>(p => p == productToCreate)),
                Times.Once);
        }

        [Fact]
        public async Task CreateProduct_WhenIdIsEmpty_GeneratesNewGuid()
        {
            // Arrange
            var productToCreate = ProductTestHelpers.CreateTestProduct(id: Guid.Empty);

            var generatedId = Guid.NewGuid();
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ProductModel>()))
                .ReturnsAsync((ProductModel p) =>
                {
                    p.Id = generatedId;
                    return p;
                });

            // Act
            var result = await _controller.CreateProduct(productToCreate);

            // Assert
            var createdAtResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtResult.StatusCode.Should().Be(201);

            var createdProduct = createdAtResult.Value.Should().BeOfType<ProductModel>().Subject;

            createdProduct.Id.Should().NotBeEmpty();
            createdProduct.Id.Should().NotBe(Guid.Empty);
            createdProduct.Id.Should().Be(generatedId);

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<ProductModel>()),
                Times.Once);

            _mockValidator.Verify(
                v => v.Validate(It.IsAny<ProductModel>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateProduct_WithInvalidProduct_Returns400BadRequest()
        {
            // Arrange
            var invalidProduct = ProductTestHelpers.CreateInvalidProduct();

            var mockInvalidValidator = ValidationHelpers.CreateInvalidValidator<ProductModel>(new Dictionary<string, string[]>
            {
                { nameof(ProductModel.Name), new string[] { Constants.ProductNameRequired } },
                { nameof(ProductModel.Description), new[] { Constants.ProductDescriptionRequired } },
                { nameof(ProductModel.Stock), new[] { Constants.ProductStockMustBeNonNegative } },
                { nameof(ProductModel.Price), new[] { Constants.ProductPriceMustBeGreaterThanZero } }
            });

            var controller = new ProductsController(
                _mockLogger.Object,
                _mockRepository.Object,
                mockInvalidValidator.Object);

            ControllerHelpers.SetupHttpContext(controller);

            // Act
            var result = await controller.CreateProduct(invalidProduct);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);

            var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Errors.Should().NotBeNull();

            // Name
            errorResponse.Errors.Should().ContainKey(nameof(ProductModel.Name));
            errorResponse.Errors[nameof(ProductModel.Name)].Should().Contain(Constants.ProductNameRequired);

            // Description
            errorResponse.Errors.Should().ContainKey(nameof(ProductModel.Description));
            errorResponse.Errors[nameof(ProductModel.Description)].Should().Contain(Constants.ProductDescriptionRequired);

            // Stock
            errorResponse.Errors.Should().ContainKey(nameof(ProductModel.Stock));
            errorResponse.Errors[nameof(ProductModel.Stock)].Should().Contain(Constants.ProductStockMustBeNonNegative);

            // Price
            errorResponse.Errors.Should().ContainKey(nameof(ProductModel.Price));
            errorResponse.Errors[nameof(ProductModel.Price)].Should().Contain(Constants.ProductPriceMustBeGreaterThanZero);

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<ProductModel>()),
                Times.Never);

            mockInvalidValidator.Verify(
                v => v.Validate(It.Is<ProductModel>(p => p == invalidProduct)),
                Times.Once);
        }

        [Fact]
        public async Task CreateProduct_WithNameExceedingMaxLength_Returns400BadRequest()
        {
            // Arrange
            var invalidProduct = ProductTestHelpers.CreateTestProduct();
            invalidProduct.Name = new string('A', Constants.ProductNameMaxLength + 1);

            var mockInvalidValidator = ValidationHelpers.CreateInvalidValidator<ProductModel>(new Dictionary<string, string[]>
            {
                { nameof(ProductModel.Name), new string[] { Constants.ProductNameTooLong } }
            });

            var controller = new ProductsController(
                _mockLogger.Object,
                _mockRepository.Object,
                mockInvalidValidator.Object);

            ControllerHelpers.SetupHttpContext(controller);

            // Act
            var result = await controller.CreateProduct(invalidProduct);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);

            var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Errors.Should().NotBeNull();

            errorResponse.Errors.Should().ContainKey(nameof(ProductModel.Name));
            errorResponse.Errors[nameof(ProductModel.Name)].Should().Contain(Constants.ProductNameTooLong);

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<ProductModel>()),
                Times.Never);

            mockInvalidValidator.Verify(
                v => v.Validate(It.Is<ProductModel>(p => p == invalidProduct)),
                Times.Once);
        }

        [Fact]
        public async Task CreateProduct_WithDescriptionExceedingMaxLength_Returns400BadRequest()
        {
            // Arrange
            var invalidProduct = ProductTestHelpers.CreateTestProduct();
            invalidProduct.Description = new string('A', Constants.ProductDescriptionMaxLength + 1);

            var mockInvalidValidator = ValidationHelpers.CreateInvalidValidator<ProductModel>(new Dictionary<string, string[]>
            {
                { nameof(ProductModel.Description), new string[] { Constants.ProductDescriptionTooLong } }
            });

            var controller = new ProductsController(
                _mockLogger.Object,
                _mockRepository.Object,
                mockInvalidValidator.Object);

            ControllerHelpers.SetupHttpContext(controller);

            // Act
            var result = await controller.CreateProduct(invalidProduct);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);

            var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Errors.Should().NotBeNull();

            errorResponse.Errors.Should().ContainKey(nameof(ProductModel.Description));
            errorResponse.Errors[nameof(ProductModel.Description)].Should().Contain(Constants.ProductDescriptionTooLong);

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<ProductModel>()),
                Times.Never);

            mockInvalidValidator.Verify(
                v => v.Validate(It.Is<ProductModel>(p => p == invalidProduct)),
                Times.Once);
        }

        [Fact]
        public async Task CreateProduct_WhenRepositoryThrowsException_Returns500InternalServerError()
        {
            // Arrange
            var product = ProductTestHelpers.CreateTestProduct();
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ProductModel>()))
                           .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.CreateProduct(product);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeOfType<ErrorResponse>();

            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<ProductModel>()),
                Times.Once);
        }
    }

    public class UpdateProductTests : ProductsControllerTests
    {
        [Fact]
        public async Task UpdateProduct_WithValidProduct_Returns204NoContent()
        {
            // Arrange
            var existingProductId = Guid.NewGuid();
            var updatedProduct = ProductTestHelpers.CreateTestProduct(existingProductId);

            _mockRepository.Setup(r => r.UpdateAsync(existingProductId, It.IsAny<ProductModel>()))
                           .ReturnsAsync(updatedProduct);

            // Act
            var result = await _controller.UpdateProduct(existingProductId, updatedProduct);

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
            noContentResult!.StatusCode.Should().Be(204);

            _mockRepository.Verify(
                r => r.UpdateAsync(existingProductId, It.Is<ProductModel>(p => p == updatedProduct)),
                Times.Once);

            _mockValidator.Verify(
                v => v.Validate(It.Is<ProductModel>(p => p == updatedProduct)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_WhenProductDoesNotExist_Returns404NotFound()
        {
            // Arrange
            var nonExistentProductId = Guid.NewGuid();
            var updatedProduct = ProductTestHelpers.CreateTestProduct();

            _mockRepository.Setup(r => r.UpdateAsync(nonExistentProductId, It.IsAny<ProductModel>()))
                           .ReturnsAsync((ProductModel?)null);

            // Act
            var result = await _controller.UpdateProduct(nonExistentProductId, updatedProduct);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(404);

            _mockRepository.Verify(
                r => r.UpdateAsync(nonExistentProductId, It.IsAny<ProductModel>()),
                Times.Once);

            _mockValidator.Verify(
                v => v.Validate(It.Is<ProductModel>(p => p == updatedProduct)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_WithInvalidProduct_Returns400BadRequest()
        {
            // Arrange
            var existingProductId = Guid.NewGuid();
            var invalidProduct = ProductTestHelpers.CreateInvalidProduct();

            var mockInvalidValidator = ValidationHelpers.CreateInvalidValidator<ProductModel>(new Dictionary<string, string[]>
            {
                { nameof(ProductModel.Name), new string[] { Constants.ProductNameRequired } },
                { nameof(ProductModel.Description), new[] { Constants.ProductDescriptionRequired } },
                { nameof(ProductModel.Stock), new[] { Constants.ProductStockMustBeNonNegative } },
                { nameof(ProductModel.Price), new[] { Constants.ProductPriceMustBeGreaterThanZero } }
            });

            var controller = new ProductsController(
                _mockLogger.Object,
                _mockRepository.Object,
                mockInvalidValidator.Object);
            ControllerHelpers.SetupHttpContext(controller);

            // Act
            var result = await controller.UpdateProduct(existingProductId, invalidProduct);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);

            var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Errors.Should().NotBeNull();

            // Name
            errorResponse.Errors.Should().ContainKey(nameof(ProductModel.Name));
            errorResponse.Errors[nameof(ProductModel.Name)].Should().Contain(Constants.ProductNameRequired);

            // Description
            errorResponse.Errors.Should().ContainKey(nameof(ProductModel.Description));
            errorResponse.Errors[nameof(ProductModel.Description)].Should().Contain(Constants.ProductDescriptionRequired);

            // Stock
            errorResponse.Errors.Should().ContainKey(nameof(ProductModel.Stock));
            errorResponse.Errors[nameof(ProductModel.Stock)].Should().Contain(Constants.ProductStockMustBeNonNegative);

            // Price
            errorResponse.Errors.Should().ContainKey(nameof(ProductModel.Price));
            errorResponse.Errors[nameof(ProductModel.Price)].Should().Contain(Constants.ProductPriceMustBeGreaterThanZero);

            _mockRepository.Verify(
                r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<ProductModel>()),
                Times.Never);

            mockInvalidValidator.Verify(
                v => v.Validate(It.Is<ProductModel>(p => p == invalidProduct)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_WhenRepositoryThrowsException_Returns500InternalServerError()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var updatedProduct = ProductTestHelpers.CreateTestProduct();

            _mockRepository.Setup(r => r.UpdateAsync(productId, It.IsAny<ProductModel>()))
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateProduct(productId, updatedProduct);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeOfType<ErrorResponse>();

            _mockRepository.Verify(
                r => r.UpdateAsync(productId, It.IsAny<ProductModel>()),
                Times.Once);
        }
    }

    public class GetProductsTests : ProductsControllerTests
    {
        [Fact]
        public async Task GetProducts_Returns200OkWithListOfProducts()
        {
            // Arrange
            var products = ProductTestHelpers.CreateTestProducts(3);

            _mockRepository.Setup(r => r.GetAllAsync())
                           .ReturnsAsync(products);

            // Act
            var result = await _controller.GetProducts();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(products);

            _mockRepository.Verify(
                r => r.GetAllAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetProducts_WhenNoProductsExist_Returns200OkWithEmptyList()
        {
            // Arrange
            var emptyProducts = new List<ProductModel>();

            _mockRepository.Setup(r => r.GetAllAsync())
                           .ReturnsAsync(emptyProducts);

            // Act
            var result = await _controller.GetProducts();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(emptyProducts);

            _mockRepository.Verify(
                r => r.GetAllAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetProducts_WhenRepositoryThrowsException_Returns500InternalServerError()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetProducts();

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeOfType<ErrorResponse>();

            _mockRepository.Verify(
                r => r.GetAllAsync(),
                Times.Once);
        }
    }

    public class GetProductByIdTests : ProductsControllerTests
    {
        [Fact]
        public async Task GetProduct_WithValidId_Returns200OkWithProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = ProductTestHelpers.CreateTestProduct(productId);
            _mockRepository.Setup(r => r.GetByIdAsync(productId))
                           .ReturnsAsync(product);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(product);

            _mockRepository.Verify(
                r => r.GetByIdAsync(productId),
                Times.Once);
        }

        [Fact]
        public async Task GetProduct_WhenProductDoesNotExist_Returns404NotFound()
        {
            // Arrange
            var nonExistentProductId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(nonExistentProductId))
                           .ReturnsAsync((ProductModel?)null);

            // Act
            var result = await _controller.GetProduct(nonExistentProductId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult!.StatusCode.Should().Be(404);

            _mockRepository.Verify(
                r => r.GetByIdAsync(nonExistentProductId),
                Times.Once);
        }

        [Fact]
        public async Task GetProduct_WhenRepositoryThrowsException_Returns500InternalServerError()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(productId))
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeOfType<ErrorResponse>();

            _mockRepository.Verify(
                r => r.GetByIdAsync(productId),
                Times.Once);
        }
    }

    public class DeleteProductTests : ProductsControllerTests
    {
        [Fact]
        public async Task DeleteProduct_WhenProductExists_Returns204NoContent()
        {
            // Arrange
            var existingProductId = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteAsync(existingProductId))
                           .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteProduct(existingProductId);

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
            noContentResult!.StatusCode.Should().Be(204);

            _mockRepository.Verify(
                r => r.DeleteAsync(existingProductId),
                Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_WhenProductDoesNotExist_Returns404NotFound()
        {
            // Arrange
            var nonExistentProductId = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteAsync(nonExistentProductId))
                           .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteProduct(nonExistentProductId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult!.StatusCode.Should().Be(404);

            _mockRepository.Verify(
                r => r.DeleteAsync(nonExistentProductId),
                Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_WhenRepositoryThrowsException_Returns500InternalServerError()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteAsync(productId))
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteProduct(productId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeOfType<ErrorResponse>();

            _mockRepository.Verify(
                r => r.DeleteAsync(productId),
                Times.Once);
        }
    }
}