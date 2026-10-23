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
public class ListTodoItemsQueryHandler : IRequestHandler<ListTodoItemsQuery, OperationResult<PagedResult<TodoItemDto>>>
{
    private readonly ApplicationDbContext _dbContext;
    public ListTodoItemsQueryHandler(ApplicationDbContext dbContext) => _dbContext = dbContext;
    public async Task<OperationResult<PagedResult<TodoItemDto>>> Handle(ListTodoItemsQuery request, CancellationToken cancellationToken)
    {
        var value = await _dbContext.TodoItems.AsNoTracking()
            .OrderBy(item => item.Id)
            .Select(item => new TodoItemDto
            {
                Id = item.Id, Title = item.Title, IsCompleted = item.IsCompleted, TodoListId = item.TodoListId, TodoListTitle = item.TodoList.Title, UserName = item.TodoList.User.UserName
            })
            .GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        return new OperationResult<PagedResult<TodoItemDto>>(value);
    }
}
