import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import { Users, MapPin, Calendar, ArrowRight, Loader2, CheckCircle2, LogOut } from 'lucide-react';

interface Group {
  id: string;
  name: string;
  description: string;
  category: string;
  schedule: string;
  meetingLocation: string;
  memberCount: number;
  maxMembers: number;
  isMember: boolean;
}

export default function Groups() {
  const [activeTab, setActiveTab] = useState<'all' | 'mine'>('all');
  const queryClient = useQueryClient();

  const { data: groups, isLoading } = useQuery<Group[]>({
    queryKey: ['groups'],
    queryFn: async () => {
      const res = await api.get('/groups');
      return res.data;
    }
  });

  const joinMutation = useMutation({
    mutationFn: (id: string) => api.post(`/groups/${id}/join`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['groups'] })
  });

  const leaveMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/groups/${id}/leave`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['groups'] })
  });

  const filteredGroups = groups?.filter(g => activeTab === 'all' || g.isMember) || [];

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500 pb-12">
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 glass p-8 rounded-3xl relative overflow-hidden mb-8">
        <div className="absolute top-0 right-0 w-64 h-64 bg-gradient-to-br from-emerald-200 to-teal-200 rounded-full blur-3xl opacity-40 -mr-20 -mt-20 pointer-events-none"></div>
        <div className="relative z-10">
          <h1 className="text-4xl font-extrabold tracking-tight text-transparent bg-clip-text bg-gradient-to-r from-emerald-600 to-teal-500">
            Community Groups
          </h1>
          <p className="text-slate-600 mt-2 text-lg">Find your people. Grow together in faith and life.</p>
        </div>
      </div>

      <div className="flex gap-4 mb-6 bg-white/40 p-2 rounded-2xl w-fit">
        {(['all', 'mine'] as const).map(tab => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`px-6 py-2.5 rounded-xl font-bold transition-all capitalize ${
              activeTab === tab 
                ? 'bg-white text-emerald-600 shadow-sm' 
                : 'text-slate-600 hover:bg-white/60 hover:text-slate-900'
            }`}
          >
            {tab === 'all' ? 'All Groups' : 'My Groups'}
          </button>
        ))}
      </div>

      {isLoading ? (
        <div className="flex justify-center p-12">
          <Loader2 className="animate-spin h-8 w-8 text-emerald-500" />
        </div>
      ) : (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {filteredGroups.map(group => (
            <div key={group.id} className="glass glass-hover p-8 rounded-3xl flex flex-col h-full border border-white/50 relative overflow-hidden">
              {group.isMember && (
                <div className="absolute top-0 right-0 bg-emerald-500 text-white text-xs font-bold px-3 py-1 rounded-bl-xl z-10 flex items-center gap-1 shadow-sm">
                  <CheckCircle2 className="h-3 w-3" /> Member
                </div>
              )}
              
              <div className="mb-4 mt-2">
                <span className="inline-block bg-emerald-100 text-emerald-800 text-xs px-3 py-1 rounded-full font-bold uppercase tracking-wider mb-3">
                  {group.category}
                </span>
                <h3 className="text-2xl font-black text-slate-800 tracking-tight leading-tight">{group.name}</h3>
              </div>
              <p className="text-slate-600 leading-relaxed mb-6 flex-1">{group.description}</p>
              
              <div className="space-y-3 mb-8">
                <div className="flex items-center gap-3 text-sm text-slate-500 font-medium">
                  <Calendar className="h-4 w-4 text-emerald-500" />
                  {group.schedule}
                </div>
                <div className="flex items-center gap-3 text-sm text-slate-500 font-medium">
                  <MapPin className="h-4 w-4 text-emerald-500" />
                  {group.meetingLocation}
                </div>
                <div className="flex items-center gap-3 text-sm text-slate-500 font-medium">
                  <Users className="h-4 w-4 text-emerald-500" />
                  {group.memberCount} / {group.maxMembers} Members
                </div>
              </div>

              {group.isMember ? (
                <button 
                  onClick={() => leaveMutation.mutate(group.id)}
                  disabled={leaveMutation.isPending && leaveMutation.variables === group.id}
                  className="mt-auto w-full group flex items-center justify-center gap-2 bg-rose-50 text-rose-600 px-6 py-3.5 rounded-2xl hover:bg-rose-100 transition-all font-bold disabled:opacity-50"
                >
                  {leaveMutation.isPending && leaveMutation.variables === group.id ? (
                    <Loader2 className="h-5 w-5 animate-spin" />
                  ) : (
                    <LogOut className="h-5 w-5" />
                  )}
                  Leave Group
                </button>
              ) : (
                <button 
                  onClick={() => joinMutation.mutate(group.id)}
                  disabled={(joinMutation.isPending && joinMutation.variables === group.id) || group.memberCount >= group.maxMembers}
                  className="mt-auto w-full group flex items-center justify-center gap-2 bg-gradient-to-r from-emerald-500 to-teal-500 text-white px-6 py-3.5 rounded-2xl hover:from-emerald-600 hover:to-teal-600 transition-all font-bold shadow-lg shadow-emerald-500/30 hover:shadow-emerald-500/50 hover:-translate-y-0.5 disabled:opacity-50 disabled:hover:translate-y-0"
                >
                  {joinMutation.isPending && joinMutation.variables === group.id ? (
                    <Loader2 className="h-5 w-5 animate-spin" />
                  ) : (
                    group.memberCount >= group.maxMembers ? 'Group Full' : (
                      <>Join Group <ArrowRight className="h-5 w-5 group-hover:translate-x-1 transition-transform" /></>
                    )
                  )}
                </button>
              )}
            </div>
          ))}
          {filteredGroups.length === 0 && (
            <div className="col-span-full text-center py-12 text-slate-500 font-medium bg-white/40 rounded-3xl border border-dashed border-white">
              <Users className="h-12 w-12 mx-auto mb-4 text-slate-400 opacity-50" />
              <p>No groups found in this category.</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
