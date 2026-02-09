# WorkFlowHub UI - Frontend Integration Documentation

This document summarizes all the integration work done to connect the React frontend with the .NET backend API.

---

## Overview

| Phase | Task | Status |
|-------|------|--------|
| 1 | Environment Variable Setup | ✅ Completed |
| 2 | Consolidate API Layer | ✅ Completed |
| 3 | Auth Context (Global State) | ✅ Completed |
| 4 | Protected Routes | 🔲 Pending |
| 5 | Logout Functionality | 🔲 Pending |

---

## Phase 1: Environment Variable Setup

### Problem
API URL was hardcoded in multiple files:
```typescript
// ❌ Before - Hardcoded in every API file
const API_URL = "http://localhost:5296/api";
```

### Why It's Bad
- URL scattered across multiple files
- Hard to change for different environments (dev/staging/production)
- Easy to forget to update one file

### Solution

**Step 1: Created `.env` file in project root:**
```
REACT_APP_API_URL=http://localhost:5296/api
```

**Step 2: Updated axios instance to use environment variable:**
```typescript
// src/api/api.ts
const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL,
});
```

### Key Learning
- Create React App requires `REACT_APP_` prefix for environment variables
- Environment variables are embedded at build time
- Must restart dev server after changing `.env`
- For production, set the variable in your hosting platform (Vercel, Netlify, etc.)

---

## Phase 2: Consolidate API Layer

### Problem
Mixed API patterns across the codebase:
- `api.ts` → Axios with interceptor
- `authApi.ts` → Native fetch
- `projectApi.ts` → Native fetch
- `taskApi.ts` → Axios

### Why It's Bad
- Duplicate code for headers/authentication
- Inconsistent error handling
- Multiple places to update if API URL changes
- Some files had auth token logic, others didn't

### Solution
Converted all API files to use the central axios instance.

**Central Axios Instance (`src/api/api.ts`):**
```typescript
import axios from "axios";
import { getToken } from "../utils/auth";

const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL,
});

// Automatically add auth token to every request
api.interceptors.request.use(config => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
```

**Updated `authApi.ts`:**
```typescript
import api from "./api";

export const login = async (email: string, password: string) => {
  const res = await api.post("/auth/login", { email, password });
  return res.data.token;
};

export const register = async (email: string, password: string) => {
  await api.post("/auth/register", { email, password });
};
```

**Updated `projectApi.ts`:**
```typescript
import api from "./api";

export const getProjects = async () => {
  const res = await api.get("/projects");
  return res.data;
};

export const getProjectById = async (projectId: number) => {
  const res = await api.get(`/projects/${projectId}`);
  return res.data;
};

export const createProject = async (data: { name: string; description: string }) => {
  const res = await api.post("/projects", {
    name: data.name,
    description: data.description,
    status: "Active"
  });
  return res.data;
};
```

**`taskApi.ts` (already using axios):**
```typescript
import api from "./api";

export const getTasks = async (projectId: number) => {
  const res = await api.get(`/projects/${projectId}/tasks`);
  return res.data;
};

export const createTask = async (
  projectId: number,
  data: { title: string; description: string; status: 0 }
) => {
  const res = await api.post(`/projects/${projectId}/tasks`, data);
  return res.data;
};

export const deleteTask = async (projectId: number, taskId: number) => {
  await api.delete(`/projects/${projectId}/tasks/${taskId}`);
};
```

### Benefits After Consolidation

| Benefit | Explanation |
|---------|-------------|
| Single source of truth | All API calls go through one instance |
| Automatic auth | Interceptor adds token to every request |
| Cleaner code | No manual headers in each function |
| Easier maintenance | Change URL in one place |
| Consistent error handling | Axios throws on HTTP errors automatically |

### Axios vs Fetch Comparison

| Feature | Axios | Fetch |
|---------|-------|-------|
| Auto JSON parsing | ✅ `res.data` | ❌ Need `res.json()` |
| Auto JSON stringify | ✅ Automatic | ❌ Need `JSON.stringify()` |
| Error handling | ✅ Throws on 4xx/5xx | ❌ Need manual check |
| Interceptors | ✅ Built-in | ❌ Not available |
| Request cancellation | ✅ Easy | ❌ Complex |

---

## Phase 3: Auth Context (Global Authentication State)

### Problem
- No global way to know if user is logged in
- Each component managed auth independently
- No logout functionality
- Can't protect routes

### Why It's Bad
- Duplicate auth logic in components
- Components can't react to auth state changes
- No centralized place to manage authentication

### Solution
Created a React Context for authentication state.

**New File: `src/context/AuthContext.tsx`**
```typescript
import { createContext, useContext, useState, useEffect, ReactNode } from "react";
import { getToken, saveToken, logout as removeToken } from "../utils/auth";

// Define what the context will provide
type AuthContextType = {
  isAuthenticated: boolean;
  login: (token: string) => void;
  logout: () => void;
};

// Create the context
const AuthContext = createContext<AuthContextType | null>(null);

// Provider component that wraps the app
export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  // Check if user is already logged in on app load
  useEffect(() => {
    const token = getToken();
    setIsAuthenticated(!!token);
  }, []);

  const login = (token: string) => {
    saveToken(token);
    setIsAuthenticated(true);
  };

  const logout = () => {
    removeToken();
    setIsAuthenticated(false);
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

// Custom hook to use the auth context
export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return context;
}
```

