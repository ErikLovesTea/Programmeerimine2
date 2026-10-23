using KooliProjekt.Application.Dto;
using KooliProjekt.Application.Infrastructure.Paging;
using KooliProjekt.Application.Infrastructure.Results;
using MediatR;
namespace KooliProjekt.Application.Features.TodoItems;
public class GetTodoItemQuery : IRequest<OperationResult<TodoItemDto>>
{
    public int Id { get; set; }
}
