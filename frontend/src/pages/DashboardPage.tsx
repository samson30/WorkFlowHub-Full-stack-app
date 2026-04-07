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
}

const DashboardPage: React.FC = () => {
  const [projects, setProjects] = useState<Project[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api.get<PagedResult>('/projects?pageNumber=1&pageSize=5')
      .then(({ data }) => {
        setProjects(data.items)
        setTotalCount(data.totalCount)
      })
      .finally(() => setLoading(false))
  }, [])

  return (
    <>
      <Navbar />
      <main className="container">
        <div className="page-header">
          <h2>Dashboard</h2>
        </div>

        <div className="stats-grid">
          <div className="stat-card">
            <span className="stat-value">{totalCount}</span>
            <span className="stat-label">Total Projects</span>
          </div>
        </div>

        <div className="section-header">
          <h3>Recent Projects</h3>
          <Link to="/projects" className="btn btn-primary btn-sm">View all</Link>
        </div>

        {loading ? (
          <p className="loading">Loading...</p>
        ) : projects.length === 0 ? (
          <div className="empty-state">
            <p>No projects yet.</p>
            <Link to="/projects" className="btn btn-primary">Create your first project</Link>
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
