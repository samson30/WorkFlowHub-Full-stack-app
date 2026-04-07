import axios from 'axios'

// Token lives in memory only — never in localStorage
let memoryToken: string | null = null

export const setToken = (token: string | null) => {
  memoryToken = token
}

export const getToken = () => memoryToken

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

// Attach token to every request
api.interceptors.request.use((config) => {
  if (memoryToken) {
    config.headers.Authorization = `Bearer ${memoryToken}`
  }
  return config
})

// On 401 clear token and redirect to login
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      memoryToken = null
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default api
