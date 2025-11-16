using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoWebApp.Server.Data;
using TodoWebApp.Server.DTOs;
using TodoWebApp.Server.Models;

namespace TodoWebApp.Server.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly TodoDbContext _context;
        private readonly ILogger<TodoController> _logger;

        public TodoController(TodoDbContext context, ILogger<TodoController> logger) 
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodos(
                [FromQuery] bool? isComplete = null,
                [FromQuery] string? category = null,
                [FromQuery] int? priority = null) {

            try 
            {
                var query = _context.Todos.AsQueryable();

                // Apply filters
                if (isComplete.HasValue)
                    query = query.Where(t => t.IsComplete == isComplete.Value);

                if (!string.IsNullOrWhiteSpace(category))
                    query = query.Where(t => t.Category == category);

                if (priority.HasValue)
                    query = query.Where(t => (int)t.Priority == priority);

                var todos = await query.OrderByDescending(t => t.DueDate)
                                    .ThenByDescending(t => t.Priority)
                                    .ToListAsync();

                return Ok(todos);

            } catch (Exception ex) 
            {
                _logger.LogError(ex, "Error retrieving todos");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving todos");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> GetTodo(int id) 
        {
            try 
            {
                var todo = await _context.Todos.FindAsync(id);

                if (todo == null)
                    return NotFound($"Todo with id {id} not found");

                return Ok(todo);
            } 
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error retrieving todo with id {id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving todo");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Todo>> CreateTodo([FromBody] CreateTodoDto createTodoDto) 
        {
            try 
            {
                _logger.LogInformation("CreateTodo called with: Title={Title}, Priority={Priority}, DueDate={DueDate}", 
                    createTodoDto?.Title, createTodoDto?.Priority, createTodoDto?.DueDate);

                if (createTodoDto == null)
                {
                    _logger.LogWarning("CreateTodo received null DTO");
                    return BadRequest(new { message = "Request body is required" });
                }

                if (string.IsNullOrWhiteSpace(createTodoDto.Title))
                {
                    _logger.LogWarning("CreateTodo received empty title");
                    return BadRequest(new { message = "Title is required" });
                }

                var due = createTodoDto.DueDate.HasValue ? createTodoDto.DueDate.Value.Date : DateTime.UtcNow.Date;

                var todo = new Todo 
                {
                    Title = createTodoDto.Title.Trim(),
                    Description = createTodoDto.Description?.Trim() ?? string.Empty,
                    Priority = createTodoDto.Priority,
                    DueDate = due,
                    Category = createTodoDto.Category?.Trim() ?? string.Empty,
                    Tags = createTodoDto.Tags?.Trim() ?? string.Empty,
                    IsComplete = false,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                _logger.LogInformation("Adding todo to context: {Title}", todo.Title);
                _context.Todos.Add(todo);
                
                _logger.LogInformation("Saving changes to database");
                await _context.SaveChangesAsync();

                _logger.LogInformation("Todo created successfully with id {id}", todo.Id);
                return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
            } 
            catch (DbUpdateException dbEx) 
            {
                _logger.LogError(dbEx, "Database error creating todo");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Database error occurred while creating todo", details = dbEx.InnerException?.Message });
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error creating todo");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error creating todo", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] UpdateTodoDto updateTodoDto) 
        {
            try 
            {
                var todo = await _context.Todos.FindAsync(id);

                if (todo == null)
                    return NotFound($"Todo with id {id} not found.");

                if (!string.IsNullOrWhiteSpace(updateTodoDto.Title))
                    todo.Title = updateTodoDto.Title.Trim();

                if (!string.IsNullOrWhiteSpace(updateTodoDto.Description))
                    todo.Description = updateTodoDto.Description.Trim();

                if (updateTodoDto.Priority.HasValue)
                    todo.Priority = updateTodoDto.Priority.Value;

                if (updateTodoDto.DueDate.HasValue)
                    todo.DueDate = updateTodoDto.DueDate.Value.Date;

                if (updateTodoDto.Category != null)
                    todo.Category = updateTodoDto.Category.Trim();

                if (updateTodoDto.Tags != null)
                    todo.Tags = updateTodoDto.Tags.Trim();

                if (updateTodoDto.IsComplete.HasValue)
                    todo.IsComplete = updateTodoDto.IsComplete.Value;

                todo.LastModifiedDate = DateTime.UtcNow;

                _context.Todos.Update(todo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Todo with id {id} updated", id);
                return NoContent();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error updating todo with id {id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating todo");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id) 
        {
            try 
            {
                var todo = await _context.Todos.FindAsync(id);

                if (todo == null)
                    return NotFound($"Todo with id {id} not found");

                _context.Todos.Remove(todo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Todo with id {id} deleted", id);
                return NoContent();
            } 
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error deleting todo with id {id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting todo");
            }
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleTodoCompletion(int id) 
        {
            try 
            {
                var todo = await _context.Todos.FindAsync(id);

                if (todo == null)
                    return NotFound($"Todo with id {id} not found");

                todo.IsComplete = !todo.IsComplete;
                todo.LastModifiedDate = DateTime.UtcNow;

                _context.Todos.Update(todo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Todo with id {id} completion toggled", id);
                return NoContent();
            } 
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error toggling todo with id {id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error toggling todo");
            }
        }

    }
}
