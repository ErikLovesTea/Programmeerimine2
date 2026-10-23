using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.Application.Data;
using KooliProjekt.Application.Dto;
using KooliProjekt.Application.Infrastructure.Paging;
using KooliProjekt.Application.Infrastructure.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace KooliProjekt.Application.Features.TodoItems;
public class GetTodoItemQueryHandler : IRequestHandler<GetTodoItemQuery, OperationResult<TodoItemDto>>
{
    private readonly ApplicationDbContext _dbContext;
    public GetTodoItemQueryHandler(ApplicationDbContext dbContext) => _dbContext = dbContext;
    public async Task<OperationResult<TodoItemDto>> Handle(GetTodoItemQuery request, CancellationToken cancellationToken)
    {
        var value = await _dbContext.TodoItems.AsNoTracking()
            .Where(item => item.Id == request.Id)
            .Select(item => new TodoItemDto
            {
                Id = item.Id, Title = item.Title, IsCompleted = item.IsCompleted, TodoListId = item.TodoListId, TodoListTitle = item.TodoList.Title, UserName = item.TodoList.User.UserName
            })
            .FirstOrDefaultAsync(cancellationToken);
        return new OperationResult<TodoItemDto>(value);
    }
}
