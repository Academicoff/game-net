package lobby

import "sync"

type Manager struct {
	lobbies map[string]*Lobby
	mu      sync.RWMutex
}

func NewManager() *Manager {
	return &Manager{lobbies: map[string]*Lobby{}}
}

func (m *Manager) Create(name string) *Lobby {
	l := New(name)
	m.mu.Lock()
	m.lobbies[l.ID] = l
	m.mu.Unlock()
	return l
}

func (m *Manager) List() []*Lobby {
	m.mu.RLock()
	defer m.mu.RUnlock()
	out := make([]*Lobby, 0, len(m.lobbies))
	for _, l := range m.lobbies {
		out = append(out, l)
	}
	return out
}

func (m *Manager) Get(id string) (*Lobby, bool) {
	m.mu.RLock()
	defer m.mu.RUnlock()
	l, ok := m.lobbies[id]
	return l, ok
}
