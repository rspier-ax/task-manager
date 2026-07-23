export const TaskStatus = {
  Todo: 0,
  InProgress: 1,
  Done: 2,
} as const

export type TaskStatus = (typeof TaskStatus)[keyof typeof TaskStatus]

export type AuthResponse = {
  token: string
  userId: string
  email: string
  displayName: string
}

export type Task = {
  id: string
  userId: string
  title: string
  description: string | null
  status: TaskStatus
  dueDate: string | null
  createdAt: string
  updatedAt: string
}

export type CreateTaskRequest = {
  title: string
  description: string | null
  status: TaskStatus
  dueDate: string | null
}

export type UpdateTaskRequest = CreateTaskRequest

export const TASK_STATUS_LABELS: Record<TaskStatus, string> = {
  [TaskStatus.Todo]: 'Todo',
  [TaskStatus.InProgress]: 'In progress',
  [TaskStatus.Done]: 'Done',
}
