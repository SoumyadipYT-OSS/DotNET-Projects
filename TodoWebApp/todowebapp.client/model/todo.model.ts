
export interface Todo
{
  id: number;
  title: string;
  description: string;
  isComplete: boolean;
  priority: TodoPriority;
  dueDate: Date | null;
  category: string;
  tags: string;
  createdDate: Date;
  lastModifiedDate: Date;
}

export interface CreateTodoDto
{
  title: string;
  description?: string;
  priority?: TodoPriority;
  dueDate?: Date | null;
  category?: string;
  tags?: string;
}

export interface UpdateTodoDto
{
  title?: string;
  description?: string;
  priority?: TodoPriority;
  dueDate?: Date | null;
  category?: string;
  tags?: string;
  isComplete?: boolean;
}

export enum TodoPriority
{
  Low = 0,
  Medium = 1,
  High = 2
}
