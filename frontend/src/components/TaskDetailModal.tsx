import React, { useEffect, useRef, useState } from 'react'
import api from '../api/axios'
import { useAuth } from '../context/AuthContext'
import { useToast } from '../context/ToastContext'
import ConfirmDialog from './ConfirmDialog'

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

const PRESET_LABELS = [
  { name: 'Bug', color: '#ef4444' },
  { name: 'Feature', color: '#8b5cf6' },
  { name: 'Design', color: '#ec4899' },
  { name: 'Blocker', color: '#f97316' },
  { name: 'Enhancement', color: '#3b82f6' },
  { name: 'Docs', color: '#10b981' },
]

const TaskDetailModal: React.FC<Props> = ({ task, projectId, onClose, onStatusChange, onDelete }) => {
  const { user } = useAuth()
  const { toast } = useToast()
  const [comments, setComments] = useState<Comment[]>([])
  const [commentBody, setCommentBody] = useState('')
  const [loadingComments, setLoadingComments] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [currentStatus, setCurrentStatus] = useState(task.status)
  const [confirmDelete, setConfirmDelete] = useState(false)
  const [labels, setLabels] = useState<Label[]>(task.labels ?? [])
  const backdropRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    fetchComments()
    const handler = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose() }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [])

  const fetchComments = () => {
    setLoadingComments(true)
    api.get<Comment[]>(`/tasks/${task.id}/comments`)
      .then(({ data }) => setComments(data))
      .catch(() => toast('Failed to load comments', 'error'))
      .finally(() => setLoadingComments(false))
  }

  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === backdropRef.current) onClose()
  }

  const handleStatusChange = async (status: string) => {
    try {
      await api.patch(`/projects/${projectId}/tasks/${task.id}/status`, { status })
      setCurrentStatus(status)
      onStatusChange(task.id, status)
      toast('Status updated')
    } catch {
      toast('Failed to update status', 'error')
    }
  }

  const handleSubmitComment = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!commentBody.trim()) return
    setSubmitting(true)
    try {
      await api.post(`/tasks/${task.id}/comments`, { body: commentBody.trim() })
      setCommentBody('')
      fetchComments()
      toast('Comment added')
    } catch {
      toast('Failed to add comment', 'error')
    } finally {
      setSubmitting(false)
    }
  }

  const handleDeleteComment = async (commentId: string) => {
    try {
      await api.delete(`/tasks/${task.id}/comments/${commentId}`)
      setComments(prev => prev.filter(c => c.id !== commentId))
      toast('Comment deleted')
    } catch {
      toast('Failed to delete comment', 'error')
    }
  }

  const handleToggleLabel = async (preset: { name: string; color: string }) => {
    const existing = labels.find(l => l.name === preset.name)
    if (existing) {
      try {
        await api.delete(`/projects/${projectId}/tasks/${task.id}/labels/${existing.id}`)
        setLabels(prev => prev.filter((l: Label) => l.id !== existing.id))
      } catch {
        toast('Failed to remove label', 'error')
      }
    } else {
      try {
        const { data } = await api.post<Label>(`/projects/${projectId}/tasks/${task.id}/labels`, preset)
        setLabels((prev: Label[]) => [...prev, data])
      } catch {
        toast('Failed to add label', 'error')
      }
    }
  }

  return (
    <>
      <div className="modal-backdrop" ref={backdropRef} onClick={handleBackdropClick}>
        <div className="modal">
          <div className="modal-header">
            <div className="modal-header-left">
              <span className={`priority-badge ${priorityClass[task.priority] ?? 'medium'}`}>{task.priority}</span>
              <span className={`badge ${statusBadge[currentStatus] ?? 'badge-default'}`}>
                {statusLabel[currentStatus] ?? currentStatus}
              </span>
            </div>
            <div className="modal-header-right">
              <button className="btn btn-danger btn-sm" onClick={() => setConfirmDelete(true)}>Delete task</button>
              <button className="modal-close" onClick={onClose} aria-label="Close">✕</button>
            </div>
          </div>

          <div className="modal-body">
            <div className="modal-main">
              <h2 className="modal-title">{task.title}</h2>
              {task.description ? (
                <p className="modal-description">{task.description}</p>
              ) : (
                <p className="modal-description empty">No description provided.</p>
              )}

              <div className="modal-comments">
                <h4 className="modal-section-title">
                  Comments <span className="comment-count">{comments.length}</span>
                </h4>

                {loadingComments ? (
                  <div className="loading" style={{ padding: '16px 0' }}>
                    <div className="spinner" />Loading...
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
                            <span className="comment-date">{new Date(c.createdAt).toLocaleString()}</span>
                            {c.authorEmail === user?.email && (
                              <button className="comment-delete" onClick={() => handleDeleteComment(c.id)}>
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

                <form className="comment-form" onSubmit={handleSubmitComment}>
                  <div className="comment-input-wrap">
                    <textarea
                      value={commentBody}
                      onChange={e => setCommentBody(e.target.value)}
                      placeholder="Add a comment..."
                      rows={3}
                      maxLength={2000}
                      onKeyDown={e => {
                        if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
                          e.preventDefault()
                          handleSubmitComment(e as unknown as React.FormEvent)
                        }
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

            <aside className="modal-sidebar">
              <div className="sidebar-section">
                <p className="sidebar-label">Status</p>
                <select className="select-sm" value={currentStatus} onChange={e => handleStatusChange(e.target.value)}>
                  {STATUS_OPTIONS.map(s => (
                    <option key={s} value={s}>{statusLabel[s] ?? s}</option>
                  ))}
                </select>
              </div>

              <div className="sidebar-section">
                <p className="sidebar-label">Priority</p>
                <span className={`priority-badge ${priorityClass[task.priority] ?? 'medium'}`}>{task.priority}</span>
              </div>

              <div className="sidebar-section">
                <p className="sidebar-label">Labels</p>
                <div className="label-chips">
                  {labels.map((l: Label) => (
                    <span
                      key={l.id}
                      className="label-chip"
                      style={{ backgroundColor: l.color }}
                    >
                      {l.name}
                      <button className="label-chip-remove" onClick={() => handleToggleLabel(l)}>×</button>
                    </span>
                  ))}
                </div>
                <div className="label-presets">
                  {PRESET_LABELS.map(p => (
                    <button
                      key={p.name}
                      className={`label-preset${labels.some((l: Label) => l.name === p.name) ? ' active' : ''}`}
                      style={{ '--label-color': p.color } as React.CSSProperties}
                      onClick={() => handleToggleLabel(p)}
                    >
                      {p.name}
                    </button>
                  ))}
                </div>
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

      {confirmDelete && (
        <ConfirmDialog
          message="Delete this task? This cannot be undone."
          confirmLabel="Delete"
          danger
          onConfirm={() => { onDelete(task.id); onClose() }}
          onCancel={() => setConfirmDelete(false)}
        />
      )}
    </>
  )
}

export default TaskDetailModal
