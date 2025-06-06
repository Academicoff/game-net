package transport

type Msg struct {
	Type    string      `json:"type"`
	Payload interface{} `json:"payload,omitempty"`
}

// запросы клиента
type CreateLobbyPayload struct {
	Name string `json:"name"`
}
type JoinLobbyPayload struct {
	LobbyID string `json:"lobbyId"`
}
type ReadyPayload struct {
	Ready bool `json:"ready"`
}

// ответы сервера
type LobbyListPayload struct {
	Lobbies []LobbyInfo `json:"lobbies"`
}
type LobbyInfo struct {
	ID      string `json:"id"`
	Name    string `json:"name"`
	Players int    `json:"players"`
}

type LobbyUpdatePayload struct {
	Players []PlayerInfo `json:"players"`
}
type PlayerInfo struct {
	ID    string `json:"id"`
	Name  string `json:"name"`
	Ready bool   `json:"ready"`
}

type CountdownPayload struct {
	Seconds int `json:"seconds"`
}
