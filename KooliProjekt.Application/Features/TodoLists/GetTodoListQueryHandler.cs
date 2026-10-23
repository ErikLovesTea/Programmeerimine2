using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.Application.Data;
using KooliProjekt.Application.Dto;
using KooliProjekt.Application.Infrastructure.Paging;
using KooliProjekt.Application.Infrastructure.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace KooliProjekt.Application.Features.TodoLists;
public class GetTodoListQueryHandler : IRequestHandler<GetTodoListQuery, OperationResult<TodoListDto>>
{
    private readonly ApplicationDbContext _dbContext;
    public GetTodoListQueryHandler(ApplicationDbContext dbContext) => _dbContext = dbContext;
    public async Task<OperationResult<TodoListDto>> Handle(GetTodoListQuery request, CancellationToken cancellationToken)
    {
        var value = await _dbContext.TodoLists.AsNoTracking()
            .Where(list => list.Id == request.Id)
            .Select(list => new TodoListDto
            {
                Id = list.Id, Title = list.Title, UserId = list.UserId, UserName = list.User.UserName, ItemCount = list.Items.Count, CompletedItemCount = list.Items.Count(item => item.IsCompleted), Items = list.Items.OrderBy(item => item.Id).Select(item => new TodoItemDto { Id = item.Id, Title = item.Title, IsCompleted = item.IsCompleted, TodoListId = item.TodoListId, TodoListTitle = item.TodoList.Title, UserName = item.TodoList.User.UserName }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
        return new OperationResult<TodoListDto>(value);
    }
}
