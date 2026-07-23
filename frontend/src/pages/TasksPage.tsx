import { useCallback, useEffect, useState, type FormEvent, type MouseEvent } from 'react'
import {
  Calendar,
  Check,
  ListTodo,
  Pencil,
  Plus,
  RefreshCw,
  SquareCheckBig,
  Trash2,
} from 'lucide-react'
import { ApiError } from '../api/client'
import * as tasksApi from '../api/tasksApi'
import { useAuth } from '../auth/AuthContext'
import {
  TASK_STATUS_LABELS,
  TaskStatus,
  type CreateTaskRequest,
  type Task,
} from '../types'

type FormState = {
  title: string
  description: string
  status: TaskStatus
  dueDate: string
}

const emptyForm: FormState = {
  title: '',
  description: '',
  status: TaskStatus.Todo,
  dueDate: '',
}

function toRequest(form: FormState): CreateTaskRequest {
  return {
    title: form.title.trim(),
    description: form.description.trim() ? form.description.trim() : null,
    status: form.status,
    dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null,
  }
}

function dueDateInputValue(value: string | null): string {
  if (!value) {
    return ''
  }
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return ''
  }
  return date.toISOString().slice(0, 10)
}

function statusKey(status: TaskStatus): 'todo' | 'progress' | 'done' {
  switch (status) {
    case TaskStatus.InProgress:
      return 'progress'
    case TaskStatus.Done:
      return 'done'
    default:
      return 'todo'
  }
}

