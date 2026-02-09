import { useEffect, useState } from "react";
import { getProjects, deleteProject } from "../api/projectApi";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { Project } from "../types/project";
import CreateProject from "./CreateProject";
import EditProject from "./EditProject";

export default function Dashboard() {
  const [projects, setProjects] = useState<Project[]>([]);
  const [error, setError] = useState("");
  const [showForm, setShowForm] = useState(false);
  const [editingProject, setEditingProject] = useState<Project | null>(null);
  const navigate = useNavigate();
  const { logout } = useAuth();

  const loadProjects = () => {
    getProjects()
      .then(setProjects)
      .catch(() => setError("Failed to load projects"));
  };

  useEffect(() => {
    loadProjects();
  }, []);

  const handleLogout = () => {
    logout();
    navigate("/");
  };

  const handleDelete = async (e: React.MouseEvent, projectId: number) => {
    e.stopPropagation();
    if (!window.confirm("Are you sure you want to delete this project? All tasks will be removed.")) {
      return;
    }

    try {
      await deleteProject(projectId);
      loadProjects();
    } catch {
      setError("Failed to delete project");
    }
  };

  const handleEdit = (e: React.MouseEvent, project: Project) => {
    e.stopPropagation();
    setEditingProject(project);
    setShowForm(false);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 py-4 flex justify-between items-center">
          <h1 className="text-2xl font-bold text-gray-800">WorkFlowHub</h1>
          <div className="flex gap-3">
            <button
              onClick={() => {
                setShowForm(!showForm);
                setEditingProject(null);
              }}
              className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
            >
              + New Project
            </button>
            <button
              onClick={handleLogout}
              className="bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300"
            >
              Logout
            </button>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 py-6">
        {/* Create Project Form */}
        {showForm && (
          <div className="mb-6">
            <CreateProject
              onCreated={() => {
                loadProjects();
                setShowForm(false);
              }}
              onCancel={() => setShowForm(false)}
            />
          </div>
        )}

        {/* Edit Project Form */}
        {editingProject && (
          <div className="mb-6">
            <EditProject
              project={editingProject}
              onUpdated={() => {
                loadProjects();
                setEditingProject(null);
              }}
              onCancel={() => setEditingProject(null)}
            />
          </div>
        )}

        {/* Error Message */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-600 px-4 py-3 rounded-lg mb-4">
            {error}
          </div>
        )}

        {/* Projects Grid */}
        <h2 className="text-xl font-semibold text-gray-700 mb-4">My Projects</h2>

        {projects.length === 0 ? (
          <div className="bg-white rounded-lg shadow-sm border p-8 text-center">
            <p className="text-gray-500 mb-4">No projects yet. Create your first project!</p>
            <button
              onClick={() => setShowForm(true)}
              className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
            >
              + Create Project
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {projects.map((p) => (
              <div
                key={p.id}
                onClick={() => navigate(`/projects/${p.id}`)}
                className="bg-white rounded-lg shadow-sm border p-5 cursor-pointer hover:shadow-md hover:border-blue-300 transition-all"
              >
                <h3 className="font-semibold text-lg text-gray-800 mb-2">{p.name}</h3>
                <p className="text-gray-500 text-sm line-clamp-2">
                  {p.description || "No description"}
                </p>
                <div className="mt-3 flex justify-between items-center">
                  <span className="inline-block bg-blue-100 text-blue-700 text-xs px-2 py-1 rounded">
                    {p.status}
                  </span>
                  <div className="flex gap-2">
                    <button
                      onClick={(e) => handleEdit(e, p)}
                      className="text-gray-400 hover:text-blue-600 text-sm px-2 py-1 rounded hover:bg-blue-50"
                    >
                      Edit
                    </button>
                    <button
                      onClick={(e) => handleDelete(e, p.id)}
                      className="text-gray-400 hover:text-red-600 text-sm px-2 py-1 rounded hover:bg-red-50"
                    >
                      Delete
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
