import React, { useState } from 'react'
import api from '../api/axios'
import Navbar from '../components/Navbar'

interface FileRecord {
  id: string
  fileName: string
  blobUrl: string
  createdAt: string
}

const FileUploadPage: React.FC = () => {
  const [file, setFile] = useState<File | null>(null)
  const [uploading, setUploading] = useState(false)
  const [uploaded, setUploaded] = useState<FileRecord | null>(null)
  const [error, setError] = useState('')

  const handleUpload = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!file) return
    setError('')
    setUploading(true)

    const formData = new FormData()
    formData.append('file', file)

    try {
      const { data } = await api.post<FileRecord>('/files/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      setUploaded(data)
      setFile(null)
      ;(e.target as HTMLFormElement).reset()
    } catch {
      setError('Upload failed. Please try again.')
    } finally {
      setUploading(false)
    }
  }

  return (
    <>
      <Navbar />
      <main className="container">
        <div className="page-header">
          <h2>File Upload</h2>
        </div>

        <div className="card form-card">
          <h3>Upload a file</h3>
          <p className="text-muted">Files are stored in Azure Blob Storage. Max size: 50MB.</p>
          {error && <div className="alert alert-error">{error}</div>}
          <form onSubmit={handleUpload}>
            <div className="form-group">
              <label>Select file</label>
              <input
                type="file"
                onChange={(e) => setFile(e.target.files?.[0] ?? null)}
                required
              />
            </div>
            <button type="submit" className="btn btn-primary" disabled={uploading || !file}>
              {uploading ? 'Uploading...' : 'Upload'}
            </button>
          </form>
        </div>

        {uploaded && (
          <div className="card upload-result">
            <h3>Upload successful</h3>
            <table className="detail-table">
              <tbody>
                <tr><th>File name</th><td>{uploaded.fileName}</td></tr>
                <tr><th>Uploaded at</th><td>{new Date(uploaded.createdAt).toLocaleString()}</td></tr>
                <tr>
                  <th>URL</th>
                  <td>
                    <a href={uploaded.blobUrl} target="_blank" rel="noreferrer" className="link">
                      {uploaded.blobUrl}
                    </a>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        )}
      </main>
    </>
  )
}

export default FileUploadPage
