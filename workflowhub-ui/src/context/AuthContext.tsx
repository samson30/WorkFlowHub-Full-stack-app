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
    setIsAuthenticated(!!token); // true if token exists
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
