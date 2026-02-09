import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

type Props = {
  children: React.ReactNode;
};

export default function ProtectedRoute({ children }: Props) {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    // User not logged in, redirect to login page
    return <Navigate to="/" replace />;
  }

  // User is authenticated, show the page
  return <>{children}</>;
}