export function TasksPage() {
  const { token, user, logout } = useAuth()
  const [tasks, setTasks] = useState<Task[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [form, setForm] = useState<FormState>(emptyForm)
  const [editingId, setEditingId] = useState<string | null>(null)

  const loadTasks = useCallback(async () => {
    if (!token) {
      return
    }
    setLoading(true)
    setError(null)
    try {
      const data = await tasksApi.listTasks(token)
      setTasks(data)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Failed to load tasks')
    } finally {
      setLoading(false)
    }
  }, [token])

  useEffect(() => {
    void loadTasks()
  }, [loadTasks])

  function startCreate() {
    setEditingId(null)
    setForm(emptyForm)
  }

  function startEdit(task: Task) {
    setEditingId(task.id)
    setForm({
      title: task.title,
      description: task.description ?? '',
      status: task.status,
      dueDate: dueDateInputValue(task.dueDate),
    })
  }

  async function onSubmit(event: FormEvent) {
    event.preventDefault()
    if (!token) {
      return
    }
    setSaving(true)
    setError(null)
    try {
      const body = toRequest(form)
      if (editingId) {
        await tasksApi.updateTask(token, editingId, body)
      } else {
        await tasksApi.createTask(token, body)
      }
      setForm(emptyForm)
      setEditingId(null)
      await loadTasks()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Save failed')
    } finally {
      setSaving(false)
    }
  }

  async function onDelete(task: Task) {
    if (!token) {
      return
    }
    if (!window.confirm(`Delete “${task.title}”?`)) {
      return
    }
    setError(null)
    try {
      await tasksApi.deleteTask(token, task.id)
      if (editingId === task.id) {
        startCreate()
      }
      await loadTasks()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Delete failed')
    }
  }

  async function onToggleDone(task: Task) {
    if (!token) {
      return
    }
    const nextStatus = task.status === TaskStatus.Done ? TaskStatus.Todo : TaskStatus.Done
    setError(null)
    try {
      await tasksApi.updateTask(token, task.id, {
        title: task.title,
        description: task.description,
        status: nextStatus,
        dueDate: task.dueDate,
      })
      if (editingId === task.id) {
        setForm((f) => ({ ...f, status: nextStatus }))
      }
      await loadTasks()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Update failed')
    }
  }

  function stopCardClick(event: MouseEvent) {
    event.stopPropagation()
  }

  const formStatusKey = statusKey(form.status)

  return (
    <main className="app-shell">
      <header className="topbar">
        <div className="topbar-brand">
          <div className="topbar-brand-row">
            <span className="brand-mark" aria-hidden>
              <SquareCheckBig size={26} strokeWidth={2.25} />
            </span>
            <p className="brand-name">TaskManager</p>
          </div>
          <p className="signed-in">Signed in as {user?.displayName ?? user?.email}</p>
        </div>
        <button type="button" className="ghost" onClick={logout}>
          Sign out
        </button>
      </header>

      <div className="layout">
        <section className={`card form-panel${editingId ? ' is-editing' : ''}`}>
          <div className="section-head">
            <h2 className="section-title">
              <ListTodo className="title-icon" size={20} strokeWidth={2.25} aria-hidden />
              {editingId ? 'Edit task' : 'Create a new task'}
            </h2>
          </div>
          <form onSubmit={onSubmit} className="stack">
            <label>
              Title
              <input
                value={form.title}
                onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
                placeholder="e.g. Prepare demo walkthrough"
                required
              />
            </label>
            <label>
              Description
              <textarea
                value={form.description}
                onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                placeholder="Add more details (optional)"
                rows={3}
              />
            </label>
            <div className="field-row">
              <label className="status-field">
                Status
                <span className={`status-dot ${formStatusKey}`} aria-hidden />
                <select
                  value={form.status}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, status: Number(e.target.value) as TaskStatus }))
                  }
                >
                  {Object.values(TaskStatus).map((status) => (
                    <option key={status} value={status}>
                      {TASK_STATUS_LABELS[status]}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Due date
                <input
                  type="date"
                  value={form.dueDate}
                  onChange={(e) => setForm((f) => ({ ...f, dueDate: e.target.value }))}
                  placeholder="Select a date"
                />
              </label>
            </div>
            <div className="form-actions">
              <button type="submit" className="btn-block" disabled={saving}>
                {editingId ? null : <Plus size={18} strokeWidth={2.5} aria-hidden />}
                {saving ? 'Saving…' : editingId ? 'Update task' : 'Create task'}
              </button>
              {editingId ? (
                <button type="button" className="ghost btn-block" onClick={startCreate}>
                  Cancel
                </button>
              ) : null}
            </div>
          </form>
        </section>

        <section className="card list-panel">
          <div className="section-head">
            <h2 className="section-title">
              <ListTodo className="title-icon" size={20} strokeWidth={2.25} aria-hidden />
              Your tasks
            </h2>
            <button type="button" className="ghost" onClick={() => void loadTasks()}>
              <RefreshCw size={16} strokeWidth={2.25} aria-hidden />
              Refresh
            </button>
          </div>
          {error ? <p className="error">{error}</p> : null}
          {loading ? <p className="muted">Loading tasks…</p> : null}
          {!loading && tasks.length === 0 ? (
            <div className="empty-state">
              <p className="muted">No tasks yet. Create one on the left to get started.</p>
            </div>
          ) : null}
          <ul className="task-list">
            {tasks.map((task) => {
              const key = statusKey(task.status)
              const isDone = task.status === TaskStatus.Done
              return (
                <li
                  key={task.id}
                  className={`task-card status-${key}`}
                  onClick={() => startEdit(task)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      e.preventDefault()
                      startEdit(task)
                    }
                  }}
                  role="button"
                  tabIndex={0}
                >
                  <button
                    type="button"
                    className={`task-check${isDone ? ' is-done' : ''}`}
                    aria-label={isDone ? 'Mark as todo' : 'Mark as done'}
                    onClick={(e) => {
                      stopCardClick(e)
                      void onToggleDone(task)
                    }}
                  >
                    {isDone ? <Check size={14} strokeWidth={3} aria-hidden /> : null}
                  </button>
                  <div className="task-main">
                    <p className="task-title">{task.title}</p>
                    <div className="task-meta">
                      <span className={`badge status-${key}`}>
                        {TASK_STATUS_LABELS[task.status]}
                      </span>
                      {task.dueDate ? (
                        <span className="due">
                          <Calendar size={14} strokeWidth={2} aria-hidden />
                          Due {new Date(task.dueDate).toLocaleDateString()}
                        </span>
                      ) : null}
                    </div>
                    {task.description ? (
                      <p className="task-description">{task.description}</p>
                    ) : null}
                  </div>
                  <div className="task-actions" onClick={stopCardClick}>
                    <button
                      type="button"
                      className="icon-btn"
                      aria-label="Edit task"
                      onClick={() => startEdit(task)}
                    >
                      <Pencil size={16} strokeWidth={2} />
                    </button>
                    <button
                      type="button"
                      className="icon-btn danger"
                      aria-label="Delete task"
                      onClick={() => void onDelete(task)}
                    >
                      <Trash2 size={16} strokeWidth={2} />
                    </button>
                  </div>
                </li>
              )
            })}
          </ul>
          {!loading && tasks.length > 0 ? (
            <p className="list-tip">Tip: Click on a task to edit.</p>
          ) : null}
        </section>
      </div>
    </main>
  )
}
