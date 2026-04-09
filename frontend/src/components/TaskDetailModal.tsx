import React, { useEffect, useRef, useState } from 'react'
import api from '../api/axios'
import { useAuth } from '../context/AuthContext'

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

interface Comment {
  id: string
  body: string
  authorEmail: string
  createdAt: string
}

interface Props {
  task: Task
  projectId: string
  onClose: () => void
  onStatusChange: (taskId: string, status: string) => void
  onDelete: (taskId: string) => void
}

const STATUS_OPTIONS = ['Todo', 'InProgress', 'Done', 'Cancelled']
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

const TaskDetailModal: React.FC<Props> = ({ task, projectId, onClose, onStatusChange, onDelete }) => {
  const { user } = useAuth()
  const [comments, setComments] = useState<Comment[]>([])
  const [commentBody, setCommentBody] = useState('')
  const [loadingComments, setLoadingComments] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [currentStatus, setCurrentStatus] = useState(task.status)
  const backdropRef = useRef<HTMLDivElement>(null)
  const textareaRef = useRef<HTMLTextAreaElement>(null)

  useEffect(() => {
    fetchComments()
    // close on Escape
    const handler = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose() }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [])

  const fetchComments = () => {
    setLoadingComments(true)
    api.get<Comment[]>(`/tasks/${task.id}/comments`)
      .then(({ data }) => setComments(data))
      .finally(() => setLoadingComments(false))
  }

  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === backdropRef.current) onClose()
  }

  const handleStatusChange = async (status: string) => {
    setCurrentStatus(status)
    await api.patch(`/projects/${projectId}/tasks/${task.id}/status`, { status })
    onStatusChange(task.id, status)
  }

  const handleSubmitComment = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!commentBody.trim()) return
    setSubmitting(true)
    try {
      await api.post(`/tasks/${task.id}/comments`, { body: commentBody.trim() })
      setCommentBody('')
      fetchComments()
    } finally {
      setSubmitting(false)
    }
  }

  const handleDeleteComment = async (commentId: string) => {
    await api.delete(`/tasks/${task.id}/comments/${commentId}`)
    setComments(prev => prev.filter(c => c.id !== commentId))
  }

  const handleDelete = () => {
    onDelete(task.id)
    onClose()
  }

  return (
    <div className="modal-backdrop" ref={backdropRef} onClick={handleBackdropClick}>
      <div className="modal">
        {/* Header */}
        <div className="modal-header">
          <div className="modal-header-left">
            <span className={`priority-badge ${priorityClass[task.priority] ?? 'medium'}`}>{task.priority}</span>
            <span className={`badge ${statusBadge[currentStatus] ?? 'badge-default'}`}>
              {statusLabel[currentStatus] ?? currentStatus}
            </span>
          </div>
          <div className="modal-header-right">
            <button className="btn btn-danger btn-sm" onClick={handleDelete}>Delete task</button>
            <button className="modal-close" onClick={onClose} aria-label="Close">✕</button>
          </div>
        </div>

        <div className="modal-body">
          {/* Main content */}
          <div className="modal-main">
            <h2 className="modal-title">{task.title}</h2>
            {task.description ? (
              <p className="modal-description">{task.description}</p>
            ) : (
              <p className="modal-description empty">No description provided.</p>
            )}

            {/* Comments */}
            <div className="modal-comments">
              <h4 className="modal-section-title">
                Comments <span className="comment-count">{comments.length}</span>
              </h4>

              {loadingComments ? (
                <div className="loading" style={{ padding: '16px 0' }}>
                  <div className="spinner" />Loading comments...
                </div>
              ) : (
                <div className="comment-list">
                  {comments.length === 0 && (
                    <p className="comment-empty">No comments yet. Be the first to add one.</p>
                  )}
                  {comments.map(c => (
                    <div key={c.id} className="comment-item">
                      <div className="comment-avatar">
                        {c.authorEmail[0].toUpperCase()}
                      </div>
                      <div className="comment-content">
                        <div className="comment-meta">
                          <span className="comment-author">{c.authorEmail}</span>
                          <span className="comment-date">
                            {new Date(c.createdAt).toLocaleString()}
                          </span>
                          {c.authorEmail === user?.email && (
                            <button
                              className="comment-delete"
                              onClick={() => handleDeleteComment(c.id)}
                            >
                              Delete
                            </button>
                          )}
                        </div>
                        <p className="comment-body">{c.body}</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {/* Add comment */}
              <form className="comment-form" onSubmit={handleSubmitComment}>
                <div className="comment-input-wrap">
                  <textarea
                    ref={textareaRef}
                    value={commentBody}
                    onChange={e => setCommentBody(e.target.value)}
                    placeholder="Add a comment..."
                    rows={3}
                    maxLength={2000}
                    onKeyDown={e => {
                      if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) handleSubmitComment(e as any)
                    }}
                  />
                  <div className="comment-form-actions">
                    <span className="comment-hint">Ctrl+Enter to submit</span>
                    <button
                      type="submit"
                      className="btn btn-primary btn-sm"
                      disabled={submitting || !commentBody.trim()}
                    >
                      {submitting ? 'Saving...' : 'Comment'}
                    </button>
                  </div>
                </div>
              </form>
            </div>
          </div>

          {/* Sidebar */}
          <aside className="modal-sidebar">
            <div className="sidebar-section">
              <p className="sidebar-label">Status</p>
              <select
                className="select-sm"
                value={currentStatus}
                onChange={e => handleStatusChange(e.target.value)}
              >
                {STATUS_OPTIONS.map(s => (
                  <option key={s} value={s}>{statusLabel[s] ?? s}</option>
                ))}
              </select>
            </div>

            <div className="sidebar-section">
              <p className="sidebar-label">Priority</p>
              <span className={`priority-badge ${priorityClass[task.priority] ?? 'medium'}`}>
                {task.priority}
              </span>
            </div>

            <div className="sidebar-section">
              <p className="sidebar-label">Due Date</p>
              <p className="sidebar-value">
                {task.dueDate ? new Date(task.dueDate).toLocaleDateString(undefined, {
                  year: 'numeric', month: 'short', day: 'numeric'
                }) : '—'}
              </p>
            </div>

            {task.assignedUserEmail && (
              <div className="sidebar-section">
                <p className="sidebar-label">Assignee</p>
                <p className="sidebar-value">{task.assignedUserEmail}</p>
              </div>
            )}

            <div className="sidebar-section">
              <p className="sidebar-label">Created</p>
              <p className="sidebar-value">
                {new Date(task.createdAt).toLocaleDateString(undefined, {
                  year: 'numeric', month: 'short', day: 'numeric'
                })}
              </p>
            </div>
          </aside>
        </div>
      </div>
    </div>
  )
}

export default TaskDetailModal
