import React, { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/axios'
import Navbar from '../components/Navbar'
import { useAuth } from '../context/AuthContext'

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
}

const DashboardPage: React.FC = () => {
  const { user } = useAuth()
  const [projects, setProjects] = useState<Project[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api.get<PagedResult>('/projects?pageNumber=1&pageSize=6')
      .then(({ data }) => {
        setProjects(data.items)
        setTotalCount(data.totalCount)
      })
      .finally(() => setLoading(false))
  }, [])

  const totalTasks = projects.reduce((sum, p) => sum + (p.taskCount ?? 0), 0)
  const firstName = user?.email?.split('@')[0] ?? 'there'

  return (
    <>
      <Navbar />
      <main className="container">
        <div className="dashboard-hero">
          <div>
            <h2>Welcome back, {firstName} 👋</h2>
            <p>Here's an overview of your workspace</p>
          </div>
          <Link to="/projects" className="btn btn-secondary btn-sm">
            View all projects →
          </Link>
        </div>

        <div className="stats-grid">
          <div className="stat-card">
            <span className="stat-value">{totalCount}</span>
            <span className="stat-label">Total Projects</span>
          </div>
          <div className="stat-card">
            <span className="stat-value">{totalTasks}</span>
            <span className="stat-label">Tasks (recent)</span>
          </div>
        </div>

        <div className="section-header">
          <h3>Recent Projects</h3>
          <Link to="/projects" className="btn btn-primary btn-sm">+ New Project</Link>
        </div>

        {loading ? (
          <div className="loading"><div className="spinner" />Loading projects...</div>
        ) : projects.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">📋</div>
            <p>No projects yet</p>
            <small>Create your first project to get started</small>
            <Link to="/projects" className="btn btn-primary">Create a project</Link>
          </div>
        ) : (
          <div className="project-grid">
            {projects.map((p) => (
              <Link key={p.id} to={`/projects/${p.id}`} className="project-card">
                <h4>{p.name}</h4>
                <p className="project-desc">{p.description || 'No description'}</p>
                <div className="project-meta">
                  <span>{p.taskCount} task{p.taskCount !== 1 ? 's' : ''}</span>
                  <span>{new Date(p.updatedAt).toLocaleDateString()}</span>
                </div>
              </Link>
            ))}
          </div>
        )}
      </main>
    </>
  )
}

export default DashboardPage
