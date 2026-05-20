import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import toast from 'react-hot-toast';
import { Calendar, Clock, MapPin, CheckCircle2, Loader2, XCircle, HeartHandshake } from 'lucide-react';

interface Opportunity {
  id: string;
  title: string;
  description: string;
  category: string;
  date: string;
  location: string;
  totalSpots: number;
  filledSpots: number;
  isSignedUp: boolean;
}

export default function Volunteer() {
  const [activeTab, setActiveTab] = useState<'all' | 'mine'>('all');
  const queryClient = useQueryClient();

  const { data: opportunities, isLoading } = useQuery<Opportunity[]>({
    queryKey: ['opportunities'],
    queryFn: async () => {
      const res = await api.get('/volunteer/opportunities');
      return res.data;
    }
  });

  const signupMutation = useMutation({
    mutationFn: (id: string) => api.post(`/volunteer/opportunities/${id}/signup`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['opportunities'] });
      toast.success('You\'re signed up to serve!');
    },
    onError: (err: any) => toast.error(err.response?.data?.error || 'Could not sign up.')
  });

  const cancelMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/volunteer/opportunities/${id}/cancel`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['opportunities'] });
      toast('Signup cancelled.', { icon: '↩️' });
    },
    onError: (err: any) => toast.error(err.response?.data?.error || 'Could not cancel.')
  });

  const filteredOpps = opportunities?.filter(opp => activeTab === 'all' || opp.isSignedUp) || [];

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500 pb-12">
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 glass p-8 rounded-3xl relative overflow-hidden mb-8">
        <div className="absolute top-0 right-0 w-64 h-64 bg-gradient-to-br from-indigo-200 to-purple-200 rounded-full blur-3xl opacity-40 -mr-20 -mt-20 pointer-events-none"></div>
        <div className="relative z-10">
          <h1 className="text-4xl font-extrabold tracking-tight text-transparent bg-clip-text bg-gradient-to-r from-indigo-600 to-purple-600">
            Serve & Volunteer
          </h1>
          <p className="text-slate-600 mt-2 text-lg">Use your gifts to make a difference in our community.</p>
        </div>
      </div>

      <div className="flex gap-4 mb-6 bg-white/40 p-2 rounded-2xl w-fit">
        {(['all', 'mine'] as const).map(tab => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`px-6 py-2.5 rounded-xl font-bold transition-all capitalize ${
              activeTab === tab 
                ? 'bg-white text-indigo-600 shadow-sm' 
                : 'text-slate-600 hover:bg-white/60 hover:text-slate-900'
            }`}
          >
            {tab === 'all' ? 'All Opportunities' : 'My Signups'}
          </button>
        ))}
      </div>

      {isLoading ? (
        <div className="flex justify-center p-12">
          <Loader2 className="animate-spin h-8 w-8 text-indigo-500" />
        </div>
      ) : (
        <div className="grid gap-6 lg:grid-cols-2">
          {filteredOpps.map(opp => (
            <div key={opp.id} className="glass glass-hover p-8 rounded-3xl flex flex-col md:flex-row gap-6 relative overflow-hidden border border-white/50">
              {opp.isSignedUp && (
                <div className="absolute top-0 right-0 bg-indigo-500 text-white text-xs font-bold px-3 py-1 rounded-bl-xl z-10 flex items-center gap-1 shadow-sm">
                  <CheckCircle2 className="h-3 w-3" /> Signed Up
                </div>
              )}

              <div className="flex-1 mt-2">
                <div className="mb-4">
                  <span className="inline-block bg-indigo-100 text-indigo-800 text-xs px-3 py-1 rounded-full font-bold uppercase tracking-wider mb-3">
                    {opp.category}
                  </span>
                  <h3 className="text-2xl font-black text-slate-800 tracking-tight leading-tight">{opp.title}</h3>
                </div>
                <p className="text-slate-600 leading-relaxed mb-6">{opp.description}</p>
                
                <div className="grid grid-cols-2 gap-y-3 gap-x-4 mb-6 md:mb-0">
                  <div className="flex items-center gap-2 text-sm text-slate-500 font-medium">
                    <Calendar className="h-4 w-4 text-indigo-500" />
                    {new Date(opp.date).toLocaleDateString()}
                  </div>
                  <div className="flex items-center gap-2 text-sm text-slate-500 font-medium">
                    <Clock className="h-4 w-4 text-indigo-500" />
                    {new Date(opp.date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </div>
                  <div className="flex items-center gap-2 text-sm text-slate-500 font-medium col-span-2">
                    <MapPin className="h-4 w-4 text-indigo-500" />
                    {opp.location}
                  </div>
                </div>
              </div>
              
              <div className="md:w-48 flex flex-col justify-between border-t md:border-t-0 md:border-l border-white/40 pt-6 md:pt-0 md:pl-6">
                <div>
                  <div className="text-sm font-bold text-slate-500 uppercase tracking-wider mb-2">Capacity</div>
                  <div className="w-full bg-slate-200 rounded-full h-2 mb-2 overflow-hidden">
                    <div 
                      className="bg-indigo-500 h-full rounded-full transition-all duration-500" 
                      style={{ width: `${Math.min(100, (opp.filledSpots / opp.totalSpots) * 100)}%` }}
                    ></div>
                  </div>
                  <div className="text-sm font-semibold text-slate-700">
                    {opp.filledSpots} / {opp.totalSpots} Filled
                  </div>
                </div>

                {opp.isSignedUp ? (
                  <button 
                    onClick={() => cancelMutation.mutate(opp.id)}
                    disabled={cancelMutation.isPending && cancelMutation.variables === opp.id}
                    className="mt-6 w-full group flex items-center justify-center gap-2 bg-rose-50 text-rose-600 px-4 py-3 rounded-xl hover:bg-rose-100 transition-all font-bold disabled:opacity-50"
                  >
                    {cancelMutation.isPending && cancelMutation.variables === opp.id ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      <XCircle className="h-4 w-4" />
                    )}
                    Cancel
                  </button>
                ) : (
                  <button 
                    onClick={() => signupMutation.mutate(opp.id)}
                    disabled={(signupMutation.isPending && signupMutation.variables === opp.id) || opp.filledSpots >= opp.totalSpots}
                    className="mt-6 w-full group flex items-center justify-center gap-2 bg-gradient-to-r from-indigo-500 to-purple-500 text-white px-4 py-3 rounded-xl hover:from-indigo-600 hover:to-purple-600 transition-all font-bold shadow-lg shadow-indigo-500/30 hover:shadow-indigo-500/50 disabled:opacity-50 disabled:hover:translate-y-0"
                  >
                    {signupMutation.isPending && signupMutation.variables === opp.id ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      opp.filledSpots >= opp.totalSpots ? 'Full' : (
                        <>Sign Up <CheckCircle2 className="h-4 w-4" /></>
                      )
                    )}
                  </button>
                )}
              </div>
            </div>
          ))}
          {filteredOpps.length === 0 && (
            <div className="col-span-full text-center py-12 text-slate-500 font-medium bg-white/40 rounded-3xl border border-dashed border-white">
              <HeartHandshake className="h-12 w-12 mx-auto mb-4 text-slate-400 opacity-50" />
              <p>No volunteer opportunities found.</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
