import React, { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/axios'
import Navbar from '../components/Navbar'

interface Project {
  id: string
  name: string
  description: string
  taskCount: number
  updatedAt: string
}

interface PagedResult {
  items: Project[]
  totalCount: number
  totalPages: number
  pageNumber: number
}

const ProjectsPage: React.FC = () => {
  const [projects, setProjects] = useState<Project[]>([])
  const [paged, setPaged] = useState<Omit<PagedResult, 'items'> | null>(null)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [saving, setSaving] = useState(false)

  const fetchProjects = (p: number) => {
    setLoading(true)
    api.get<PagedResult>(`/projects?pageNumber=${p}&pageSize=10`)
      .then(({ data }) => {
        setProjects(data.items)
        setPaged({ totalCount: data.totalCount, totalPages: data.totalPages, pageNumber: data.pageNumber })
      })
      .finally(() => setLoading(false))
  }

  useEffect(() => { fetchProjects(page) }, [page])

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      await api.post('/projects', { name, description })
      setName('')
      setDescription('')
      setShowForm(false)
      fetchProjects(1)
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this project and all its tasks?')) return
    await api.delete(`/projects/${id}`)
    fetchProjects(page)
  }

  return (
    <>
      <Navbar />
      <main className="container">
        <div className="page-header">
          <div>
            <h2>Projects</h2>
            {paged && <p className="text-muted" style={{ marginTop: 4 }}>{paged.totalCount} project{paged.totalCount !== 1 ? 's' : ''} total</p>}
          </div>
          <button className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
            {showForm ? '✕ Cancel' : '+ New Project'}
          </button>
        </div>

        {showForm && (
          <form className="card form-card" onSubmit={handleCreate}>
            <h3>New Project</h3>
            <div className="form-group">
              <label>Project name</label>
              <input
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g. Marketing Website"
                required
                maxLength={200}
                autoFocus
              />
            </div>
            <div className="form-group">
              <label>Description <span style={{ color: 'var(--text-subtle)', fontWeight: 400 }}>(optional)</span></label>
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="What is this project about?"
                maxLength={2000}
                rows={3}
              />
            </div>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? 'Creating...' : 'Create Project'}
            </button>
          </form>
        )}

        {loading ? (
          <div className="loading"><div className="spinner" />Loading projects...</div>
        ) : projects.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">📋</div>
            <p>No projects yet</p>
            <small>Click "New Project" to create your first one</small>
          </div>
        ) : (
          <>
            <div className="project-grid">
              {projects.map((p) => (
                <div key={p.id} className="project-card">
                  <Link to={`/projects/${p.id}`}><h4>{p.name}</h4></Link>
                  <p className="project-desc">{p.description || 'No description'}</p>
                  <div className="project-meta">
                    <span>{p.taskCount} task{p.taskCount !== 1 ? 's' : ''}</span>
                    <span>Updated {new Date(p.updatedAt).toLocaleDateString()}</span>
                  </div>
                  <div className="card-actions">
                    <Link to={`/projects/${p.id}`} className="btn btn-secondary btn-sm">Open →</Link>
                    <button className="btn btn-danger btn-sm" onClick={() => handleDelete(p.id)}>Delete</button>
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
    </>
  )
}

export default ProjectsPage
