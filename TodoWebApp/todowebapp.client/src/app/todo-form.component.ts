import { Component, OnInit, OnDestroy, Output, EventEmitter, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TodoService } from '../../service/todo.service';
import { Todo, CreateTodoDto, TodoPriority, UpdateTodoDto } from '../../model/todo.model';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-todo-form',
  standalone: false,
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

  private todayString(): string {
    const d = new Date();
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  initializeForm(): void {
    this.todoForm = this.formBuilder.group({
      title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(2000)]],
      priority: [TodoPriority.Medium, Validators.required],
      dueDate: [this.todayString()],
      category: ['', [Validators.maxLength(100)]],
      tags: ['', [Validators.maxLength(500)]]
    });
  }

  populateForm(): void {
    if (this.todoToEdit) {
      const dueDate = this.todoToEdit.dueDate
        ? new Date(this.todoToEdit.dueDate).toISOString().split('T')[0]
        : this.todayString();

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

  // Map enum name from priorities list to its numeric value for select option
  getPriorityValue(priorityName: string): number {
    return (TodoPriority as any)[priorityName] as number;
  }

  onSubmit(): void {
    if (this.todoForm.invalid) {
      this.errorMessage = 'Please fill in all required fields correctly.';
      Object.keys(this.todoForm.controls).forEach(key => {
        const control = this.todoForm.get(key);
        if (control?.invalid) {
          console.error(`Invalid field: ${key}`, control.errors);
        }
      });
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

  private buildCreateDto(): CreateTodoDto {
    const v = this.todoForm.value;
    
    // Ensure priority is a number
    const priorityValue = typeof v.priority === 'string' ? parseInt(v.priority, 10) : v.priority;
    
    const dto: CreateTodoDto = {
      title: v.title?.trim(),
      description: v.description?.trim() || undefined,
      priority: priorityValue,
      dueDate: v.dueDate ? new Date(v.dueDate + 'T00:00:00Z') : new Date(),
      category: v.category?.trim() || undefined,
      tags: v.tags?.trim() || undefined
    };
    
    console.log('CreateTodoDto:', dto);
    return dto;
  }

  private buildUpdateDto(): UpdateTodoDto {
    const v = this.todoForm.value;
    
    // Ensure priority is a number
    const priorityValue = typeof v.priority === 'string' ? parseInt(v.priority, 10) : v.priority;
    
    const dto: UpdateTodoDto = {
      title: v.title?.trim(),
      description: v.description?.trim() || undefined,
      priority: priorityValue,
      dueDate: v.dueDate ? new Date(v.dueDate + 'T00:00:00Z') : undefined,
      category: v.category?.trim() || undefined,
      tags: v.tags?.trim() || undefined
    };
    
    console.log('UpdateTodoDto:', dto);
    return dto;
  }

  createTodo(): void {
    const createTodoDto = this.buildCreateDto();

    this.todoService.createTodo(createTodoDto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (createdTodo: Todo) => {
          console.log('Todo created successfully:', createdTodo);
          this.successMessage = 'Todo created successfully!';
          this.todoForm.reset({ priority: TodoPriority.Medium, dueDate: this.todayString() });
          this.todoCreated.emit(createdTodo);
          this.isSubmitting = false;

          setTimeout(() => {
            this.successMessage = '';
          }, 3000);
        },
        error: (error: any) => {
          console.error('Error creating todo:', error);
          console.error('Error status:', error.status);
          console.error('Error message:', error.message);
          console.error('Error details:', error.error);
          
          this.errorMessage = error.error?.message || error.message || 'Failed to create todo. Please try again.';
          this.isSubmitting = false;
        }
      });
  }

  updateTodo(): void {
    if (!this.todoToEdit) return;

    const updateTodoDto = this.buildUpdateDto();

    this.todoService.updateTodo(this.todoToEdit.id, updateTodoDto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          console.log('Todo updated successfully');
          this.successMessage = 'Todo updated successfully!';
          this.todoUpdated.emit();
          this.isSubmitting = false;

          setTimeout(() => {
            this.closeForm();
          }, 2000);
        },
        error: (error: any) => {
          console.error('Error updating todo:', error);
          console.error('Error status:', error.status);
          console.error('Error message:', error.message);
          console.error('Error details:', error.error);
          
          this.errorMessage = error.error?.message || error.message || 'Failed to update todo. Please try again.';
          this.isSubmitting = false;
        }
      });
  }

  closeForm(): void {
    this.todoForm.reset({ priority: TodoPriority.Medium, dueDate: this.todayString() });
    this.errorMessage = '';
    this.successMessage = '';
    this.isEditMode = false;
    this.todoToEdit = null;
    this.formClosed.emit();
  }

  get title() { return this.todoForm.get('title'); }
  get description() { return this.todoForm.get('description'); }
  get priority() { return this.todoForm.get('priority'); }
  get dueDate() { return this.todoForm.get('dueDate'); }
  get category() { return this.todoForm.get('category'); }
  get tags() { return this.todoForm.get('tags'); }
}
