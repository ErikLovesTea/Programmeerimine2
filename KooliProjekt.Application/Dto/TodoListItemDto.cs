using System.Collections.Generic;
namespace KooliProjekt.Application.Dto;
public class TodoListItemDto
{
public int Id { get; set; }
public string Title { get; set; }
public int UserId { get; set; }
public string UserName { get; set; }
public int ItemCount { get; set; }
public int CompletedItemCount { get; set; }
}
