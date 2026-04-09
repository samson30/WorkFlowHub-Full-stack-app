import React from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import GlobalSearch from './GlobalSearch'

const Navbar: React.FC = () => {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <NavLink to="/dashboard">WorkFlowHub</NavLink>
      </div>
      <div className="navbar-links">
        <NavLink to="/projects" className={({ isActive }) => isActive ? 'active' : ''}>
          Projects
        </NavLink>
        <NavLink to="/files" className={({ isActive }) => isActive ? 'active' : ''}>
          Files
        </NavLink>
      </div>
      <GlobalSearch />
      <div className="navbar-user">
        <span className="user-email">{user?.email}</span>
        <span className="user-role">{user?.role}</span>
        <button onClick={handleLogout} className="btn btn-secondary btn-sm">
          Sign out
        </button>
      </div>
    </nav>
  )
}

export default Navbar