**Updated `src/index.tsx`:**
```typescript
import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import { BrowserRouter } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";

const root = ReactDOM.createRoot(
  document.getElementById("root") as HTMLElement
);

root.render(
  <React.StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <App />
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>
);
```

**Updated `src/auth/Login.tsx`:**
```typescript
import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { login as loginApi } from "../api/authApi";
import { useAuth } from "../context/AuthContext";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleLogin = async () => {
    try {
      setError("");
      const token = await loginApi(email, password);
      login(token); // Save token via context
      navigate("/dashboard");
    } catch {
      setError("Invalid email or password");
    }
  };

  return (
    <div style={{ padding: "40px" }}>
      <h2>Login</h2>

      {error && <p style={{ color: "red" }}>{error}</p>}

      <input
        placeholder="Email"
        value={email}
        onChange={e => setEmail(e.target.value)}
      />
      <br /><br />

      <input
        type="password"
        placeholder="Password"
        value={password}
        onChange={e => setPassword(e.target.value)}
      />
      <br /><br />

      <button onClick={handleLogin}>Login</button>

      <p>
        Don't have an account? <Link to="/register">Register</Link>
      </p>
    </div>
  );
}
```

### How Auth Context Works

```
┌─────────────────────────────────────────────────────────────┐
│                        AuthProvider                         │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  State: isAuthenticated (true/false)                │   │
│  │  Functions: login(), logout()                       │   │
│  └─────────────────────────────────────────────────────┘   │
│                           │                                 │
│              Provides via Context                           │
│                           │                                 │
│    ┌──────────────────────┼──────────────────────┐         │
│    ▼                      ▼                      ▼         │
│ Login.tsx            Dashboard.tsx         Any Component   │
│ useAuth()            useAuth()              useAuth()      │
└─────────────────────────────────────────────────────────────┘
```

### Key Learning

| Concept | Explanation |
|---------|-------------|
| **Context** | React's way to share data across components without prop drilling |
| **Provider** | Component that holds and provides the shared state |
| **useContext** | Hook to access context values |
| **Custom Hook** | `useAuth()` makes it easy to use the context |

---

## Files Modified/Created

### New Files Created
| File | Purpose |
|------|---------|
| `.env` | Environment variables (API URL) |
| `src/context/AuthContext.tsx` | Global authentication state |

### Files Modified
| File | Changes |
|------|---------|
| `src/api/api.ts` | Uses environment variable for baseURL |
| `src/api/authApi.ts` | Converted to axios, returns token |
| `src/api/projectApi.ts` | Converted to axios |
| `src/index.tsx` | Wrapped app with AuthProvider |
| `src/auth/Login.tsx` | Uses useAuth hook |
| `src/pages/Dashboard.tsx` | Fixed import path |

### Files Deleted
| File | Reason |
|------|--------|
| `src/services/projectService.ts` | Duplicate of projectApi |
| `src/services/api.ts` | Duplicate fetch wrapper |

---

## Current Project Structure

```
workflowhub-ui/
├── .env                          # Environment variables
├── .gitignore                    # Git ignore rules
├── package.json
├── src/
│   ├── api/
│   │   ├── api.ts               # Axios instance with interceptor
│   │   ├── authApi.ts           # Login/Register API calls
│   │   ├── projectApi.ts        # Project CRUD API calls
│   │   └── taskApi.ts           # Task CRUD API calls
│   ├── auth/
│   │   ├── Login.tsx            # Login page
│   │   └── Register.tsx         # Register page
│   ├── context/
│   │   └── AuthContext.tsx      # Global auth state
│   ├── pages/
│   │   ├── Dashboard.tsx        # Projects list
│   │   ├── ProjectDetails.tsx   # Tasks within project
│   │   └── CreateProject.tsx    # Create project form
│   ├── types/
│   │   └── task.ts              # TypeScript interfaces
│   ├── utils/
│   │   └── auth.ts              # Token storage utilities
│   ├── App.tsx                  # Routes configuration
│   ├── App.css
│   ├── index.tsx                # Entry point with providers
│   └── index.css
```

---

## Pending Work

### Phase 4: Protected Routes
Create a component that redirects to login if user is not authenticated:
```typescript
// src/components/ProtectedRoute.tsx
function ProtectedRoute({ children }) {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/" />;
  }

  return children;
}
```

### Phase 5: Logout Functionality
Add a logout button to Dashboard that calls `logout()` from useAuth.

### Phase 6: Register Page Update
Update Register.tsx to use the same patterns as Login.tsx.

---

## API Endpoints Reference

| Frontend Function | HTTP Method | Backend Endpoint |
|-------------------|-------------|------------------|
| `login()` | POST | `/api/auth/login` |
| `register()` | POST | `/api/auth/register` |
| `getProjects()` | GET | `/api/projects` |
| `getProjectById()` | GET | `/api/projects/{id}` |
| `createProject()` | POST | `/api/projects` |
| `getTasks()` | GET | `/api/projects/{id}/tasks` |
| `createTask()` | POST | `/api/projects/{id}/tasks` |
| `deleteTask()` | DELETE | `/api/projects/{id}/tasks/{taskId}` |

---

## Summary

This integration work established:

1. **Environment-based configuration** - API URL in `.env` file
2. **Unified API layer** - All calls through single axios instance
3. **Automatic authentication** - Interceptor adds JWT to all requests
4. **Global auth state** - Context provides isAuthenticated, login, logout

The frontend is now properly structured to communicate with the backend API and manage user authentication state globally.
