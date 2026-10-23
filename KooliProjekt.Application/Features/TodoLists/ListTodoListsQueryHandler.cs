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
public class ListTodoListsQueryHandler : IRequestHandler<ListTodoListsQuery, OperationResult<PagedResult<TodoListItemDto>>>
{
    private readonly ApplicationDbContext _dbContext;
    public ListTodoListsQueryHandler(ApplicationDbContext dbContext) => _dbContext = dbContext;
    public async Task<OperationResult<PagedResult<TodoListItemDto>>> Handle(ListTodoListsQuery request, CancellationToken cancellationToken)
    {
        var value = await _dbContext.TodoLists.AsNoTracking()
            .OrderBy(list => list.Id)
            .Select(list => new TodoListItemDto
            {
                Id = list.Id, Title = list.Title, UserId = list.UserId, UserName = list.User.UserName, ItemCount = list.Items.Count, CompletedItemCount = list.Items.Count(item => item.IsCompleted)
            })
            .GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        return new OperationResult<PagedResult<TodoListItemDto>>(value);
    }
}
