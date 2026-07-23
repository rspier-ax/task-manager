import { apiRequest } from './client'
import type { CreateTaskRequest, Task, UpdateTaskRequest } from '../types'

export function listTasks(token: string): Promise<Task[]> {
  return apiRequest<Task[]>('/api/tasks', { token })
}

export function createTask(token: string, body: CreateTaskRequest): Promise<Task> {
  return apiRequest<Task>('/api/tasks', { method: 'POST', token, body })
}

export function updateTask(token: string, id: string, body: UpdateTaskRequest): Promise<Task> {
  return apiRequest<Task>(`/api/tasks/${id}`, { method: 'PUT', token, body })
}

export function deleteTask(token: string, id: string): Promise<void> {
  return apiRequest<void>(`/api/tasks/${id}`, { method: 'DELETE', token })
}
