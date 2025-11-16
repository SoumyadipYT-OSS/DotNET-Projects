import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Todo, CreateTodoDto, UpdateTodoDto } from '../model/todo.model';

@Injectable({
  providedIn: 'root'
})
export class TodoService {
  private apiUrl = '/api/todo';

  constructor(private http: HttpClient) { }

  /**
   * Get all todos with optional filtering
   */
  getTodos(
    isComplete?: boolean,
    category?: string,
    priority?: number
  ): Observable<Todo[]> {
    let params = new HttpParams();

    if (isComplete !== undefined) {
      params = params.set('isComplete', isComplete);
    }

    if (category) {
      params = params.set('category', category);
    }

    if (priority !== undefined) {
      params = params.set('priority', priority);
    }

    return this.http.get<Todo[]>(this.apiUrl, { params });
  }

  /**
   * Get a specific todo by id
   */
  getTodo(id: number): Observable<Todo> {
    return this.http.get<Todo>(`${this.apiUrl}/${id}`);
  }

  /**
   * Create a new todo
   */
  createTodo(createTodoDto: CreateTodoDto): Observable<Todo> {
    return this.http.post<Todo>(this.apiUrl, createTodoDto);
  }

  /**
   * Update an existing todo
   */
  updateTodo(id: number, updateTodoDto: UpdateTodoDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, updateTodoDto);
  }

  /**
   * Delete a todo
   */
  deleteTodo(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Toggle the completion status of a todo
   */
  toggleTodoCompletion(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/toggle`, {});
  }
}
