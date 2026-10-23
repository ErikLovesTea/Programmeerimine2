using KooliProjekt.Application.Infrastructure.Results;
using Microsoft.AspNetCore.Mvc;
namespace KooliProjekt.WebAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Result<T>(OperationResult<T> result)
    {
        if (result == null) return NotFound();
        if (result.HasErrors) return BadRequest(result);
        if (result.Value == null) return NotFound();
        return Ok(result);
    }
}
