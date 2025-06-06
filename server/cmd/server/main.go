package main

import (
	"encoding/json"
	"log"
	"net/http"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/gorilla/websocket"
)

/* ---------- базовые типы сообщений ---------- */

type Msg struct {
	Type    string      `json:"type"`
	Payload interface{} `json:"payload,omitempty"`
}

type (
	CreateLobbyPayload struct {
		Name string `json:"name"`
	}
	JoinLobbyPayload struct {
		LobbyID string `json:"lobbyId"`
	}
	ReadyPayload struct {
		Ready bool `json:"ready"`
	}
)

type LobbyInfo struct {
	ID      string `json:"id"`
	Name    string `json:"name"`
	Players int    `json:"players"`
}

type PlayerInfo struct {
	ID    string `json:"id"`
	Name  string `json:"name"`
	Ready bool   `json:"ready"`
}

/* ---------- модели лобби и игроков ---------- */

type Player struct {
	ID   string
	Conn *websocket.Conn
}

type Lobby struct {
	ID      string
	Name    string
	Players map[string]*Player
	Ready   map[string]bool
}

/* ---------- глобальное хранилище лобби ---------- */

var (
	upgrader  = websocket.Upgrader{CheckOrigin: func(r *http.Request) bool { return true }}
	lobbies   = make(map[string]*Lobby)
	lobbiesMu sync.RWMutex
)

/* ---------- вспомогательные функции ---------- */

func send(conn *websocket.Conn, v interface{}) {
	_ = conn.WriteJSON(v)
}

func listLobbies() []LobbyInfo {
	lobbiesMu.RLock()
	defer lobbiesMu.RUnlock()

	out := make([]LobbyInfo, 0, len(lobbies))
	for _, l := range lobbies {
		out = append(out, LobbyInfo{ID: l.ID, Name: l.Name, Players: len(l.Players)})
	}
	return out
}

func broadcastLobbyList() {
	infos := listLobbies()
	msg := Msg{Type: "LobbyList", Payload: infos}

	lobbiesMu.RLock()
	defer lobbiesMu.RUnlock()
	for _, l := range lobbies {
		for _, p := range l.Players {
			send(p.Conn, msg)
		}
	}
}

/* ---------- методы лобби ---------- */

func (l *Lobby) broadcastState() {
	list := make([]PlayerInfo, 0, len(l.Players))
	for id := range l.Players {
		list = append(list, PlayerInfo{ID: id, Name: id, Ready: l.Ready[id]})
	}
	msg := Msg{Type: "LobbyUpdate", Payload: list}
	for _, p := range l.Players {
		send(p.Conn, msg)
	}
}

func (l *Lobby) everyoneReady() bool {
	if len(l.Players) != 2 {
		return false
	}
	for id := range l.Players {
		if !l.Ready[id] {
			return false
		}
	}
	return true
}

func (l *Lobby) startCountdown() {
	countMsg := Msg{Type: "StartCountdown", Payload: map[string]int{"seconds": 10}}
	for _, p := range l.Players {
		send(p.Conn, countMsg)
	}

	time.AfterFunc(10*time.Second, func() {
		startMsg := Msg{Type: "MatchStart"}
		for _, p := range l.Players {
			send(p.Conn, startMsg)
		}
	})
}

/* ---------- обработка WebSocket ---------- */

func handleWS(w http.ResponseWriter, r *http.Request) {
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		return
	}
	defer conn.Close()

	playerID := uuid.New().String()
	var curLobby *Lobby

	// сразу отправляем список лобби
	send(conn, Msg{Type: "LobbyList", Payload: listLobbies()})

	for {
		var m Msg
		if err := conn.ReadJSON(&m); err != nil {
			break
		}

		switch m.Type {

		case "CreateLobby":
			var p CreateLobbyPayload
			_ = json.Unmarshal(mustJSON(m.Payload), &p)

			l := &Lobby{
				ID:      uuid.New().String(),
				Name:    p.Name,
				Players: map[string]*Player{},
				Ready:   map[string]bool{},
			}
			lobbiesMu.Lock()
			lobbies[l.ID] = l
			lobbiesMu.Unlock()

			l.Players[playerID] = &Player{ID: playerID, Conn: conn}
			l.Ready[playerID] = false
			curLobby = l
			l.broadcastState()
			broadcastLobbyList()

		case "JoinLobby":
			var p JoinLobbyPayload
			_ = json.Unmarshal(mustJSON(m.Payload), &p)

			lobbiesMu.RLock()
			l, ok := lobbies[p.LobbyID]
			lobbiesMu.RUnlock()
			if ok && len(l.Players) < 2 {
				l.Players[playerID] = &Player{ID: playerID, Conn: conn}
				l.Ready[playerID] = false
				curLobby = l
				l.broadcastState()
				broadcastLobbyList()
			}

		case "LeaveLobby":
			if curLobby != nil {
				delete(curLobby.Players, playerID)
				delete(curLobby.Ready, playerID)
				curLobby.broadcastState()
				broadcastLobbyList()
				if len(curLobby.Players) == 0 {
					lobbiesMu.Lock()
					delete(lobbies, curLobby.ID)
					lobbiesMu.Unlock()
				}
				curLobby = nil
			}

		case "Ready":
			var p ReadyPayload
			_ = json.Unmarshal(mustJSON(m.Payload), &p)

			if curLobby != nil {
				curLobby.Ready[playerID] = p.Ready
				curLobby.broadcastState()
				if curLobby.everyoneReady() {
					curLobby.startCountdown()
				}
			}
		}
	}

	// очистка при разрыве
	if curLobby != nil {
		delete(curLobby.Players, playerID)
		delete(curLobby.Ready, playerID)
		curLobby.broadcastState()
		broadcastLobbyList()
	}
}

func mustJSON(v interface{}) []byte {
	switch t := v.(type) {
	case json.RawMessage:
		return t
	default:
		b, _ := json.Marshal(v)
		return b
	}
}

/* ---------- точка входа ---------- */

func main() {
	http.HandleFunc("/ws", handleWS)
	log.Println("Сервер слушает ws://localhost:8080/ws")
	log.Fatal(http.ListenAndServe(":8080", nil))
}
