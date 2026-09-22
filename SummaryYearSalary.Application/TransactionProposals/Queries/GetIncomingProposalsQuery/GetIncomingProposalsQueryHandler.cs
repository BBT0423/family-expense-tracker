using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.TransactionProposals.Responses;
using SummaryYearSalary.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Application.TransactionProposals.Queries.GetIncomingProposalsQuery;

public class GetIncomingProposalsQueryHandler(
    ITransactionProposalRepository proposalRepository,
    UserManager<ApplicationUser> userManager
) : IRequestHandler<GetIncomingProposalsQuery, Result<IEnumerable<TransactionProposalResponse>>>
{
    private readonly ITransactionProposalRepository _proposalRepository = proposalRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<IEnumerable<TransactionProposalResponse>>> Handle(GetIncomingProposalsQuery request, CancellationToken cancellationToken)
    {
        var proposals = await _proposalRepository.GetPendingByOwnerAsync(request.UserId);

        var names = new Dictionary<string, string>();
        var responses = new List<TransactionProposalResponse>();
        foreach (var p in proposals)
        {
            if (!names.TryGetValue(p.ProposedByUserId, out var name))
            {
                name = UserDisplayName.Of(await _userManager.FindByIdAsync(p.ProposedByUserId));
                names[p.ProposedByUserId] = name;
            }

            responses.Add(new TransactionProposalResponse(
                p.Id, name, p.Type.ToString(), p.Amount,
                p.CategoryId, p.Category.Name, p.Category.Icon, p.Category.Color,
                p.Note, p.TransactionDate, p.CreatedAt));
        }

        return Result.Success<IEnumerable<TransactionProposalResponse>>(responses);
    }
}
