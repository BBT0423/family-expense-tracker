using Microsoft.AspNetCore.Identity;

using Moq;

using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.TransactionProposals.Commands.AcceptTransactionProposalCommand;
using SummaryYearSalary.Application.TransactionProposals.Commands.CreateTransactionProposalCommand;
using SummaryYearSalary.Application.TransactionProposals.Commands.RejectTransactionProposalCommand;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;
using SummaryYearSalary.Tests.Fixtures;

namespace SummaryYearSalary.Tests.Application.Handlers;

public class TransactionProposalHandlerTests : BaseTest
{
    private readonly Mock<ITransactionProposalRepository> _proposals = new();
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<UserManager<ApplicationUser>> _users = new(
        Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

    private static Category SystemCategory(TransactionType type = TransactionType.Expense) =>
        new() { Id = 1, Name = "อาหาร", Type = type, UserId = null };

    private static TransactionProposal PendingProposal() => new()
    {
        Id = 10, OwnerUserId = "owner", ProposedByUserId = "proposer", Type = TransactionType.Expense,
        Amount = 50m, CategoryId = 1, Status = TransactionProposalStatus.Pending, TransactionDate = DateTime.Today
    };

    // ---- Create ----

    private CreateTransactionProposalCommandHandler CreateHandler() =>
        new(_proposals.Object, _categories.Object, _users.Object, _notifications.Object, _uow.Object);

    private static CreateTransactionProposalCommand CreateCommand(string email = "o@x.com") =>
        new("proposer", email, TransactionType.Expense, 50m, 1, null, DateTime.Today);

    [Fact]
    public async Task Create_UnknownEmail_Returns404()
    {
        _users.Setup(u => u.FindByEmailAsync("o@x.com")).ReturnsAsync((ApplicationUser?)null);

        var result = await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task Create_ToSelf_Fails()
    {
        _users.Setup(u => u.FindByEmailAsync("o@x.com")).ReturnsAsync(new ApplicationUser { Id = "proposer" });

        var result = await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _proposals.Verify(r => r.AddAsync(It.IsAny<TransactionProposal>()), Times.Never);
    }

    [Fact]
    public async Task Create_PrivateCategory_Fails()
    {
        _users.Setup(u => u.FindByEmailAsync("o@x.com")).ReturnsAsync(new ApplicationUser { Id = "owner" });
        _categories.Setup(c => c.GetByIdAsync(1)).ReturnsAsync(new Category { Id = 1, Name = "ส่วนตัว", Type = TransactionType.Expense, UserId = "someone" });

        var result = await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _proposals.Verify(r => r.AddAsync(It.IsAny<TransactionProposal>()), Times.Never);
    }

    [Fact]
    public async Task Create_Valid_SavesPendingAndNotifiesOwner()
    {
        _users.Setup(u => u.FindByEmailAsync("o@x.com")).ReturnsAsync(new ApplicationUser { Id = "owner" });
        _users.Setup(u => u.FindByIdAsync("proposer")).ReturnsAsync(new ApplicationUser { Id = "proposer", FirstName = "A" });
        _categories.Setup(c => c.GetByIdAsync(1)).ReturnsAsync(SystemCategory());
        TransactionProposal? saved = null;
        _proposals.Setup(r => r.AddAsync(It.IsAny<TransactionProposal>()))
            .Callback<TransactionProposal>(p => saved = p)
            .ReturnsAsync((TransactionProposal p) => p);

        var result = await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("owner", saved!.OwnerUserId);
        Assert.Equal(TransactionProposalStatus.Pending, saved.Status);
        _notifications.Verify(n => n.NotifyAsync("owner", NotificationType.TransactionProposed,
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", 1, 10, false)]
    [InlineData("not-an-email", 1, 10, false)]
    [InlineData("o@x.com", 0, 10, false)]
    [InlineData("o@x.com", 1, 0, false)]
    [InlineData("o@x.com", 1, 10, true)]
    public void CreateValidator_Rules(string email, int categoryId, decimal amount, bool valid)
    {
        var command = new CreateTransactionProposalCommand("p", email, TransactionType.Expense, amount, categoryId, null, DateTime.Today);

        Assert.Equal(valid, new CreateTransactionProposalCommandValidator().Validate(command).IsValid);
    }

    // ---- Accept ----

    private AcceptTransactionProposalCommandHandler AcceptHandler() =>
        new(_proposals.Object, _transactions.Object, _categories.Object,
            _users.Object, _notifications.Object, _uow.Object);

    [Fact]
    public async Task Accept_ByNonOwner_Returns404()
    {
        _proposals.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(PendingProposal());

        var result = await AcceptHandler().Handle(new AcceptTransactionProposalCommand("intruder", 10, null), CancellationToken.None);

        Assert.Equal(404, result.StatusCode);
        _transactions.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task Accept_AlreadyDecided_Returns409()
    {
        var proposal = PendingProposal();
        proposal.Status = TransactionProposalStatus.Accepted;
        _proposals.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(proposal);

        var result = await AcceptHandler().Handle(new AcceptTransactionProposalCommand("owner", 10, null), CancellationToken.None);

        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task Accept_Valid_CreatesTransactionAttributedToProposerAndNotifies()
    {
        var proposal = PendingProposal();
        _proposals.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(proposal);
        _categories.Setup(c => c.GetByIdAsync(1)).ReturnsAsync(SystemCategory());
        Transaction? created = null;
        _transactions.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => created = t)
            .ReturnsAsync((Transaction t) => t);

        var result = await AcceptHandler().Handle(new AcceptTransactionProposalCommand("owner", 10, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("owner", created!.UserId);
        Assert.Equal("proposer", created.CreatedByUserId);
        Assert.Equal(TransactionProposalStatus.Accepted, proposal.Status);
        Assert.NotNull(proposal.DecidedAt);
        _notifications.Verify(n => n.NotifyAsync("proposer", NotificationType.TransactionProposalAccepted,
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---- Reject ----

    private RejectTransactionProposalCommandHandler RejectHandler() =>
        new(_proposals.Object, _users.Object, _notifications.Object, _uow.Object);

    [Fact]
    public async Task Reject_Valid_MarksRejectedCreatesNothingAndNotifiesProposer()
    {
        var proposal = PendingProposal();
        _proposals.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(proposal);

        var result = await RejectHandler().Handle(new RejectTransactionProposalCommand("owner", 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TransactionProposalStatus.Rejected, proposal.Status);
        _transactions.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Never);
        _notifications.Verify(n => n.NotifyAsync("proposer", NotificationType.TransactionProposalRejected,
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reject_ByNonOwner_Returns404()
    {
        _proposals.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(PendingProposal());

        var result = await RejectHandler().Handle(new RejectTransactionProposalCommand("intruder", 10), CancellationToken.None);

        Assert.Equal(404, result.StatusCode);
    }
}
