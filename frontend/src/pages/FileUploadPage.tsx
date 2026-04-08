import React, { useRef, useState } from 'react'
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
  const inputRef = useRef<HTMLInputElement>(null)

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFile(e.target.files?.[0] ?? null)
    setUploaded(null)
    setError('')
  }

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
      if (inputRef.current) inputRef.current.value = ''
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
          <div>
            <h2>File Upload</h2>
            <p className="text-muted" style={{ marginTop: 4 }}>Files are stored securely in Azure Blob Storage</p>
          </div>
        </div>

        <div className="card form-card">
          <h3>Upload a file</h3>

          {error && <div className="alert alert-error">{error}</div>}

          <form onSubmit={handleUpload}>
            <div className="form-group">
              <label>Select file <span style={{ color: 'var(--text-subtle)', fontWeight: 400 }}>(max 50 MB)</span></label>
              <label className={`upload-zone${file ? ' has-file' : ''}`} htmlFor="file-input">
                <div className="upload-zone-icon">{file ? '📄' : '☁️'}</div>
                {file ? (
                  <>
                    <p style={{ fontWeight: 600, color: 'var(--primary)' }}>{file.name}</p>
                    <small>{(file.size / 1024 / 1024).toFixed(2)} MB — click to change</small>
                  </>
                ) : (
                  <>
                    <p>Click to browse files</p>
                    <small>Any file type up to 50 MB</small>
                  </>
                )}
              </label>
              <input
                id="file-input"
                ref={inputRef}
                type="file"
                onChange={handleFileChange}
                style={{ display: 'none' }}
              />
            </div>

            <button
              type="submit"
              className="btn btn-primary"
              disabled={uploading || !file}
              style={{ marginTop: 8 }}
            >
              {uploading ? 'Uploading...' : 'Upload file'}
            </button>
          </form>
        </div>

        {uploaded && (
          <div className="card upload-result">
            <div className="alert alert-success" style={{ marginBottom: 16 }}>
              File uploaded successfully!
            </div>
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
