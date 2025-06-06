package lobby

import (
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/yourname/game-net/server/internal/transport"
)

type PlayerConn struct {
	ID    string
	Name  string
	WS    WebSocket // интерфейс с методами Send/Recv (сделаем ниже)
	Ready bool
}

type Lobby struct {
	ID      string
	Name    string
	Players []*PlayerConn
	mu      sync.Mutex
}

func New(name string) *Lobby {
	return &Lobby{
		ID:   uuid.New().String(),
		Name: name,
	}
}

func (l *Lobby) Add(p *PlayerConn) bool {
	l.mu.Lock()
	defer l.mu.Unlock()
	if len(l.Players) >= 2 {
		return false
	}
	l.Players = append(l.Players, p)
	l.broadcastState()
	return true
}

func (l *Lobby) Remove(id string) {
	l.mu.Lock()
	defer l.mu.Unlock()
	for i, p := range l.Players {
		if p.ID == id {
			l.Players = append(l.Players[:i], l.Players[i+1:]...)
			break
		}
	}
	l.broadcastState()
}

func (l *Lobby) SetReady(id string, ready bool) {
	l.mu.Lock()
	defer l.mu.Unlock()
	for _, p := range l.Players {
		if p.ID == id {
			p.Ready = ready
		}
	}
	l.broadcastState()
	if l.everyoneReady() {
		l.startCountdown()
	}
}

func (l *Lobby) broadcastState() {
	list := make([]transport.PlayerInfo, len(l.Players))
	for i, p := range l.Players {
		list[i] = transport.PlayerInfo{ID: p.ID, Name: p.Name, Ready: p.Ready}
	}
	msg := transport.Msg{
		Type:    "LobbyUpdate",
		Payload: transport.LobbyUpdatePayload{Players: list},
	}
	for _, p := range l.Players {
		p.WS.Send(msg)
	}
}

func (l *Lobby) everyoneReady() bool {
	if len(l.Players) != 2 {
		return false
	}
	for _, p := range l.Players {
		if !p.Ready {
			return false
		}
	}
	return true
}

func (l *Lobby) startCountdown() {
	msg := transport.Msg{Type: "StartCountdown", Payload: transport.CountdownPayload{Seconds: 10}}
	for _, p := range l.Players {
		p.WS.Send(msg)
	}
	time.AfterFunc(10*time.Second, func() {
		start := transport.Msg{Type: "MatchStart"}
		for _, p := range l.Players {
			p.WS.Send(start)
		}
	})
}
