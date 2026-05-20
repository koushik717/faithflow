import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import toast from 'react-hot-toast';
import { BookOpen, Search, Plus, Save, Trash2, Edit3, Loader2 } from 'lucide-react';

interface SermonNote {
  id: string;
  title: string;
  notes: string;
  date: string;
  preacherName: string;
  scriptureReference: string;
  scriptureText?: string;
}

export default function SermonNotes() {
  const queryClient = useQueryClient();
  const [selectedNote, setSelectedNote] = useState<SermonNote | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState<Partial<SermonNote>>({});
  const [searchQuery, setSearchQuery] = useState('');
  
  const { data: notes, isLoading } = useQuery<SermonNote[]>({
    queryKey: ['sermon-notes'],
    queryFn: async () => {
      const res = await api.get('/sermon-notes');
      return res.data;
    }
  });

  const saveMutation = useMutation({
    mutationFn: async (note: Partial<SermonNote>) => {
      if (note.id) {
        return api.put(`/sermon-notes/${note.id}`, note);
      } else {
        return api.post('/sermon-notes', note);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sermon-notes'] });
      setIsEditing(false);
      toast.success('Note saved!');
    },
    onError: () => toast.error('Failed to save note.')
  });

  const scriptureLookupMutation = useMutation({
    mutationFn: async (id: string) => api.post(`/sermon-notes/${id}/scripture-lookup`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sermon-notes'] });
      toast.success('Scripture loaded!');
    },
    onError: () => toast.error('Scripture lookup failed.')
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) => api.delete(`/sermon-notes/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sermon-notes'] });
      setSelectedNote(null);
      toast('Note deleted.', { icon: '🗑️' });
    },
    onError: () => toast.error('Failed to delete note.')
  });

  const handleNewNote = () => {
    setSelectedNote(null);
    setEditForm({
      title: '',
      preacherName: '',
      date: new Date().toISOString().split('T')[0],
      scriptureReference: '',
      notes: ''
    });
    setIsEditing(true);
  };

  const handleEdit = (note: SermonNote) => {
    setSelectedNote(note);
    setEditForm(note);
    setIsEditing(true);
  };

  const handleSave = () => {
    saveMutation.mutate(editForm);
  };

  return (
    <div className="h-[calc(100vh-8rem)] flex flex-col animate-in fade-in slide-in-from-bottom-4 duration-500">
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 glass p-6 rounded-3xl relative overflow-hidden mb-6 flex-shrink-0">
        <div className="absolute top-0 right-0 w-64 h-64 bg-gradient-to-br from-blue-200 to-cyan-200 rounded-full blur-3xl opacity-40 -mr-20 -mt-20 pointer-events-none"></div>
        <div className="relative z-10 flex w-full items-center justify-between">
          <div>
            <h1 className="text-4xl font-extrabold tracking-tight text-transparent bg-clip-text bg-gradient-to-r from-blue-600 to-cyan-500">
              Sermon Notes
            </h1>
            <p className="text-slate-600 mt-1 text-lg">Capture insights and grow in the Word.</p>
          </div>
          <button 
            onClick={handleNewNote}
            className="flex items-center gap-2 bg-gradient-to-r from-blue-500 to-cyan-500 text-white px-6 py-3 rounded-xl hover:from-blue-600 hover:to-cyan-600 transition-all font-bold shadow-lg shadow-blue-500/30 hover:shadow-blue-500/50 hover:-translate-y-0.5"
          >
            <Plus className="h-5 w-5" />
            New Note
          </button>
        </div>
      </div>

      <div className="flex flex-col md:flex-row gap-6 flex-1 min-h-0">
        {/* Left pane: Note list */}
        <div className="w-full md:w-1/3 flex flex-col gap-4">
          <div className="relative">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
            <input 
              type="text" 
              placeholder="Search notes..." 
              value={searchQuery}
              onChange={e => setSearchQuery(e.target.value)}
              className="w-full pl-12 pr-4 py-3.5 rounded-2xl border border-white/40 bg-white/50 focus:bg-white focus:ring-2 focus:ring-blue-500/50 outline-none transition-all text-slate-800 shadow-sm font-medium"
            />
          </div>
          
          <div className="flex-1 overflow-y-auto space-y-3 pr-2 custom-scrollbar">
            {isLoading ? (
              <div className="flex justify-center p-8"><Loader2 className="h-6 w-6 animate-spin text-blue-500" /></div>
            ) : (
              notes?.filter(n => n.title.toLowerCase().includes(searchQuery.toLowerCase())).map(note => (
                <div 
                  key={note.id} 
                  onClick={() => { setSelectedNote(note); setIsEditing(false); }}
                  className={`glass p-5 rounded-2xl cursor-pointer transition-all duration-200 border-l-4 ${
                    selectedNote?.id === note.id ? 'border-l-blue-500 bg-white/80 shadow-md' : 'border-l-transparent hover:bg-white/60'
                  }`}
                >
                  <h4 className="font-bold text-slate-800 mb-1">{note.title}</h4>
                  <div className="flex justify-between text-xs font-medium text-slate-500">
                    <span>{new Date(note.date).toLocaleDateString()}</span>
                    <span>{note.preacherName}</span>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Right pane: Editor / Viewer */}
        <div className="flex-1 glass rounded-3xl overflow-hidden flex flex-col">
          {isEditing ? (
            <div className="flex flex-col h-full bg-white/40">
              <div className="p-6 border-b border-white/40 flex justify-between items-center bg-white/20">
                <input 
                  type="text" 
                  value={editForm.title} 
                  onChange={e => setEditForm({...editForm, title: e.target.value})}
                  className="text-2xl font-bold bg-transparent border-none outline-none text-slate-800 w-full placeholder:text-slate-400"
                  placeholder="Note Title"
                />
                <button onClick={handleSave} className="flex items-center gap-2 bg-blue-500 text-white px-5 py-2 rounded-xl font-bold hover:bg-blue-600 transition-colors shadow-sm">
                  <Save className="h-4 w-4" /> Save
                </button>
              </div>
              <div className="p-6 border-b border-white/40 flex flex-wrap gap-4 bg-white/10">
                <input 
                  type="date" 
                  value={editForm.date?.split('T')[0]} 
                  onChange={e => setEditForm({...editForm, date: e.target.value})}
                  className="bg-white/50 border border-white/40 rounded-xl px-4 py-2 outline-none focus:ring-2 focus:ring-blue-500 text-sm font-medium text-slate-700"
                />
                <input 
                  type="text" 
                  value={editForm.preacherName || ''} 
                  onChange={e => setEditForm({...editForm, preacherName: e.target.value})}
                  placeholder="Preacher"
                  className="bg-white/50 border border-white/40 rounded-xl px-4 py-2 outline-none focus:ring-2 focus:ring-blue-500 text-sm font-medium text-slate-700 flex-1"
                />
                <input 
                  type="text" 
                  value={editForm.scriptureReference} 
                  onChange={e => setEditForm({...editForm, scriptureReference: e.target.value})}
                  placeholder="Scripture (e.g. John 3:16)"
                  className="bg-white/50 border border-white/40 rounded-xl px-4 py-2 outline-none focus:ring-2 focus:ring-blue-500 text-sm font-medium text-slate-700 flex-1"
                />
              </div>
              <textarea 
                value={editForm.notes || ''} 
                onChange={e => setEditForm({...editForm, notes: e.target.value})}
                placeholder="Start typing your notes here..."
                className="flex-1 bg-transparent border-none outline-none p-8 resize-none text-slate-700 leading-relaxed"
              />
            </div>
          ) : selectedNote ? (
            <div className="flex flex-col h-full bg-white/20">
              <div className="p-8 border-b border-white/40">
                <div className="flex justify-between items-start mb-4">
                  <h2 className="text-3xl font-extrabold text-slate-800">{selectedNote.title}</h2>
                  <div className="flex gap-2">
                    <button onClick={() => scriptureLookupMutation.mutate(selectedNote.id)} title="AI Scripture Lookup" className="p-2 text-indigo-600 bg-indigo-50 hover:bg-indigo-100 rounded-xl transition-colors">
                      <BookOpen className="h-5 w-5" />
                    </button>
                    <button onClick={() => handleEdit(selectedNote)} className="p-2 text-blue-600 bg-blue-50 hover:bg-blue-100 rounded-xl transition-colors">
                      <Edit3 className="h-5 w-5" />
                    </button>
                    <button onClick={() => deleteMutation.mutate(selectedNote.id)} className="p-2 text-rose-600 bg-rose-50 hover:bg-rose-100 rounded-xl transition-colors">
                      <Trash2 className="h-5 w-5" />
                    </button>
                  </div>
                </div>
                <div className="flex gap-4 text-sm font-semibold text-slate-500 bg-white/40 p-4 rounded-2xl inline-flex border border-white/30">
                  <span>{new Date(selectedNote.date).toLocaleDateString()}</span>
                  <span>•</span>
                  <span>{selectedNote.preacherName || 'No speaker'}</span>
                  <span>•</span>
                  <span className="text-indigo-600">{selectedNote.scriptureReference || 'No scripture ref'}</span>
                </div>
              </div>
              <div className="p-8 overflow-y-auto flex-1">
                {selectedNote.scriptureText && (
                  <div className="mb-6 p-4 bg-indigo-50 border border-indigo-100 rounded-xl text-indigo-900 text-sm italic">
                    "{selectedNote.scriptureText}"
                  </div>
                )}
                <p className="text-slate-700 leading-relaxed whitespace-pre-wrap text-lg">{selectedNote.notes}</p>
              </div>
            </div>
          ) : (
            <div className="flex-1 flex flex-col items-center justify-center text-slate-400 p-8 text-center bg-white/10">
              <BookOpen className="h-16 w-16 mb-4 text-slate-300 opacity-50" />
              <p className="text-lg font-medium">Select a note to read or edit</p>
              <p className="text-sm mt-2">Or create a new note to start typing</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
