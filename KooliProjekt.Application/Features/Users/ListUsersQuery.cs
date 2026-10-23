using KooliProjekt.Application.Dto;
using KooliProjekt.Application.Infrastructure.Paging;
using KooliProjekt.Application.Infrastructure.Results;
using MediatR;
namespace KooliProjekt.Application.Features.Users;
public class ListUsersQuery : IRequest<OperationResult<PagedResult<UserListItemDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
