import { useCallback, useEffect, useState, type FormEvent } from 'react'
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

  return (
    <main className="app-shell">
      <header className="topbar">
        <div>
          <h1>TaskManager</h1>
          <p className="muted">Signed in as {user?.displayName ?? user?.email}</p>
        </div>
        <button type="button" className="secondary" onClick={logout}>
          Sign out
        </button>
      </header>

      <div className="layout">
        <section className="card">
          <div className="section-head">
            <h2>{editingId ? 'Edit task' : 'New task'}</h2>
            {editingId ? (
              <button type="button" className="secondary" onClick={startCreate}>
                Cancel edit
              </button>
            ) : null}
          </div>
          <form onSubmit={onSubmit} className="stack">
            <label>
              Title
              <input
                value={form.title}
                onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
                required
              />
            </label>
            <label>
              Description
              <textarea
                value={form.description}
                onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                rows={3}
              />
            </label>
            <label>
              Status
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
              />
            </label>
            <button type="submit" disabled={saving}>
              {saving ? 'Saving…' : editingId ? 'Update task' : 'Create task'}
            </button>
          </form>
        </section>

        <section className="card">
          <div className="section-head">
            <h2>Your tasks</h2>
            <button type="button" className="secondary" onClick={() => void loadTasks()}>
              Refresh
            </button>
          </div>
          {error ? <p className="error">{error}</p> : null}
          {loading ? <p className="muted">Loading tasks…</p> : null}
          {!loading && tasks.length === 0 ? (
            <p className="muted">No tasks yet. Create one on the left.</p>
          ) : null}
          <ul className="task-list">
            {tasks.map((task) => (
              <li key={task.id} className="task-item">
                <div>
                  <strong>{task.title}</strong>
                  <p className="muted">
                    {TASK_STATUS_LABELS[task.status]}
                    {task.dueDate
                      ? ` · due ${new Date(task.dueDate).toLocaleDateString()}`
                      : ''}
                  </p>
                  {task.description ? <p>{task.description}</p> : null}
                </div>
                <div className="row">
                  <button type="button" className="secondary" onClick={() => startEdit(task)}>
                    Edit
                  </button>
                  <button type="button" className="danger" onClick={() => void onDelete(task)}>
                    Delete
                  </button>
                </div>
              </li>
            ))}
          </ul>
        </section>
      </div>
    </main>
  )
}
