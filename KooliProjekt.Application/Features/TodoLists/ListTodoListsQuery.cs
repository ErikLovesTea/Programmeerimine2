using KooliProjekt.Application.Dto;
using KooliProjekt.Application.Infrastructure.Paging;
using KooliProjekt.Application.Infrastructure.Results;
using MediatR;
namespace KooliProjekt.Application.Features.TodoLists;
public class ListTodoListsQuery : IRequest<OperationResult<PagedResult<TodoListItemDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
