using System.Collections.Generic;
namespace KooliProjekt.Application.Dto;
public class UserDto : UserListItemDto
{
public IList<TodoListItemDto> TodoLists { get; set; } = new List<TodoListItemDto>();
}
