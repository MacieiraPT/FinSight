using FinSightPro.Application.DTOs;
using FinSightPro.Application.Interfaces;
using FinSightPro.Application.Services;
using FinSightPro.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinSightPro.Tests.Services;

public class ExpenseServiceTests
{
    private readonly Mock<IExpenseRepository> _repo = new();
    private readonly Mock<ICategoryRepository> _cats = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly ExpenseService _sut;

    public ExpenseServiceTests()
    {
        _sut = new ExpenseService(_repo.Object, _cats.Object, _uow.Object, NullLogger<ExpenseService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_RejectsNegativeAmount()
    {
        var result = await _sut.CreateAsync("u1", new ExpenseCreateDto { Amount = -1, Date = DateTime.UtcNow, Description = "x", CategoryId = 1 });
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_RejectsFutureDate()
    {
        var result = await _sut.CreateAsync("u1", new ExpenseCreateDto { Amount = 5, Date = DateTime.UtcNow.AddDays(1), Description = "x", CategoryId = 1 });
        Assert.False(result.Success);
        Assert.Contains("futura", result.Error!);
    }

    [Fact]
    public async Task CreateAsync_RejectsInvalidCategory()
    {
        _cats.Setup(c => c.GetByIdAsync(1, "u1", It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        var result = await _sut.CreateAsync("u1", new ExpenseCreateDto { Amount = 5, Date = DateTime.UtcNow, Description = "x", CategoryId = 1 });
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_PersistsValidExpense()
    {
        _cats.Setup(c => c.GetByIdAsync(1, "u1", It.IsAny<CancellationToken>())).ReturnsAsync(new Category { Id = 1, UserId = "u1", Name = "Comida" });
        _repo.Setup(r => r.AddAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .Callback<Expense, CancellationToken>((e, _) => e.Id = 7)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync("u1", new ExpenseCreateDto
        {
            Amount = 12.34m,
            Date = DateTime.UtcNow.Date,
            Description = "Almoço",
            CategoryId = 1
        });

        Assert.True(result.Success);
        Assert.Equal(7, result.Value);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
