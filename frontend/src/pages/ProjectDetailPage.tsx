import React, { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import api from '../api/axios'
import Navbar from '../components/Navbar'

interface Task {
  id: string
  title: string
  description: string
  status: string
  priority: string
  assignedUserEmail: string | null
  dueDate: string | null
  createdAt: string
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

const STATUS_OPTIONS = ['Todo', 'InProgress', 'Done', 'Cancelled']
const PRIORITY_OPTIONS = ['Low', 'Medium', 'High', 'Critical']

const statusBadge: Record<string, string> = {
  Todo: 'badge-default',
  InProgress: 'badge-info',
  Done: 'badge-success',
  Cancelled: 'badge-danger',
}

const ProjectDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
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

  useEffect(() => {
    api.get<Project>(`/projects/${id}`).then(({ data }) => setProject(data))
  }, [id])

  const fetchTasks = (p: number) => {
    setLoading(true)
    api.get<PagedResult>(`/projects/${id}/tasks?pageNumber=${p}&pageSize=10`)
      .then(({ data }) => {
        setTasks(data.items)
        setPaged({ totalCount: data.totalCount, totalPages: data.totalPages, pageNumber: data.pageNumber })
      })
      .finally(() => setLoading(false))
  }

  useEffect(() => { fetchTasks(page) }, [id, page])

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      await api.post(`/projects/${id}/tasks`, {
        title,
        description,
        priority,
        dueDate: dueDate || null,
      })
      setTitle('')
      setDescription('')
      setPriority('Medium')
      setDueDate('')
      setShowForm(false)
      fetchTasks(1)
    } finally {
      setSaving(false)
    }
  }

  const handleStatusChange = async (taskId: string, status: string) => {
    await api.patch(`/projects/${id}/tasks/${taskId}/status`, { status })
    fetchTasks(page)
  }

  const handleDeleteTask = async (taskId: string) => {
    if (!confirm('Delete this task?')) return
    await api.delete(`/projects/${id}/tasks/${taskId}`)
    fetchTasks(page)
  }

  return (
    <>
      <Navbar />
      <main className="container">
        <div className="page-header">
          <div>
            <button className="btn btn-secondary btn-sm" onClick={() => navigate('/projects')}>
              ← Back
            </button>
            <h2>{project?.name ?? 'Loading...'}</h2>
            {project?.description && <p className="text-muted">{project.description}</p>}
          </div>
          <button className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
            {showForm ? 'Cancel' : 'New Task'}
          </button>
        </div>

        {showForm && (
          <form className="card form-card" onSubmit={handleCreateTask}>
            <h3>New Task</h3>
            <div className="form-group">
              <label>Title</label>
              <input value={title} onChange={(e) => setTitle(e.target.value)} required maxLength={300} />
            </div>
            <div className="form-group">
              <label>Description</label>
              <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={3} maxLength={2000} />
            </div>
            <div className="form-row">
              <div className="form-group">
                <label>Priority</label>
                <select value={priority} onChange={(e) => setPriority(e.target.value)}>
                  {PRIORITY_OPTIONS.map((p) => <option key={p}>{p}</option>)}
                </select>
              </div>
              <div className="form-group">
                <label>Due Date</label>
                <input type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} />
              </div>
            </div>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? 'Creating...' : 'Create Task'}
            </button>
          </form>
        )}

        {loading ? (
          <p className="loading">Loading tasks...</p>
        ) : tasks.length === 0 ? (
          <div className="empty-state"><p>No tasks yet.</p></div>
        ) : (
          <>
            <div className="task-list">
              {tasks.map((t) => (
                <div key={t.id} className="task-card">
                  <div className="task-header">
                    <h4>{t.title}</h4>
                    <span className={`badge ${statusBadge[t.status] ?? 'badge-default'}`}>{t.status}</span>
                  </div>
                  {t.description && <p className="task-desc">{t.description}</p>}
                  <div className="task-meta">
                    <span className="priority">Priority: {t.priority}</span>
                    {t.dueDate && <span>Due: {new Date(t.dueDate).toLocaleDateString()}</span>}
                    {t.assignedUserEmail && <span>Assigned: {t.assignedUserEmail}</span>}
                  </div>
                  <div className="card-actions">
                    <select
                      value={t.status}
                      onChange={(e) => handleStatusChange(t.id, e.target.value)}
                      className="select-sm"
                    >
                      {STATUS_OPTIONS.map((s) => <option key={s}>{s}</option>)}
                    </select>
                    <button className="btn btn-danger btn-sm" onClick={() => handleDeleteTask(t.id)}>Delete</button>
                  </div>
                </div>
              ))}
            </div>

            {paged && paged.totalPages > 1 && (
              <div className="pagination">
                <button className="btn btn-secondary btn-sm" disabled={page === 1} onClick={() => setPage(page - 1)}>Prev</button>
                <span>Page {paged.pageNumber} of {paged.totalPages}</span>
                <button className="btn btn-secondary btn-sm" disabled={page === paged.totalPages} onClick={() => setPage(page + 1)}>Next</button>
              </div>
            )}
          </>
        )}
      </main>
    </>
  )
}

export default ProjectDetailPage
