import React, { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import api from '../api/axios'
import Navbar from '../components/Navbar'
import TaskDetailModal from '../components/TaskDetailModal'
import ConfirmDialog from '../components/ConfirmDialog'
import KanbanBoard from '../components/KanbanBoard'
import { useToast } from '../context/ToastContext'

interface Label {
  id: string
  name: string
  color: string
}

interface Task {
  id: string
  title: string
  description: string
  status: string
  priority: string
  assignedUserEmail: string | null
  dueDate: string | null
  createdAt: string
  labels: Label[]
}

interface Project {
  id: string
  name: string
  description: string
}

interface PagedResult {
  items: Task[]
  totalCount: number
  totalPages: number
  pageNumber: number
}

const PRIORITY_OPTIONS = ['Low', 'Medium', 'High', 'Critical']

const statusBadge: Record<string, string> = {
  Todo: 'badge-default',
  InProgress: 'badge-info',
  Done: 'badge-success',
  Cancelled: 'badge-danger',
}

const statusLabel: Record<string, string> = {
  InProgress: 'In Progress',
}

const priorityClass: Record<string, string> = {
  Low: 'low',
  Medium: 'medium',
  High: 'high',
  Critical: 'critical',
}

const ProjectDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { toast } = useToast()
  const [project, setProject] = useState<Project | null>(null)
  const [tasks, setTasks] = useState<Task[]>([])
  const [paged, setPaged] = useState<Omit<PagedResult, 'items'> | null>(null)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [priority, setPriority] = useState('Medium')
  const [dueDate, setDueDate] = useState('')
  const [saving, setSaving] = useState(false)
  const [selectedTask, setSelectedTask] = useState<Task | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<string | null>(null)
  const [viewMode, setViewMode] = useState<'list' | 'board'>('list')

  useEffect(() => {
    api.get<Project>(`/projects/${id}`)
      .then(({ data }) => setProject(data))
      .catch(() => toast('Failed to load project', 'error'))
  }, [id])

  const fetchTasks = (p: number) => {
    setLoading(true)
    api.get<PagedResult>(`/projects/${id}/tasks?pageNumber=${p}&pageSize=10`)
      .then(({ data }) => {
        setTasks(data.items)
        setPaged({ totalCount: data.totalCount, totalPages: data.totalPages, pageNumber: data.pageNumber })
      })
      .catch(() => toast('Failed to load tasks', 'error'))
      .finally(() => setLoading(false))
  }

  useEffect(() => { fetchTasks(page) }, [id, page])

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      await api.post(`/projects/${id}/tasks`, { title, description, priority, dueDate: dueDate || null })
      setTitle('')
      setDescription('')
      setPriority('Medium')
      setDueDate('')
      setShowForm(false)
      fetchTasks(1)
      toast('Task created')
    } catch {
      toast('Failed to create task', 'error')
    } finally {
      setSaving(false)
    }
  }

  const handleStatusChange = (taskId: string, status: string) => {
    setTasks(prev => prev.map(t => t.id === taskId ? { ...t, status } : t))
    if (selectedTask?.id === taskId) setSelectedTask(prev => prev ? { ...prev, status } : prev)
  }

  const handleDeleteTask = async () => {
    if (!deleteTarget) return
    try {
      await api.delete(`/projects/${id}/tasks/${deleteTarget}`)
      setDeleteTarget(null)
      setSelectedTask(null)
      fetchTasks(page)
      toast('Task deleted')
    } catch {
      toast('Failed to delete task', 'error')
    }
  }

  return (
    <>
      <Navbar />
      <main className="container">
        <div className="page-header">
          <div>
            <button className="btn btn-secondary btn-sm" style={{ marginBottom: 10 }} onClick={() => navigate('/projects')}>
              ← Back to Projects
            </button>
            <h2>{project?.name ?? 'Loading...'}</h2>
            {project?.description && <p className="text-muted" style={{ marginTop: 4 }}>{project.description}</p>}
          </div>
          <div style={{ display: 'flex', gap: 8 }}>
            <div className="view-toggle">
              <button
                className={`view-toggle-btn${viewMode === 'list' ? ' active' : ''}`}
                onClick={() => setViewMode('list')}
                title="List view"
              >☰ List</button>
              <button
                className={`view-toggle-btn${viewMode === 'board' ? ' active' : ''}`}
                onClick={() => setViewMode('board')}
                title="Board view"
              >⬛ Board</button>
            </div>
            <button className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
              {showForm ? '✕ Cancel' : '+ New Task'}
            </button>
          </div>
        </div>

        {showForm && (
          <form className="card form-card" onSubmit={handleCreateTask}>
            <h3>New Task</h3>
            <div className="form-group">
              <label>Task title</label>
              <input
                value={title}
                onChange={e => setTitle(e.target.value)}
                placeholder="e.g. Design landing page"
                required
                maxLength={300}
                autoFocus
              />
            </div>
            <div className="form-group">
              <label>Description <span style={{ color: 'var(--text-subtle)', fontWeight: 400 }}>(optional)</span></label>
              <textarea
                value={description}
                onChange={e => setDescription(e.target.value)}
                placeholder="Add more detail about this task..."
                rows={3}
                maxLength={2000}
              />
            </div>
            <div className="form-row">
              <div className="form-group">
                <label>Priority</label>
                <select value={priority} onChange={e => setPriority(e.target.value)}>
                  {PRIORITY_OPTIONS.map(p => <option key={p}>{p}</option>)}
                </select>
              </div>
              <div className="form-group">
                <label>Due Date</label>
                <input type="date" value={dueDate} onChange={e => setDueDate(e.target.value)} required />
              </div>
            </div>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? 'Creating...' : 'Create Task'}
            </button>
          </form>
        )}

        {loading ? (
          <div className="loading"><div className="spinner" />Loading tasks...</div>
        ) : tasks.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">✅</div>
            <p>No tasks yet</p>
            <small>Click "New Task" to add the first task to this project</small>
          </div>
        ) : viewMode === 'board' ? (
          <KanbanBoard tasks={tasks} onTaskClick={setSelectedTask} />
        ) : (
          <>
            {paged && (
              <p className="text-muted" style={{ marginBottom: 14 }}>
                {paged.totalCount} task{paged.totalCount !== 1 ? 's' : ''}
              </p>
            )}
            <div className="task-list">
              {tasks.map(t => (
                <div
                  key={t.id}
                  className={`task-card priority-${priorityClass[t.priority] ?? 'medium'}`}
                  onClick={() => setSelectedTask(t)}
                  style={{ cursor: 'pointer' }}
                >
                  <div className="task-header">
                    <h4>{t.title}</h4>
                    <span className={`badge ${statusBadge[t.status] ?? 'badge-default'}`}>
                      {statusLabel[t.status] ?? t.status}
                    </span>
                  </div>
                  {t.description && <p className="task-desc">{t.description}</p>}
                  <div className="task-meta">
                    <span className={`priority-badge ${priorityClass[t.priority] ?? 'medium'}`}>{t.priority}</span>
                    {t.dueDate && <span>Due {new Date(t.dueDate).toLocaleDateString()}</span>}
                    {t.assignedUserEmail && <span>Assigned: {t.assignedUserEmail}</span>}
                    {t.labels?.map((l: Label) => (
                      <span key={l.id} className="label-chip label-chip-sm" style={{ backgroundColor: l.color }}>{l.name}</span>
                    ))}
                  </div>
                </div>
              ))}
            </div>

            {paged && paged.totalPages > 1 && (
              <div className="pagination">
                <button className="btn btn-secondary btn-sm" disabled={page === 1} onClick={() => setPage(page - 1)}>← Prev</button>
                <span>Page {paged.pageNumber} of {paged.totalPages}</span>
                <button className="btn btn-secondary btn-sm" disabled={page === paged.totalPages} onClick={() => setPage(page + 1)}>Next →</button>
              </div>
            )}
          </>
        )}
      </main>

      {selectedTask && (
        <TaskDetailModal
          task={selectedTask}
          projectId={id!}
          onClose={() => setSelectedTask(null)}
          onStatusChange={handleStatusChange}
          onDelete={taskId => setDeleteTarget(taskId)}
        />
      )}

      {deleteTarget && (
        <ConfirmDialog
          message="Delete this task? This cannot be undone."
          confirmLabel="Delete"
          danger
          onConfirm={handleDeleteTask}
          onCancel={() => setDeleteTarget(null)}
        />
      )}
    </>
  )
}

export default ProjectDetailPage
