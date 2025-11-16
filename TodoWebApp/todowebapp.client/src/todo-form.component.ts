import { Component, OnInit, OnDestroy, Output, EventEmitter, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TodoService } from '../service/todo.service';
import { Todo, CreateTodoDto, TodoPriority, UpdateTodoDto } from '../model/todo.model';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-todo-form',
  templateUrl: './todo-form.component.html',
  styleUrls: ['./todo-form.component.css']
})
export class TodoFormComponent implements OnInit, OnDestroy {
  @Input() todoToEdit: Todo | null = null;
  @Output() todoCreated = new EventEmitter<Todo>();
  @Output() todoUpdated = new EventEmitter<void>();
  @Output() formClosed = new EventEmitter<void>();

  todoForm!: FormGroup;
  isSubmitting: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  isEditMode: boolean = false;
  TodoPriority = TodoPriority;
  priorities = Object.keys(TodoPriority).filter(k => isNaN(Number(k)));

  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private todoService: TodoService
  ) { }

  ngOnInit(): void {
    this.initializeForm();
    if (this.todoToEdit) {
      this.isEditMode = true;
      this.populateForm();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  initializeForm(): void {
    this.todoForm = this.formBuilder.group({
      title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(2000)]],
      priority: [TodoPriority.Medium, Validators.required],
      dueDate: [''],
      category: ['', [Validators.maxLength(100)]],
      tags: ['', [Validators.maxLength(500)]]
    });
  }

  populateForm(): void {
    if (this.todoToEdit) {
      const dueDate = this.todoToEdit.dueDate
        ? new Date(this.todoToEdit.dueDate).toISOString().split('T')[0]
        : '';

      this.todoForm.patchValue({
        title: this.todoToEdit.title,
        description: this.todoToEdit.description,
        priority: this.todoToEdit.priority,
        dueDate: dueDate,
        category: this.todoToEdit.category,
        tags: this.todoToEdit.tags
      });
    }
  }

  onSubmit(): void {
    if (this.todoForm.invalid) {
      this.errorMessage = 'Please fill in all required fields correctly.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (this.isEditMode && this.todoToEdit) {
      this.updateTodo();
    } else {
      this.createTodo();
    }
  }

  createTodo(): void {
    const createTodoDto: CreateTodoDto = this.todoForm.value;

    this.todoService.createTodo(createTodoDto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (createdTodo: Todo) => {
          this.successMessage = 'Todo created successfully!';
          this.todoForm.reset({ priority: TodoPriority.Medium });
          this.todoCreated.emit(createdTodo);
          this.isSubmitting = false;

          setTimeout(() => {
            this.successMessage = '';
          }, 3000);
        },
        error: (error) => {
          this.errorMessage = 'Failed to create todo. Please try again.';
          console.error('Error creating todo:', error);
          this.isSubmitting = false;
        }
      });
  }

  updateTodo(): void {
    if (!this.todoToEdit) return;

    const updateTodoDto: UpdateTodoDto = this.todoForm.value;

    this.todoService.updateTodo(this.todoToEdit.id, updateTodoDto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.successMessage = 'Todo updated successfully!';
          this.todoUpdated.emit();
          this.isSubmitting = false;

          setTimeout(() => {
            this.closeForm();
          }, 2000);
        },
        error: (error) => {
          this.errorMessage = 'Failed to update todo. Please try again.';
          console.error('Error updating todo:', error);
          this.isSubmitting = false;
        }
      });
  }

  closeForm(): void {
    this.todoForm.reset({ priority: TodoPriority.Medium });
    this.errorMessage = '';
    this.successMessage = '';
    this.isEditMode = false;
    this.todoToEdit = null;
    this.formClosed.emit();
  }

  get title() {
    return this.todoForm.get('title');
  }

  get description() {
    return this.todoForm.get('description');
  }

  get priority() {
    return this.todoForm.get('priority');
  }

  get dueDate() {
    return this.todoForm.get('dueDate');
  }

  get category() {
    return this.todoForm.get('category');
  }

  get tags() {
    return this.todoForm.get('tags');
  }
}
