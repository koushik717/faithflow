import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as signalR from '@microsoft/signalr';
import { Heart, Send, Sparkles, X } from 'lucide-react';
import toast from 'react-hot-toast';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

interface PrayerRequest {
  id: string;
  userId: string | null;
  userName: string | null;
  title: string;
  description: string;
  prayCount: number;
  isPublic: boolean;
  isAnonymous: boolean;
  isAnswered: boolean;
  createdAt: string;
}

interface AiPrayerModalProps {
  prayer: string;
  onClose: () => void;
}

function AiPrayerModal({ prayer, onClose }: AiPrayerModalProps) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm" onClick={onClose}>
      <div
        className="glass rounded-3xl p-8 max-w-lg w-full shadow-2xl animate-in fade-in slide-in-from-bottom-4 duration-300"
        onClick={e => e.stopPropagation()}
      >
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-3">
            <div className="bg-indigo-100 p-2.5 rounded-xl">
              <Sparkles className="h-5 w-5 text-indigo-600" />
            </div>
            <h3 className="text-xl font-bold text-slate-800">AI Guided Prayer</h3>
          </div>
          <button onClick={onClose} className="p-2 text-slate-400 hover:text-slate-700 hover:bg-white/60 rounded-xl transition-colors">
            <X className="h-5 w-5" />
          </button>
        </div>
        <p className="text-slate-700 leading-relaxed italic text-lg whitespace-pre-wrap">{prayer}</p>
        <button
          onClick={onClose}
          className="mt-6 w-full bg-gradient-to-r from-indigo-500 to-purple-500 text-white py-3 rounded-2xl font-bold hover:from-indigo-600 hover:to-purple-600 transition-all shadow-lg shadow-indigo-500/30"
        >
          Amen
        </button>
      </div>
    </div>
  );
}

