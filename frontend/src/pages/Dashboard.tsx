import { useAuthStore } from '@/store/auth';
import { Users, Heart, BookOpen, Calendar, ArrowRight } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';

export function Dashboard() {
  const { user } = useAuthStore();

  const { data: feedData } = useQuery({
    queryKey: ['dashboard-feed'],
    queryFn: async () => {
      const res = await api.get('/dashboard/feed');
      return res.data;
    }
  });
  // We'll keep the top stat cards hardcoded for now or pull from stats endpoint later if needed
  const cards = [
    { title: 'My Groups', count: 0, icon: Users, color: 'text-blue-500', bg: 'bg-blue-100', link: '/groups' },
    { title: 'Active Prayers', count: feedData?.recentActivity?.length || 0, icon: Heart, color: 'text-rose-500', bg: 'bg-rose-100', link: '/prayer' },
    { title: 'Sermon Notes', count: 0, icon: BookOpen, color: 'text-indigo-500', bg: 'bg-indigo-100', link: '/sermons' },
    { title: 'Upcoming Shifts', count: feedData?.upcomingEvents?.length || 0, icon: Calendar, color: 'text-amber-500', bg: 'bg-amber-100', link: '/volunteer' },
  ];

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500 pb-12">
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 glass p-8 rounded-3xl relative overflow-hidden">
        <div className="absolute top-0 right-0 w-64 h-64 bg-gradient-to-br from-primary-200 to-indigo-200 rounded-full blur-3xl opacity-40 -mr-20 -mt-20 pointer-events-none"></div>
        <div className="relative z-10">
          <h1 className="text-4xl font-extrabold tracking-tight text-transparent bg-clip-text bg-gradient-to-r from-slate-900 to-slate-600">
            Welcome back, {user?.firstName || 'Guest'}!
          </h1>
          <p className="text-slate-600 mt-2 text-lg">Here's what's happening in your community today.</p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {cards.map((card) => (
          <Link key={card.title} to={card.link} className="block group">
            <div className="glass glass-hover p-6 rounded-3xl h-full flex flex-col justify-between">
              <div className="flex items-center justify-between mb-8">
                <div className={`${card.bg} p-4 rounded-2xl shadow-inner`}>
                  <card.icon className={`h-7 w-7 ${card.color}`} />
                </div>
                <ArrowRight className="h-6 w-6 text-slate-400 opacity-0 group-hover:opacity-100 group-hover:translate-x-2 group-hover:text-primary transition-all duration-300" />
              </div>
              <div>
                <h3 className="text-4xl font-black text-slate-800 tracking-tight">{card.count}</h3>
                <p className="text-sm font-semibold text-slate-500 uppercase tracking-wider mt-1">{card.title}</p>
              </div>
            </div>
          </Link>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Activity placeholder */}
        <div className="glass rounded-3xl overflow-hidden flex flex-col">
          <div className="px-8 py-6 border-b border-white/20 bg-white/40 backdrop-blur-md">
            <h3 className="font-bold text-lg text-slate-800">Recent Activity</h3>
          </div>
          <div className="p-0 flex-1 overflow-y-auto">
            {feedData?.recentActivity?.length > 0 ? (
              <div className="divide-y divide-white/20">
                {feedData.recentActivity.map((activity: any, idx: number) => (
                  <div key={idx} className="p-6 hover:bg-white/40 transition-colors flex items-start gap-4">
                    <div className="bg-rose-100 p-2 rounded-xl text-rose-500 mt-1">
                      <Heart className="h-5 w-5" />
                    </div>
                    <div>
                      <p className="font-semibold text-slate-800">{activity.title}</p>
                      <p className="text-sm text-slate-500">Prayer request by {activity.user}</p>
                      <p className="text-xs font-medium text-slate-400 mt-1">{new Date(activity.time).toLocaleDateString()}</p>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="p-8 h-full flex items-center justify-center">
                <div className="space-y-4 text-center">
                  <div className="inline-flex h-16 w-16 items-center justify-center rounded-full bg-slate-100 mb-2">
                    <Users className="h-8 w-8 text-slate-300" />
                  </div>
                  <p className="text-slate-500 font-medium">No recent activity.</p>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Upcoming Events placeholder */}
        <div className="glass rounded-3xl overflow-hidden flex flex-col">
          <div className="px-8 py-6 border-b border-white/20 bg-white/40 backdrop-blur-md">
            <h3 className="font-bold text-lg text-slate-800">Upcoming Events</h3>
          </div>
          <div className="p-0 flex-1 overflow-y-auto">
            {feedData?.upcomingEvents?.length > 0 ? (
              <div className="divide-y divide-white/20">
                {feedData.upcomingEvents.map((event: any, idx: number) => (
                  <div key={idx} className="p-6 hover:bg-white/40 transition-colors flex items-start gap-4">
                    <div className="bg-amber-100 p-2 rounded-xl text-amber-500 mt-1">
                      <Calendar className="h-5 w-5" />
                    </div>
                    <div>
                      <p className="font-semibold text-slate-800">{event.title}</p>
                      <p className="text-sm text-slate-500">{event.location}</p>
                      <p className="text-xs font-medium text-slate-400 mt-1">{new Date(event.date).toLocaleDateString()}</p>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="p-8 h-full flex items-center justify-center">
                <div className="space-y-4 text-center">
                  <div className="inline-flex h-16 w-16 items-center justify-center rounded-full bg-slate-100 mb-2">
                    <Calendar className="h-8 w-8 text-slate-300" />
                  </div>
                  <p className="text-slate-500 font-medium">No upcoming events.</p>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
