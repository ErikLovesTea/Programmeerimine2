using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.Application.Data;
using KooliProjekt.Application.Dto;
using KooliProjekt.Application.Infrastructure.Paging;
using KooliProjekt.Application.Infrastructure.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace KooliProjekt.Application.Features.Users;
public class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, OperationResult<PagedResult<UserListItemDto>>>
{
    private readonly ApplicationDbContext _dbContext;
    public ListUsersQueryHandler(ApplicationDbContext dbContext) => _dbContext = dbContext;
    public async Task<OperationResult<PagedResult<UserListItemDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var value = await _dbContext.Users.AsNoTracking()
            .OrderBy(user => user.Id)
            .Select(user => new UserListItemDto
            {
                Id = user.Id, UserName = user.UserName, TodoListCount = _dbContext.TodoLists.Count(list => list.UserId == user.Id)
            })
            .GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        return new OperationResult<PagedResult<UserListItemDto>>(value);
    }
}
