import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '@/store/auth';
import { Home, BookOpen, Users, Heart, Calendar, LogOut, Menu, X } from 'lucide-react';
import { useState } from 'react';

export function MainLayout() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();
  const location = useLocation();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const handleAuthAction = () => {
    if (user) {
      logout();
      navigate('/login');
    } else {
      navigate('/login');
    }
  };

  const navItems = [
    { name: 'Dashboard', path: '/', icon: Home },
    { name: 'Sermons & Notes', path: '/sermons', icon: BookOpen },
    { name: 'Groups', path: '/groups', icon: Users },
    { name: 'Prayer Wall', path: '/prayer', icon: Heart },
    { name: 'Volunteer', path: '/volunteer', icon: Calendar },
  ];

  if (user?.role === 'Admin') {
    navItems.push({ name: 'Admin Panel', path: '/admin', icon: Users });
  }

  const isActive = (path: string) => {
    if (path === '/' && location.pathname !== '/') return false;
    return location.pathname.startsWith(path);
  };

  return (
    <div className="min-h-screen flex flex-col md:flex-row bg-transparent">
      {/* Mobile Navigation Bar */}
      <div className="md:hidden flex items-center justify-between glass p-4 sticky top-0 z-50 rounded-b-3xl mb-4 mx-2">
        <div className="flex items-center gap-2">
          <Heart className="h-6 w-6 text-primary" />
          <span className="font-bold text-xl text-foreground tracking-tight">FaithFlow</span>
        </div>
        <button onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)} className="text-foreground p-2 rounded-xl hover:bg-black/5 transition-colors">
          {isMobileMenuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
        </button>
      </div>

      {/* Sidebar Navigation */}
      <nav className={`${isMobileMenuOpen ? 'block' : 'hidden'} md:block w-full md:w-72 md:m-4 md:rounded-3xl glass flex flex-col transition-all duration-300 ease-in-out`}>
        <div className="hidden md:flex p-6 items-center gap-3">
          <div className="bg-gradient-to-br from-primary to-primary-600 p-2.5 rounded-xl shadow-lg shadow-primary/30">
            <Heart className="h-6 w-6 text-white" />
          </div>
          <span className="font-extrabold text-2xl text-transparent bg-clip-text bg-gradient-to-r from-primary-700 to-primary-500 tracking-tight">FaithFlow</span>
        </div>

        <div className="flex-1 px-4 py-4 space-y-1">
          {navItems.map((item) => {
            const active = isActive(item.path);
            return (
              <Link
                key={item.path}
                to={item.path}
                onClick={() => setIsMobileMenuOpen(false)}
                className={`flex items-center gap-3 px-4 py-3.5 rounded-2xl transition-all duration-300 ${
                  active 
                    ? 'bg-white shadow-lg shadow-black/5 scale-[1.02] text-primary border border-white/60 font-semibold' 
                    : 'text-slate-600 hover:bg-white/40 hover:text-slate-900 font-medium'
                }`}
              >
                <item.icon className={`h-5 w-5 ${active ? 'text-primary' : 'text-slate-500'}`} />
                <span>{item.name}</span>
              </Link>
            );
          })}
        </div>

        <div className="p-4 mt-auto mb-2 mx-4 rounded-2xl bg-white/40 border border-white/50">
          <div className="flex items-center gap-3 mb-4 px-2 pt-2">
            <div className="h-10 w-10 rounded-full bg-gradient-to-br from-primary-100 to-primary-200 flex items-center justify-center text-primary-700 font-bold shadow-inner">
              {user ? (user.firstName?.[0] + user.lastName?.[0]) : 'G'}
            </div>
            <div className="flex-1 min-w-0">
              <p className="font-semibold text-sm text-slate-800 truncate">{user ? `${user.firstName} ${user.lastName}` : 'Guest User'}</p>
              <p className="text-xs text-slate-500 truncate">{user?.email || 'Not logged in'}</p>
            </div>
          </div>
          <button
            onClick={handleAuthAction}
            className="flex w-full items-center gap-3 px-4 py-2 text-muted-foreground hover:text-primary hover:bg-primary/10 rounded-xl transition-colors"
          >
            <LogOut className="h-5 w-5" />
            <span className="font-medium">{user ? 'Logout' : 'Login'}</span>
          </button>
        </div>
      </nav>

      {/* Main Content */}
      <main className="flex-1 overflow-y-auto w-full">
        <div className="p-4 md:p-8 max-w-7xl mx-auto">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
