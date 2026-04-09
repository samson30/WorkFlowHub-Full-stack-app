import React from 'react'

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

interface Props {
  tasks: Task[]
  onTaskClick: (task: Task) => void
}

const COLUMNS = [
  { key: 'Todo',       label: 'To Do',      cls: 'col-todo' },
  { key: 'InProgress', label: 'In Progress', cls: 'col-inprogress' },
  { key: 'Done',       label: 'Done',        cls: 'col-done' },
  { key: 'Cancelled',  label: 'Cancelled',   cls: 'col-cancelled' },
]

const priorityClass: Record<string, string> = {
  Low: 'low', Medium: 'medium', High: 'high', Critical: 'critical',
}

const isOverdue = (dueDate: string | null) =>
  dueDate ? new Date(dueDate) < new Date() : false

const KanbanBoard: React.FC<Props> = ({ tasks, onTaskClick }) => {
  const byStatus = (status: string) => tasks.filter((t: Task) => t.status === status)

  return (
    <div className="kanban-board">
      {COLUMNS.map(col => {
        const colTasks = byStatus(col.key)
        return (
          <div key={col.key} className={`kanban-col ${col.cls}`}>
            <div className="kanban-col-header">
              <span className="kanban-col-title">{col.label}</span>
              <span className="kanban-col-count">{colTasks.length}</span>
            </div>
            <div className="kanban-col-body">
              {colTasks.length === 0 && (
                <div className="kanban-empty">No tasks</div>
              )}
              {colTasks.map((task: Task) => (
                <div
                  key={task.id}
                  className={`kanban-card priority-${priorityClass[task.priority] ?? 'medium'}`}
                  onClick={() => onTaskClick(task)}
                >
                  <p className="kanban-card-title">{task.title}</p>
                  {task.description && (
                    <p className="kanban-card-desc">{task.description}</p>
                  )}
                  {task.labels?.length > 0 && (
                    <div className="kanban-card-labels">
                      {task.labels.map((l: Label) => (
                        <span key={l.id} className="label-chip label-chip-sm" style={{ backgroundColor: l.color }}>{l.name}</span>
                      ))}
                    </div>
                  )}
                  <div className="kanban-card-footer">
                    <span className={`priority-badge ${priorityClass[task.priority] ?? 'medium'}`}>
                      {task.priority}
                    </span>
                    {task.dueDate && (
                      <span className={`kanban-due${isOverdue(task.dueDate) && task.status !== 'Done' && task.status !== 'Cancelled' ? ' overdue' : ''}`}>
                        {new Date(task.dueDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}
                      </span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )
      })}
    </div>
  )
}

export default KanbanBoard
