import React from 'react';
import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';
import { Users, Activity, Heart, ShieldCheck } from 'lucide-react';

interface AdminStats {
  totalMembers: number;
  activeGroups: number;
  prayerRequestsThisWeek: number;
  volunteerCoverage: number;
}

export function Admin() {
  const { data: stats, isLoading, isError } = useQuery<AdminStats>({
    queryKey: ['adminStats'],
    queryFn: async () => {
      const response = await api.get('/admin/stats');
      return response.data;
    },
  });

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center p-8">
        <div className="animate-spin h-8 w-8 border-4 border-primary border-t-transparent rounded-full"></div>
      </div>
    );
  }

  if (isError || !stats) {
    return (
      <div className="p-8 text-center text-red-500">
        Failed to load admin statistics.
      </div>
    );
  }

  return (
    <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-primary">Admin Dashboard</h1>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="glass p-6 rounded-2xl border border-white/20 shadow-lg relative overflow-hidden group hover:scale-105 transition-all duration-300">
          <div className="absolute top-0 right-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
            <Users size={64} className="text-primary" />
          </div>
          <p className="text-sm text-surface-600 font-medium mb-1 relative z-10">Total Members</p>
          <p className="text-4xl font-bold text-primary relative z-10">{stats.totalMembers}</p>
        </div>

        <div className="glass p-6 rounded-2xl border border-white/20 shadow-lg relative overflow-hidden group hover:scale-105 transition-all duration-300">
          <div className="absolute top-0 right-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
            <Activity size={64} className="text-primary" />
          </div>
          <p className="text-sm text-surface-600 font-medium mb-1 relative z-10">Active Groups</p>
          <p className="text-4xl font-bold text-primary relative z-10">{stats.activeGroups}</p>
        </div>

        <div className="glass p-6 rounded-2xl border border-white/20 shadow-lg relative overflow-hidden group hover:scale-105 transition-all duration-300">
          <div className="absolute top-0 right-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
            <Heart size={64} className="text-primary" />
          </div>
          <p className="text-sm text-surface-600 font-medium mb-1 relative z-10">Prayer Requests (This Week)</p>
          <p className="text-4xl font-bold text-primary relative z-10">{stats.prayerRequestsThisWeek}</p>
        </div>

        <div className="glass p-6 rounded-2xl border border-white/20 shadow-lg relative overflow-hidden group hover:scale-105 transition-all duration-300">
          <div className="absolute top-0 right-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
            <ShieldCheck size={64} className="text-primary" />
          </div>
          <p className="text-sm text-surface-600 font-medium mb-1 relative z-10">Volunteer Coverage</p>
          <p className="text-4xl font-bold text-primary relative z-10">{stats.volunteerCoverage}%</p>
        </div>
      </div>
    </div>
  );
}

export default Admin;
