using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.Members.Commands.DeleteMemberCommand;

public record DeleteMemberCommand(int Id) : IRequest<Result<bool>>;
