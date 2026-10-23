using System.Collections.Generic;
namespace KooliProjekt.Application.Dto;
public class TodoListDto : TodoListItemDto
{
public IList<TodoItemDto> Items { get; set; } = new List<TodoItemDto>();
}
