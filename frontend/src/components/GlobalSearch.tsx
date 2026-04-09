import React, { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../api/axios'

interface SearchItem {
  id: string
  title: string
  subtitle?: string
  projectId?: string
}

interface SearchResult {
  projects: SearchItem[]
  tasks: SearchItem[]
}

const GlobalSearch: React.FC = () => {
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<SearchResult | null>(null)
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)
  const wrapperRef = useRef<HTMLDivElement>(null)
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const navigate = useNavigate()

  useEffect(() => {
    const handleKey = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault()
        inputRef.current?.focus()
        setOpen(true)
      }
      if (e.key === 'Escape') {
        setOpen(false)
        inputRef.current?.blur()
      }
    }
    window.addEventListener('keydown', handleKey)
    return () => window.removeEventListener('keydown', handleKey)
  }, [])

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (wrapperRef.current && !wrapperRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value
    setQuery(val)

    if (debounceRef.current) clearTimeout(debounceRef.current)

    if (val.trim().length < 2) {
      setResults(null)
      return
    }

    debounceRef.current = setTimeout(async () => {
      setLoading(true)
      try {
        const { data } = await api.get<SearchResult>(`/search?q=${encodeURIComponent(val)}`)
        setResults(data)
        setOpen(true)
      } finally {
        setLoading(false)
      }
    }, 280)
  }

  const handleSelect = (type: 'project' | 'task', item: SearchItem) => {
    setQuery('')
    setResults(null)
    setOpen(false)
    if (type === 'project') navigate(`/projects/${item.id}`)
    else navigate(`/projects/${item.projectId}`)
  }

  const hasResults = results && (results.projects.length > 0 || results.tasks.length > 0)

  return (
    <div className="search-wrapper" ref={wrapperRef}>
      <div className="search-input-wrap">
        <span className="search-icon">⌕</span>
        <input
          ref={inputRef}
          className="search-input"
          type="text"
          placeholder="Search…"
          value={query}
          onChange={handleChange}
          onFocus={() => { if (results) setOpen(true) }}
          autoComplete="off"
          spellCheck={false}
        />
        <span className="search-shortcut">Ctrl K</span>
      </div>

      {open && query.length >= 2 && (
        <div className="search-dropdown">
          {loading && <div className="search-loading">Searching...</div>}

          {!loading && !hasResults && (
            <div className="search-empty">No results for "{query}"</div>
          )}

          {!loading && results && results.projects.length > 0 && (
            <div className="search-group">
              <p className="search-group-label">Projects</p>
              {results.projects.map(item => (
                <button key={item.id} className="search-item" onClick={() => handleSelect('project', item)}>
                  <span className="search-item-icon">📋</span>
                  <span className="search-item-body">
                    <span className="search-item-title">{item.title}</span>
                    {item.subtitle && <span className="search-item-sub">{item.subtitle}</span>}
                  </span>
                </button>
              ))}
            </div>
          )}

          {!loading && results && results.tasks.length > 0 && (
            <div className="search-group">
              <p className="search-group-label">Tasks</p>
              {results.tasks.map(item => (
                <button key={item.id} className="search-item" onClick={() => handleSelect('task', item)}>
                  <span className="search-item-icon">✅</span>
                  <span className="search-item-body">
                    <span className="search-item-title">{item.title}</span>
                    {item.subtitle && <span className="search-item-sub">in {item.subtitle}</span>}
                  </span>
                </button>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  )
}

export default GlobalSearch
