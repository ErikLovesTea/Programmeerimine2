using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.Application.Features.TodoLists;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace KooliProjekt.WebAPI.Controllers;
public class TodoListsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public TodoListsController(IMediator mediator) => _mediator = mediator;
    [HttpGet("get/{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        if (id <= 0) return NotFound();
        var result = await _mediator.Send(new GetTodoListQuery { Id = id }, cancellationToken);
        return Result(result);
    }
    [HttpGet("list/{page:int}/{pageSize:int}")]
    [HttpGet("list")]
    public async Task<IActionResult> List(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListTodoListsQuery { Page = page, PageSize = pageSize }, cancellationToken);
        return Result(result);
    }
}
