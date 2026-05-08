using FinSightPro.Application.DTOs;
using FinSightPro.Application.Interfaces;
using FinSightPro.Application.Services;
using FinSightPro.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinSightPro.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(_repo.Object, _uow.Object, NullLogger<CategoryService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_RejectsEmptyName()
    {
        var result = await _sut.CreateAsync("u1", new CategoryCreateDto { Name = "  " });
        Assert.False(result.Success);
        Assert.Equal("O nome é obrigatório.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateName()
    {
        _repo.Setup(r => r.NameExistsAsync("u1", "Comida", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.CreateAsync("u1", new CategoryCreateDto { Name = "Comida" });

        Assert.False(result.Success);
        Assert.Contains("já existe", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_PersistsAndReturnsId()
    {
        _repo.Setup(r => r.NameExistsAsync("u1", "Comida", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Callback<Category, CancellationToken>((c, _) => c.Id = 42)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync("u1", new CategoryCreateDto { Name = "Comida", Icon = "fa-utensils", Color = "#ff0000" });

        Assert.True(result.Success);
        Assert.Equal(42, result.Value);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_BlockedWhenHasExpenses()
    {
        var existing = new Category { Id = 1, UserId = "u1", Name = "Comida" };
        _repo.Setup(r => r.GetByIdAsync(1, "u1", It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repo.Setup(r => r.HasExpensesAsync(1, "u1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.DeleteAsync("u1", 1);

        Assert.False(result.Success);
        Assert.Contains("despesas", result.Error!);
    }
}
