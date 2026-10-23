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
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, OperationResult<UserDto>>
{
    private readonly ApplicationDbContext _dbContext;
    public GetUserQueryHandler(ApplicationDbContext dbContext) => _dbContext = dbContext;
    public async Task<OperationResult<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var value = await _dbContext.Users.AsNoTracking()
            .Where(user => user.Id == request.Id)
            .Select(user => new UserDto
            {
                Id = user.Id, UserName = user.UserName, TodoListCount = _dbContext.TodoLists.Count(list => list.UserId == user.Id), TodoLists = _dbContext.TodoLists.Where(list => list.UserId == user.Id).OrderBy(list => list.Id).Select(list => new TodoListItemDto { Id = list.Id, Title = list.Title, UserId = list.UserId, UserName = list.User.UserName, ItemCount = list.Items.Count, CompletedItemCount = list.Items.Count(item => item.IsCompleted) }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
        return new OperationResult<UserDto>(value);
    }
}
