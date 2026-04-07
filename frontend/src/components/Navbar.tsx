import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

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
        <Link to="/dashboard">WorkFlowHub</Link>
      </div>
      <div className="navbar-links">
        <Link to="/projects">Projects</Link>
        <Link to="/files">Files</Link>
      </div>
      <div className="navbar-user">
        <span className="user-email">{user?.email}</span>
        <span className="user-role">{user?.role}</span>
        <button onClick={handleLogout} className="btn btn-secondary btn-sm">
          Logout
        </button>
      </div>
    </nav>
  )
}

export default Navbar
