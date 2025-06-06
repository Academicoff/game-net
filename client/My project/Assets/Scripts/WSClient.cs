using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class WSClient : MonoBehaviour
{

    public static WSClient Instance { get; private set; }
    /* -------- Публичное событие, UI подпишется -------- */
    public static event Action<List<LobbyInfo>> OnLobbyList;

    /* -------- Структура для удобства -------- */
    [Serializable]
    public class LobbyInfo
    {
        public string id;
        public string name;
        public int players;
    }

    /* -------- Внутренние поля -------- */
    private WebSocket ws;

    async void Start()
    {
        ws = new WebSocket("ws://localhost:8080/ws");

        ws.OnOpen    += () => Debug.Log("WS ► open");
        ws.OnError   += e  => Debug.LogError("WS error: " + e);
        ws.OnClose   += e  => Debug.Log("WS ◄ closed: " + e);

        ws.OnMessage += HandleMessage;

        await ws.Connect();
        // Больше ничего отправлять не нужно — сервер сам присылает LobbyList
    }

    void Update() => ws?.DispatchMessageQueue();

    void Awake()                // B 
    {
        Instance = this;        // B 
    }  

    public async Task SendRaw(string json)
    {                           // C 
        if (ws != null && ws.State == WebSocketState.Open)
            await ws.SendText(json);
    }  
    async void OnApplicationQuit()
    {
        if (ws != null)
            await ws.Close();
    }

    /* -------- Обработка входящих сообщений -------- */
    private void HandleMessage(byte[] bytes)
    {
        var json = Encoding.UTF8.GetString(bytes);
        var msg  = JObject.Parse(json);

        var type = msg["type"]?.ToString();
        if (type == "LobbyList")
        {
            var list = new List<LobbyInfo>();
            foreach (var jTok in msg["payload"]!)
            {
                list.Add(new LobbyInfo
                {
                    id      = jTok["id"]?.ToString(),
                    name    = jTok["name"]?.ToString(),
                    players = jTok["players"]?.ToObject<int>() ?? 0
                });
            }

            Debug.Log($"Получен LobbyList ({list.Count})");
            OnLobbyList?.Invoke(list);  // сообщаем подписчикам UI
        }
        else if (type == "LobbyUpdate" || type == "StartCountdown" || type == "MatchStart")
        {
            // Эти сообщения будем обрабатывать позже
            Debug.Log($"WS ⇄ {type}: {json}");
        }
        else
        {
            Debug.Log($"WS ⇄ {json}");
        }
    }
}
