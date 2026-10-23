using System.Collections.Generic;
namespace KooliProjekt.Application.Dto;
public class TodoItemDto
{
public int Id { get; set; }
public string Title { get; set; }
public bool IsCompleted { get; set; }
public int TodoListId { get; set; }
public string TodoListTitle { get; set; }
public string UserName { get; set; }
}