export function PrayerWall() {
  const [newPrayer, setNewPrayer] = useState({ title: '', description: '', isAnonymous: false, isPublic: true });
  const [activeTab, setActiveTab] = useState<'all' | 'mine' | 'answered'>('all');
  const [aiPrayerText, setAiPrayerText] = useState<string | null>(null);
  const queryClient = useQueryClient();
  const { user } = useAuthStore();

  const { data: prayers, isLoading } = useQuery<PrayerRequest[]>({
    queryKey: ['prayers', activeTab],
    queryFn: async () => {
      const res = await api.get(`/prayer-requests?filter=${activeTab}`);
      return res.data;
    }
  });

  // SignalR connection for real-time updates
  useEffect(() => {
    const token = localStorage.getItem('token');
    const baseUrl = import.meta.env.VITE_SIGNALR_URL
      || (import.meta.env.VITE_API_URL ? import.meta.env.VITE_API_URL.replace(/\/api\/?$/, '') : 'http://localhost:5227');

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/prayer?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    connection.start()
      .then(() => connection.invoke('JoinPrayerWall'))
      .catch(console.error);

    connection.on('OnNewPrayerRequest', (request: PrayerRequest) => {
      queryClient.setQueryData<PrayerRequest[]>(['prayers', 'all'], (old = []) => [request, ...old]);
    });

    connection.on('OnPrayCountUpdated', (id: string, count: number) => {
      (['all', 'mine', 'answered'] as const).forEach(tab => {
        queryClient.setQueryData<PrayerRequest[]>(['prayers', tab], (old = []) =>
          old.map(p => p.id === id ? { ...p, prayCount: count } : p)
        );
      });
    });

    connection.on('OnRequestAnswered', () => {
      queryClient.invalidateQueries({ queryKey: ['prayers'] });
    });

    return () => {
      connection.invoke('LeavePrayerWall').catch(() => {}).finally(() => connection.stop());
    };
  }, [queryClient]);

  const createMutation = useMutation({
    mutationFn: (data: typeof newPrayer) => api.post('/prayer-requests', data),
    onSuccess: () => {
      setNewPrayer({ title: '', description: '', isAnonymous: false, isPublic: true });
      queryClient.invalidateQueries({ queryKey: ['prayers'] });
      toast.success('Prayer request shared!');
    },
    onError: () => toast.error('Failed to share request. Please try again.')
  });

  const prayMutation = useMutation({
    mutationFn: (id: string) => api.post(`/prayer-requests/${id}/pray`),
    onError: () => toast.error('Failed to record prayer.')
  });

  const aiMutation = useMutation({
    mutationFn: (id: string) => api.post<{ prayer: string }>(`/prayer-requests/${id}/ai-prayer`),
    onSuccess: (res) => setAiPrayerText(res.data.prayer),
    onError: () => toast.error('AI prayer generation failed. Please try again.')
  });

  const markAnsweredMutation = useMutation({
    mutationFn: (id: string) => api.put(`/prayer-requests/${id}/answered`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['prayers'] });
      toast.success('Prayer marked as answered! Praise God!');
    },
    onError: () => toast.error('Failed to update prayer request.')
  });

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
      {aiPrayerText && <AiPrayerModal prayer={aiPrayerText} onClose={() => setAiPrayerText(null)} />}

      <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 glass p-8 rounded-3xl relative overflow-hidden mb-8">
        <div className="absolute top-0 right-0 w-64 h-64 bg-gradient-to-br from-rose-200 to-orange-200 rounded-full blur-3xl opacity-40 -mr-20 -mt-20 pointer-events-none"></div>
        <div className="relative z-10">
          <h1 className="text-4xl font-extrabold tracking-tight text-transparent bg-clip-text bg-gradient-to-r from-rose-600 to-orange-500">
            Prayer Wall
          </h1>
          <p className="text-slate-600 mt-2 text-lg">Share your burdens and pray for others in real-time.</p>
        </div>
      </div>

      {/* Create new prayer */}
      <div className="glass p-8 rounded-3xl mb-8">
        <h3 className="font-bold text-xl text-slate-800 mb-6 flex items-center gap-2">
          <Heart className="h-6 w-6 text-rose-500" /> Share a Request
        </h3>
        <form onSubmit={(e) => { e.preventDefault(); createMutation.mutate(newPrayer); }} className="space-y-5">
          <input
            type="text"
            placeholder="What's on your heart?"
            value={newPrayer.title}
            onChange={e => setNewPrayer(prev => ({ ...prev, title: e.target.value }))}
            className="w-full px-5 py-3.5 rounded-2xl border border-white/40 bg-white/50 focus:bg-white focus:ring-2 focus:ring-rose-500/50 focus:border-rose-500 transition-all font-medium text-slate-800 shadow-sm"
            required
          />
          <textarea
            placeholder="Add more details..."
            value={newPrayer.description}
            onChange={e => setNewPrayer(prev => ({ ...prev, description: e.target.value }))}
            className="w-full px-5 py-4 rounded-2xl border border-white/40 bg-white/50 focus:bg-white focus:ring-2 focus:ring-rose-500/50 focus:border-rose-500 transition-all text-slate-800 min-h-[120px] shadow-sm"
            required
          />
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <label className="flex items-center gap-3 text-sm font-medium text-slate-700 cursor-pointer p-2 hover:bg-white/40 rounded-xl transition-colors">
              <input
                type="checkbox"
                checked={newPrayer.isAnonymous}
                onChange={e => setNewPrayer(prev => ({ ...prev, isAnonymous: e.target.checked }))}
                className="rounded-md border-slate-300 text-rose-500 focus:ring-rose-500 h-5 w-5"
              />
              Anonymous
            </label>
            <label className="flex items-center gap-3 text-sm font-medium text-slate-700 cursor-pointer p-2 hover:bg-white/40 rounded-xl transition-colors">
              <input
                type="checkbox"
                checked={newPrayer.isPublic}
                onChange={e => setNewPrayer(prev => ({ ...prev, isPublic: e.target.checked }))}
                className="rounded-md border-slate-300 text-rose-500 focus:ring-rose-500 h-5 w-5"
              />
              Make this request public
            </label>
            <button
              type="submit"
              disabled={createMutation.isPending}
              className="flex items-center gap-2 bg-gradient-to-r from-rose-500 to-orange-500 text-white px-8 py-3 rounded-2xl hover:from-rose-600 hover:to-orange-600 transition-all disabled:opacity-50 font-bold shadow-lg shadow-rose-500/30 hover:shadow-rose-500/50 hover:-translate-y-0.5 w-full sm:w-auto justify-center"
            >
              <Send className="h-5 w-5" />
              {createMutation.isPending ? 'Sharing...' : 'Share Request'}
            </button>
          </div>
        </form>
      </div>

      {/* Tabs */}
      <div className="flex gap-4 mb-6 bg-white/40 p-2 rounded-2xl w-fit">
        {(['all', 'mine', 'answered'] as const).map(tab => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`px-6 py-2.5 rounded-xl font-bold transition-all capitalize ${
              activeTab === tab
                ? 'bg-white text-rose-500 shadow-sm'
                : 'text-slate-600 hover:bg-white/60 hover:text-slate-900'
            }`}
          >
            {tab}
          </button>
        ))}
      </div>

      {/* Prayer List */}
      {isLoading ? (
        <div className="flex justify-center p-12">
          <div className="animate-spin h-8 w-8 border-4 border-rose-500 border-t-transparent rounded-full"></div>
        </div>
      ) : (
        <div className="grid gap-6 md:grid-cols-2">
          {prayers?.map(prayer => (
            <div key={prayer.id} className="glass glass-hover p-8 rounded-3xl flex flex-col">
              <div className="flex justify-between items-start mb-6">
                <div>
                  <h4 className="font-extrabold text-xl text-slate-800 leading-tight mb-1">{prayer.title}</h4>
                  <p className="text-sm font-medium text-slate-500">
                    {prayer.isAnonymous ? 'Anonymous' : (prayer.userName || 'Community Member')}
                    {' '}•{' '}
                    {new Date(prayer.createdAt).toLocaleDateString()}
                  </p>
                </div>
                {prayer.isAnswered && (
                  <span className="bg-emerald-100 text-emerald-700 text-xs px-3 py-1.5 rounded-full font-bold border border-emerald-200 shadow-sm shrink-0 ml-3">
                    Answered
                  </span>
                )}
              </div>
              <p className="text-slate-700 leading-relaxed mb-8 whitespace-pre-wrap flex-1">{prayer.description}</p>

              <div className="flex flex-wrap items-center gap-3 pt-6 border-t border-white/40 mt-auto">
                <button
                  onClick={() => prayMutation.mutate(prayer.id)}
                  className="flex items-center gap-2 bg-rose-50 text-rose-600 hover:bg-rose-500 hover:text-white px-4 py-2.5 rounded-xl transition-all duration-300 text-sm font-bold shadow-sm"
                >
                  <Heart className="h-4 w-4" />
                  Pray ({prayer.prayCount})
                </button>
                <button
                  onClick={() => aiMutation.mutate(prayer.id)}
                  disabled={aiMutation.isPending && aiMutation.variables === prayer.id}
                  className="flex items-center gap-2 bg-indigo-50 text-indigo-600 hover:bg-indigo-500 hover:text-white px-4 py-2.5 rounded-xl transition-all duration-300 text-sm font-bold shadow-sm disabled:opacity-50"
                >
                  {aiMutation.isPending && aiMutation.variables === prayer.id ? (
                    <div className="animate-spin h-4 w-4 border-2 border-current border-t-transparent rounded-full"></div>
                  ) : (
                    <Sparkles className="h-4 w-4" />
                  )}
                  {aiMutation.isPending && aiMutation.variables === prayer.id ? 'Generating...' : 'AI Guided'}
                </button>
                {user?.id === prayer.userId && !prayer.isAnswered && (
                  <button
                    onClick={() => markAnsweredMutation.mutate(prayer.id)}
                    className="flex items-center gap-2 bg-emerald-50 text-emerald-600 hover:bg-emerald-500 hover:text-white px-4 py-2.5 rounded-xl transition-all duration-300 text-sm font-bold shadow-sm ml-auto"
                  >
                    Mark Answered
                  </button>
                )}
              </div>
            </div>
          ))}

          {prayers?.length === 0 && (
            <div className="col-span-full text-center py-16 text-slate-400 bg-white/40 rounded-3xl border border-dashed border-white/60">
              <Heart className="h-12 w-12 mx-auto mb-4 opacity-30" />
              <p className="font-medium">No prayer requests yet. Be the first to share one.</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
