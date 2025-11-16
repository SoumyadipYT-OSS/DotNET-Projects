import { Component, OnInit, OnDestroy } from '@angular/core';
import { TodoService } from '../../service/todo.service';
import { Todo, TodoPriority } from '../../model/todo.model';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-todo-list',
  standalone: false,
  templateUrl: './todo-list.component.html',
  styleUrls: ['./todo-list.component.css']
})
export class TodoListComponent implements OnInit, OnDestroy {
  todos: Todo[] = [];
  filteredTodos: Todo[] = [];
  loading: boolean = false;
  errorMessage: string = '';
  filterBy: 'all' | 'active' | 'completed' = 'all';
  selectedCategory: string = '';
  categories: string[] = [];
  TodoPriority = TodoPriority;

  private destroy$ = new Subject<void>();

  constructor(private todoService: TodoService) { }

  ngOnInit(): void {
    this.loadTodos();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadTodos(): void {
    this.loading = true;
    this.errorMessage = '';

    this.todoService.getTodos()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data: Todo[]) => {
          this.todos = data;
          this.extractCategories();
          this.applyFilters();
          this.loading = false;
        },
        error: (error: any) => {
          this.errorMessage = 'Failed to load todos. Please try again.';
          console.error('Error loading todos:', error);
          this.loading = false;
        }
      });
  }

  applyFilters(): void {
    let filtered = this.todos;

    // Filter by completion status
    if (this.filterBy === 'active') {
      filtered = filtered.filter(t => !t.isComplete);
    } else if (this.filterBy === 'completed') {
      filtered = filtered.filter(t => t.isComplete);
    }

    // Filter by category
    if (this.selectedCategory) {
      filtered = filtered.filter(t => t.category === this.selectedCategory);
    }

    this.filteredTodos = filtered;
  }

  extractCategories(): void {
    const categorySet = new Set(
      this.todos
        .filter(t => t.category && t.category.trim() !== '')
        .map(t => t.category)
    );
    this.categories = Array.from(categorySet).sort();
  }

  toggleCompletion(todo: Todo): void {
    this.todoService.toggleTodoCompletion(todo.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          todo.isComplete = !todo.isComplete;
          this.applyFilters();
        },
        error: (error: any) => {
          this.errorMessage = 'Failed to update todo completion status.';
          console.error('Error toggling todo:', error);
        }
      });
  }

  deleteTodo(id: number): void {
    if (confirm('Are you sure you want to delete this todo?')) {
      this.todoService.deleteTodo(id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.todos = this.todos.filter(t => t.id !== id);
            this.extractCategories();
            this.applyFilters();
          },
          error: (error: any) => {
            this.errorMessage = 'Failed to delete todo.';
            console.error('Error deleting todo:', error);
          }
        });
    }
  }

  getPriorityClass(priority: TodoPriority): string {
    switch (priority) {
      case TodoPriority.High:
        return 'danger';
      case TodoPriority.Medium:
        return 'warning';
      case TodoPriority.Low:
        return 'info';
      default:
        return 'secondary';
    }
  }

  getPriorityText(priority: TodoPriority): string {
    return TodoPriority[priority];
  }

  onFilterChange(filter: 'all' | 'active' | 'completed'): void {
    this.filterBy = filter;
    this.applyFilters();
  }

  onCategoryChange(category: string): void {
    this.selectedCategory = category;
    this.applyFilters();
  }
}
