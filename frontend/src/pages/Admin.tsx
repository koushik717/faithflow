import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import { Users, Activity, Heart, ShieldCheck, TrendingUp, UserPlus, BarChart3, Search } from 'lucide-react';
import toast from 'react-hot-toast';

interface AdminStats {
  totalMembers: number;
  newMembersThisMonth: number;
  activeGroups: number;
  prayerRequestsThisWeek: number;
  volunteerCoveragePercent: number;
}

interface Member {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  isActive: boolean;
  joinedAt: string;
}

interface GroupAnalytics {
  id: string;
  name: string;
  category: string;
  memberCount: number;
  maxMembers: number;
  averageAttendance: number;
  leaderName: string;
}

type Tab = 'overview' | 'members' | 'groups';

export function Admin() {
  const [activeTab, setActiveTab] = useState<Tab>('overview');
  const [memberSearch, setMemberSearch] = useState('');
  const queryClient = useQueryClient();

  const { data: stats, isLoading: statsLoading } = useQuery<AdminStats>({
    queryKey: ['adminStats'],
    queryFn: async () => {
      const res = await api.get('/admin/stats');
      return res.data;
    },
  });

  const { data: members, isLoading: membersLoading } = useQuery<Member[]>({
    queryKey: ['adminMembers', memberSearch],
    queryFn: async () => {
      const res = await api.get(`/admin/members?q=${memberSearch}&pageSize=50`);
      return res.data;
    },
    enabled: activeTab === 'members',
  });

  const { data: groupAnalytics, isLoading: groupsLoading } = useQuery<GroupAnalytics[]>({
    queryKey: ['adminGroupAnalytics'],
    queryFn: async () => {
      const res = await api.get('/admin/groups/analytics');
      return res.data;
    },
    enabled: activeTab === 'groups',
  });

  const changeRoleMutation = useMutation({
    mutationFn: ({ id, role }: { id: string; role: string }) =>
      api.put(`/admin/users/${id}/role`, { role }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adminMembers'] });
      toast.success('Role updated successfully.');
    },
    onError: () => toast.error('Failed to update role.'),
  });

  const tabs: { id: Tab; label: string; icon: React.ElementType }[] = [
    { id: 'overview', label: 'Overview', icon: BarChart3 },
    { id: 'members', label: 'Members', icon: Users },
    { id: 'groups', label: 'Groups', icon: Activity },
  ];

  const statCards = stats ? [
    { label: 'Total Members', value: stats.totalMembers, icon: Users, color: 'text-blue-600', bg: 'bg-blue-50' },
    { label: 'New This Month', value: stats.newMembersThisMonth, icon: UserPlus, color: 'text-emerald-600', bg: 'bg-emerald-50' },
    { label: 'Active Groups', value: stats.activeGroups, icon: Activity, color: 'text-indigo-600', bg: 'bg-indigo-50' },
    { label: 'Prayers This Week', value: stats.prayerRequestsThisWeek, icon: Heart, color: 'text-rose-600', bg: 'bg-rose-50' },
    { label: 'Volunteer Coverage', value: `${stats.volunteerCoveragePercent}%`, icon: ShieldCheck, color: 'text-amber-600', bg: 'bg-amber-50' },
    { label: 'Engagement Score', value: stats.totalMembers > 0 ? Math.round((stats.prayerRequestsThisWeek / stats.totalMembers) * 100) + '%' : '0%', icon: TrendingUp, color: 'text-purple-600', bg: 'bg-purple-50' },
  ] : [];

  return (
    <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500 pb-12">
      <div className="glass p-8 rounded-3xl relative overflow-hidden">
        <div className="absolute top-0 right-0 w-64 h-64 bg-gradient-to-br from-slate-200 to-blue-200 rounded-full blur-3xl opacity-30 -mr-20 -mt-20 pointer-events-none"></div>
        <div className="relative z-10">
          <h1 className="text-4xl font-extrabold tracking-tight text-transparent bg-clip-text bg-gradient-to-r from-slate-800 to-slate-600">
            Admin Dashboard
          </h1>
          <p className="text-slate-500 mt-2">Manage your church community.</p>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 bg-white/40 p-2 rounded-2xl w-fit">
        {tabs.map(tab => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`flex items-center gap-2 px-5 py-2.5 rounded-xl font-bold transition-all text-sm ${
              activeTab === tab.id
                ? 'bg-white text-slate-800 shadow-sm'
                : 'text-slate-500 hover:bg-white/60 hover:text-slate-800'
            }`}
          >
            <tab.icon className="h-4 w-4" />
            {tab.label}
          </button>
        ))}
      </div>

      {/* Overview Tab */}
      {activeTab === 'overview' && (
        <>
          {statsLoading ? (
            <div className="flex justify-center p-12">
              <div className="animate-spin h-8 w-8 border-4 border-primary border-t-transparent rounded-full"></div>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {statCards.map(card => (
                <div key={card.label} className="glass glass-hover p-6 rounded-3xl">
                  <div className={`${card.bg} p-3 rounded-2xl w-fit mb-4`}>
                    <card.icon className={`h-6 w-6 ${card.color}`} />
                  </div>
                  <p className="text-sm font-semibold text-slate-500 uppercase tracking-wider mb-1">{card.label}</p>
                  <p className="text-4xl font-black text-slate-800">{card.value}</p>
                </div>
              ))}
            </div>
          )}
        </>
      )}

      {/* Members Tab */}
      {activeTab === 'members' && (
        <div className="glass rounded-3xl overflow-hidden">
          <div className="p-6 border-b border-white/20 bg-white/40">
            <div className="relative max-w-sm">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
              <input
                type="text"
                placeholder="Search members..."
                value={memberSearch}
                onChange={e => setMemberSearch(e.target.value)}
                className="w-full pl-11 pr-4 py-2.5 rounded-xl border border-white/40 bg-white/50 focus:bg-white focus:ring-2 focus:ring-primary/30 outline-none text-sm font-medium text-slate-700 transition-all"
              />
            </div>
          </div>
          {membersLoading ? (
            <div className="flex justify-center p-12">
              <div className="animate-spin h-8 w-8 border-4 border-primary border-t-transparent rounded-full"></div>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-white/30 text-left">
                    <th className="px-6 py-4 font-semibold text-slate-600">Name</th>
                    <th className="px-6 py-4 font-semibold text-slate-600">Email</th>
                    <th className="px-6 py-4 font-semibold text-slate-600">Role</th>
                    <th className="px-6 py-4 font-semibold text-slate-600">Status</th>
                    <th className="px-6 py-4 font-semibold text-slate-600">Joined</th>
                    <th className="px-6 py-4 font-semibold text-slate-600">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-white/20">
                  {members?.map(member => (
                    <tr key={member.id} className="hover:bg-white/40 transition-colors">
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <div className="h-8 w-8 rounded-full bg-gradient-to-br from-primary-100 to-primary-200 flex items-center justify-center text-primary-700 font-bold text-xs shrink-0">
                            {member.firstName[0]}{member.lastName[0]}
                          </div>
                          <span className="font-semibold text-slate-800">{member.firstName} {member.lastName}</span>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-slate-500">{member.email}</td>
                      <td className="px-6 py-4">
                        <select
                          value={member.role}
                          onChange={e => changeRoleMutation.mutate({ id: member.id, role: e.target.value })}
                          className="text-xs font-bold px-2 py-1 rounded-lg bg-primary/10 text-primary border-none outline-none cursor-pointer hover:bg-primary/20 transition-colors"
                        >
                          <option value="Member">Member</option>
                          <option value="GroupLeader">Group Leader</option>
                          <option value="Admin">Admin</option>
                        </select>
                      </td>
                      <td className="px-6 py-4">
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold ${
                          member.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-100 text-slate-500'
                        }`}>
                          {member.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-slate-500">{new Date(member.joinedAt).toLocaleDateString()}</td>
                      <td className="px-6 py-4">
                        <button className="text-xs text-slate-400 hover:text-slate-600 font-medium transition-colors">
                          View
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {members?.length === 0 && (
                <div className="text-center py-12 text-slate-400">
                  <Users className="h-10 w-10 mx-auto mb-3 opacity-30" />
                  <p className="font-medium">No members found.</p>
                </div>
              )}
            </div>
          )}
        </div>
      )}

      {/* Groups Tab */}
      {activeTab === 'groups' && (
        <div className="glass rounded-3xl overflow-hidden">
          <div className="px-6 py-5 border-b border-white/20 bg-white/40">
            <h3 className="font-bold text-slate-800">Group Analytics</h3>
          </div>
          {groupsLoading ? (
            <div className="flex justify-center p-12">
              <div className="animate-spin h-8 w-8 border-4 border-primary border-t-transparent rounded-full"></div>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-white/30 text-left">
                    <th className="px-6 py-4 font-semibold text-slate-600">Group</th>
                    <th className="px-6 py-4 font-semibold text-slate-600">Category</th>
                    <th className="px-6 py-4 font-semibold text-slate-600">Leader</th>
                    <th className="px-6 py-4 font-semibold text-slate-600">Members</th>
                    <th className="px-6 py-4 font-semibold text-slate-600">Avg. Attendance</th>
                    <th className="px-6 py-4 font-semibold text-slate-600">Capacity</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-white/20">
                  {groupAnalytics?.map(group => {
                    const pct = Math.round((group.memberCount / group.maxMembers) * 100);
                    return (
                      <tr key={group.id} className="hover:bg-white/40 transition-colors">
                        <td className="px-6 py-4 font-semibold text-slate-800">{group.name}</td>
                        <td className="px-6 py-4">
                          <span className="bg-indigo-100 text-indigo-700 text-xs px-2 py-0.5 rounded-full font-bold">
                            {group.category}
                          </span>
                        </td>
                        <td className="px-6 py-4 text-slate-500">{group.leaderName}</td>
                        <td className="px-6 py-4 text-slate-700 font-semibold">{group.memberCount} / {group.maxMembers}</td>
                        <td className="px-6 py-4 text-slate-500">{group.averageAttendance.toFixed(1)}</td>
                        <td className="px-6 py-4">
                          <div className="flex items-center gap-2">
                            <div className="flex-1 bg-slate-200 rounded-full h-1.5 overflow-hidden max-w-[80px]">
                              <div
                                className={`h-full rounded-full transition-all ${pct >= 80 ? 'bg-emerald-500' : pct >= 50 ? 'bg-amber-400' : 'bg-rose-400'}`}
                                style={{ width: `${pct}%` }}
                              />
                            </div>
                            <span className="text-xs font-bold text-slate-500">{pct}%</span>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
              {groupAnalytics?.length === 0 && (
                <div className="text-center py-12 text-slate-400">
                  <Activity className="h-10 w-10 mx-auto mb-3 opacity-30" />
                  <p className="font-medium">No groups found.</p>
                </div>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default Admin;
