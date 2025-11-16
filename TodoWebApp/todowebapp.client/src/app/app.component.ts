import { Component, ViewChild } from '@angular/core';
import { Todo } from '../../model/todo.model';
import { TodoListComponent } from './todo-list.component';

@Component({
  selector: 'app-root',
  standalone: false,
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  @ViewChild('todoList') todoListComponent!: TodoListComponent;

  title = 'Todo Web App';
  currentYear = new Date().getFullYear();
  showForm: boolean = false;
  todoToEdit: Todo | null = null;

  onCreateTodo(todo: Todo): void {
    this.showForm = false;
    this.todoToEdit = null;
    // Refresh the todo list without page reload
    if (this.todoListComponent) {
      this.todoListComponent.loadTodos();
    }
  }

  onUpdateTodo(): void {
    this.showForm = false;
    this.todoToEdit = null;
    // Refresh the todo list without page reload
    if (this.todoListComponent) {
      this.todoListComponent.loadTodos();
    }
  }

  onFormClosed(): void {
    this.showForm = false;
    this.todoToEdit = null;
  }

  openCreateForm(): void {
    this.showForm = true;
    this.todoToEdit = null;
  }

  onImageError(evt: Event, fallbackUrl: string): void {
    const img = evt.target as HTMLImageElement;
    if (img && img.src !== fallbackUrl) {
      img.src = fallbackUrl;
    }
  }
}

