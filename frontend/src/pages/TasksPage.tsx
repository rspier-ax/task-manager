import {
  useCallback,
  useEffect,
  useRef,
  useState,
  type FormEvent,
  type MouseEvent,
} from 'react'
import {
  Calendar,
  Check,
  ChevronDown,
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
import { ConfirmDialog } from '../components/ConfirmDialog'
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

const STATUS_OPTIONS = Object.values(TaskStatus)

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
  const [refreshing, setRefreshing] = useState(false)
  const [saving, setSaving] = useState(false)
  const [togglingId, setTogglingId] = useState<string | null>(null)
  const [deleting, setDeleting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [form, setForm] = useState<FormState>(emptyForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [statusOpen, setStatusOpen] = useState(false)
  const [pendingDelete, setPendingDelete] = useState<Task | null>(null)
  const statusMenuRef = useRef<HTMLDivElement>(null)

  const refreshTasks = useCallback(
    async (mode: 'initial' | 'silent' | 'refresh' = 'silent') => {
      if (!token) {
        return
      }
      if (mode === 'initial') {
        setLoading(true)
      } else if (mode === 'refresh') {
        setRefreshing(true)
      }
      setError(null)
      try {
        const data = await tasksApi.listTasks(token)
        setTasks(data)
      } catch (err) {
        setError(err instanceof ApiError ? err.message : 'Failed to load tasks')
      } finally {
        if (mode === 'initial') {
          setLoading(false)
        } else if (mode === 'refresh') {
          setRefreshing(false)
        }
      }
    },
    [token],
  )

  useEffect(() => {
    void refreshTasks('initial')
  }, [refreshTasks])

  useEffect(() => {
    if (!statusOpen) {
      return
    }

    function onPointerDown(event: PointerEvent) {
      if (!statusMenuRef.current?.contains(event.target as Node)) {
        setStatusOpen(false)
      }
    }

    function onKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setStatusOpen(false)
      }
    }

    document.addEventListener('pointerdown', onPointerDown)
    document.addEventListener('keydown', onKeyDown)
    return () => {
      document.removeEventListener('pointerdown', onPointerDown)
      document.removeEventListener('keydown', onKeyDown)
    }
  }, [statusOpen])

  function startCreate() {
    setEditingId(null)
    setForm(emptyForm)
    setStatusOpen(false)
  }

  function startEdit(task: Task) {
    setEditingId(task.id)
    setForm({
      title: task.title,
      description: task.description ?? '',
      status: task.status,
      dueDate: dueDateInputValue(task.dueDate),
    })
    setStatusOpen(false)
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
      setStatusOpen(false)
      await refreshTasks('silent')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Save failed')
    } finally {
      setSaving(false)
    }
  }

  async function confirmDelete() {
    if (!token || !pendingDelete) {
      return
    }
    const task = pendingDelete
    setDeleting(true)
    setError(null)
    try {
      await tasksApi.deleteTask(token, task.id)
      setPendingDelete(null)
      if (editingId === task.id) {
        startCreate()
      }
      setTasks((current) => current.filter((item) => item.id !== task.id))
      await refreshTasks('silent')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Delete failed')
    } finally {
      setDeleting(false)
    }
  }

  async function onToggleDone(task: Task) {
    if (!token || togglingId) {
      return
    }
    const previous = task
    const nextStatus = task.status === TaskStatus.Done ? TaskStatus.Todo : TaskStatus.Done
    const optimistic: Task = { ...task, status: nextStatus }
    setTogglingId(task.id)
    setError(null)
    setTasks((current) => current.map((item) => (item.id === task.id ? optimistic : item)))
    if (editingId === task.id) {
      setForm((f) => ({ ...f, status: nextStatus }))
    }
    try {
      await tasksApi.updateTask(token, task.id, {
        title: task.title,
        description: task.description,
        status: nextStatus,
        dueDate: task.dueDate,
      })
    } catch (err) {
      setTasks((current) => current.map((item) => (item.id === task.id ? previous : item)))
      if (editingId === task.id) {
        setForm((f) => ({ ...f, status: previous.status }))
      }
      setError(err instanceof ApiError ? err.message : 'Update failed')
    } finally {
      setTogglingId(null)
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
              <div className="status-field" ref={statusMenuRef}>
                <span className="field-label">Status</span>
                <button
                  type="button"
                  className={`status-trigger${statusOpen ? ' is-open' : ''}`}
                  aria-haspopup="listbox"
                  aria-expanded={statusOpen}
                  onClick={() => setStatusOpen((open) => !open)}
                >
                  <span className={`status-dot ${formStatusKey}`} aria-hidden />
                  <span className="status-trigger-label">
                    {TASK_STATUS_LABELS[form.status]}
                  </span>
                  <ChevronDown
                    className="status-chevron"
                    size={16}
                    strokeWidth={2.25}
                    aria-hidden
                  />
                </button>
                {statusOpen ? (
                  <ul className="status-menu" role="listbox" aria-label="Status">
                    {STATUS_OPTIONS.map((status) => {
                      const key = statusKey(status)
                      const selected = form.status === status
                      return (
                        <li key={status} role="option" aria-selected={selected}>
                          <button
                            type="button"
                            className={`status-option${selected ? ' is-selected' : ''}`}
                            onClick={() => {
                              setForm((f) => ({ ...f, status }))
                              setStatusOpen(false)
                            }}
                          >
                            <span className={`status-dot ${key}`} aria-hidden />
                            {TASK_STATUS_LABELS[status]}
                          </button>
                        </li>
                      )
                    })}
                  </ul>
                ) : null}
              </div>
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
            <button
              type="button"
              className="ghost"
              disabled={refreshing || loading}
              onClick={() => void refreshTasks('refresh')}
            >
              <RefreshCw
                className={refreshing ? 'spin' : undefined}
                size={16}
                strokeWidth={2.25}
                aria-hidden
              />
              Refresh
            </button>
          </div>
          {error ? <p className="error">{error}</p> : null}
          {loading ? (
            <ul className="task-list" aria-busy="true" aria-label="Loading tasks">
              {[0, 1, 2].map((index) => (
                <li key={index} className="task-skeleton" />
              ))}
            </ul>
          ) : null}
          {!loading && tasks.length === 0 ? (
            <div className="empty-state">
              <p className="muted">No tasks yet. Create one on the left to get started.</p>
            </div>
          ) : null}
          {!loading ? (
            <ul className="task-list">
              {tasks.map((task) => {
                const key = statusKey(task.status)
                const isDone = task.status === TaskStatus.Done
                const isToggling = togglingId === task.id
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
                      disabled={isToggling}
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
                        onClick={() => setPendingDelete(task)}
                      >
                        <Trash2 size={16} strokeWidth={2} />
                      </button>
                    </div>
                  </li>
                )
              })}
            </ul>
          ) : null}
          {!loading && tasks.length > 0 ? (
            <p className="list-tip">Tip: Click on a task to edit.</p>
          ) : null}
        </section>
      </div>

      <ConfirmDialog
        open={pendingDelete !== null}
        title="Delete task?"
        message={
          pendingDelete
            ? `“${pendingDelete.title}” will be removed permanently.`
            : ''
        }
        confirmLabel="Delete"
        cancelLabel="Cancel"
        danger
        busy={deleting}
        onCancel={() => {
          if (!deleting) {
            setPendingDelete(null)
          }
        }}
        onConfirm={() => void confirmDelete()}
      />
    </main>
  )
}
