import React from 'react'

interface Props {
  message: string
  confirmLabel?: string
  danger?: boolean
  onConfirm: () => void
  onCancel: () => void
}

const ConfirmDialog: React.FC<Props> = ({
  message,
  confirmLabel = 'Confirm',
  danger = false,
  onConfirm,
  onCancel,
}) => (
  <div className="modal-backdrop" onClick={onCancel}>
    <div className="confirm-dialog" onClick={e => e.stopPropagation()}>
      <p className="confirm-message">{message}</p>
      <div className="confirm-actions">
        <button className="btn btn-secondary" onClick={onCancel}>Cancel</button>
        <button
          className={`btn ${danger ? 'btn-danger' : 'btn-primary'}`}
          onClick={onConfirm}
          autoFocus
        >
          {confirmLabel}
        </button>
      </div>
    </div>
  </div>
)

export default ConfirmDialog
