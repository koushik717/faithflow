import { Toaster } from 'react-hot-toast';
import { BrowserRouter, Routes, Route, Navigate, Link } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { useEffect } from 'react';
import { queryClient } from './lib/queryClient';
import { useAuthStore } from './store/auth';
import api from './lib/api';

// Layouts
import { MainLayout } from './components/layout/MainLayout';

// Pages
import { Dashboard } from './pages/Dashboard';
import { Login } from './pages/auth/Login';
import { Register } from './pages/auth/Register';
import { PrayerWall } from './pages/PrayerWall';
import Groups from './pages/Groups';
import Volunteer from './pages/Volunteer';
import SermonNotes from './pages/SermonNotes';
import Admin from './pages/Admin';

function NotFound() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-surface text-center px-4">
      <div className="glass p-12 rounded-3xl max-w-md w-full">
        <p className="text-8xl font-black text-primary/20 mb-4">404</p>
        <h1 className="text-2xl font-bold text-slate-800 mb-2">Page not found</h1>
        <p className="text-slate-500 mb-8">The page you're looking for doesn't exist or has been moved.</p>
        <Link to="/" className="inline-flex items-center gap-2 bg-primary text-white px-6 py-3 rounded-2xl font-bold hover:bg-primary-600 transition-colors">
          Back to Home
        </Link>
      </div>
    </div>
  );
}

function RoleGuard({ children, requiredRole }: { children: React.ReactNode; requiredRole: string }) {
  const { user, isLoading } = useAuthStore();
  
  if (isLoading) {
    return <div className="min-h-screen flex items-center justify-center bg-surface"><div className="animate-spin h-8 w-8 border-4 border-primary border-t-transparent rounded-full"></div></div>;
  }
  
  if (!user || user.role !== requiredRole) {
    return <Navigate to="/" replace />;
  }
  
  return <>{children}</>;
}

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuthStore();
  
  if (isLoading) {
    return <div className="min-h-screen flex items-center justify-center bg-surface"><div className="animate-spin h-8 w-8 border-4 border-primary border-t-transparent rounded-full"></div></div>;
  }
  
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
}

function App() {
  const { setUser, setLoading } = useAuthStore();

  useEffect(() => {
    const initAuth = async () => {
      const token = localStorage.getItem('token');
      if (token) {
        try {
          const response = await api.get('/Users/profile');
          setUser(response.data);
        } catch (error) {
          localStorage.removeItem('token');
          localStorage.removeItem('refreshToken');
        }
      }
      setLoading(false);
    };

    initAuth();
  }, [setUser, setLoading]);

  return (
    <QueryClientProvider client={queryClient}>
      <Toaster position="top-right" toastOptions={{ className: 'glass' }} />
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="*" element={<NotFound />} />
          
          <Route path="/" element={
            <ProtectedRoute>
              <MainLayout />
            </ProtectedRoute>
          }>
            <Route index element={<Dashboard />} />
            <Route path="prayer" element={<PrayerWall />} />
            <Route path="groups" element={<Groups />} />
            <Route path="volunteer" element={<Volunteer />} />
            <Route path="sermons" element={<SermonNotes />} />
            <Route path="admin" element={
              <RoleGuard requiredRole="Admin">
                <Admin />
              </RoleGuard>
            } />
            {/* Add other protected routes here */}
          </Route>
        </Routes>
      </BrowserRouter>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}

export default App;
