import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { getTasks, createTask, deleteTask, updateTaskStatus, updateTask } from "../api/taskApi";
import { Task, TaskStatus } from "../types/task";
import KanbanBoard from "../components/KanbanBoard";

export default function ProjectDetails() {
  const { id } = useParams();
  const projectId = Number(id);

  const [tasks, setTasks] = useState<Task[]>([]);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingTask, setEditingTask] = useState<Task | null>(null);
  const [editTitle, setEditTitle] = useState("");
  const [editDescription, setEditDescription] = useState("");

  useEffect(() => {
    loadTasks();
  }, []);

  const loadTasks = async () => {
    try {
      const data = await getTasks(projectId);
      setTasks(data);
    } catch (err) {
      console.error("Failed to load tasks");
    } finally {
      setLoading(false);
    }
  };

  const handleCreateTask = async () => {
    if (!title.trim()) return;

    try {
      await createTask(projectId, { title, description, status: "Todo" });
      setTitle("");
      setDescription("");
      setShowForm(false);
      loadTasks();
    } catch (err) {
      console.error("Failed to create task");
    }
  };

  const handleStatusChange = async (taskId: number, newStatus: TaskStatus) => {
    // Optimistic update
    setTasks((prev) =>
      prev.map((t) => (t.id === taskId ? { ...t, status: newStatus } : t))
    );

    try {
      await updateTaskStatus(projectId, taskId, newStatus);
    } catch (err) {
      // Revert on error
      loadTasks();
      console.error("Failed to update task status");
    }
  };

  const handleDelete = async (taskId: number) => {
    try {
      await deleteTask(projectId, taskId);
      setTasks((prev) => prev.filter((t) => t.id !== taskId));
    } catch (err) {
      console.error("Failed to delete task");
    }
  };

  const handleEditClick = (task: Task) => {
    setEditingTask(task);
    setEditTitle(task.title);
    setEditDescription(task.description || "");
    setShowForm(false);
  };

  const handleEditSubmit = async () => {
    if (!editingTask || !editTitle.trim()) return;

    try {
      await updateTask(projectId, editingTask.id, {
        title: editTitle,
        description: editDescription,
        status: editingTask.status,
      });
      setEditingTask(null);
      loadTasks();
    } catch (err) {
      console.error("Failed to update task");
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <p className="text-gray-500">Loading tasks...</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 py-4 flex justify-between items-center">
          <div className="flex items-center gap-4">
            <Link to="/dashboard" className="text-blue-600 hover:text-blue-800">
              ← Back
            </Link>
            <h1 className="text-xl font-bold text-gray-800">Project Tasks</h1>
          </div>
          <button
            onClick={() => {
              setShowForm(!showForm);
              setEditingTask(null);
            }}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
          >
            + Add Task
          </button>
        </div>
      </div>

      {/* Add Task Form */}
      {showForm && (
        <div className="max-w-7xl mx-auto px-4 py-4">
          <div className="bg-white p-4 rounded-lg shadow-sm border">
            <h3 className="font-semibold mb-3">New Task</h3>
            <input
              placeholder="Task title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="w-full p-2 border rounded mb-2"
            />
            <textarea
              placeholder="Description (optional)"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="w-full p-2 border rounded mb-2"
              rows={2}
            />
            <div className="flex gap-2">
              <button
                onClick={handleCreateTask}
                className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
              >
                Create
              </button>
              <button
                onClick={() => setShowForm(false)}
                className="bg-gray-200 px-4 py-2 rounded hover:bg-gray-300"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Edit Task Form */}
      {editingTask && (
        <div className="max-w-7xl mx-auto px-4 py-4">
          <div className="bg-white p-4 rounded-lg shadow-sm border">
            <h3 className="font-semibold mb-3">Edit Task</h3>
            <input
              placeholder="Task title"
              value={editTitle}
              onChange={(e) => setEditTitle(e.target.value)}
              className="w-full p-2 border rounded mb-2"
            />
            <textarea
              placeholder="Description (optional)"
              value={editDescription}
              onChange={(e) => setEditDescription(e.target.value)}
              className="w-full p-2 border rounded mb-2"
              rows={2}
            />
            <div className="flex gap-2">
              <button
                onClick={handleEditSubmit}
                className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
              >
                Save Changes
              </button>
              <button
                onClick={() => setEditingTask(null)}
                className="bg-gray-200 px-4 py-2 rounded hover:bg-gray-300"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Kanban Board */}
      <div className="max-w-7xl mx-auto">
        <KanbanBoard
          tasks={tasks}
          onStatusChange={handleStatusChange}
          onDelete={handleDelete}
          onEdit={handleEditClick}
        />
      </div>
    </div>
  );
}
