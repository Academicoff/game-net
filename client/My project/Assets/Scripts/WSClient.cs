using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WSClient : MonoBehaviour
{
    public static WSClient Instance { get; private set; }

    public static event Action<List<LobbyInfo>> OnLobbyList;
    public static event Action<int> OnCountdown;

    [Serializable]
    public class LobbyInfo
    {
        public string id;
        public string name;
        public int players;
    }

    private WebSocket ws;

    void Awake()
    {
        Instance = this;
    }

    async void Start()
    {
        ws = new WebSocket("ws://localhost:8080/ws");

        ws.OnOpen += () => Debug.Log("WS ► open");
        ws.OnError += e => Debug.LogError("WS error: " + e);
        ws.OnClose += e => Debug.Log("WS ◄ closed: " + e);
        ws.OnMessage += HandleMessage;

        await ws.Connect();
    }

    void Update() => ws?.DispatchMessageQueue();

    async void OnApplicationQuit()
    {
        if (ws != null)
            await ws.Close();
    }

    public async Task SendRaw(string json)
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            Debug.Log($"[SendRaw] → {json}");
            await ws.SendText(json);
        }
    }

    private void HandleMessage(byte[] bytes)
    {
        var json = Encoding.UTF8.GetString(bytes);
        var msg = JObject.Parse(json);

        var type = msg["type"]?.ToString();

        switch (type)
        {
            case "LobbyList":
                HandleLobbyList(msg["payload"]);
                break;

            case "StartCountdown":
                int seconds = msg["payload"]?["seconds"]?.ToObject<int>() ?? 10;
                Debug.Log("▶ StartCountdown: " + seconds + " сек");
                OnCountdown?.Invoke(seconds);
                break;

            case "MatchStart":
                HandleMatchStart(msg["payload"]);
                break;

            case "LobbyUpdate":
                Debug.Log("▶ LobbyUpdate: " + msg["payload"]);
                break;

            case "HandSync":
                HandleHandSync(msg["payload"]);
                break;

            default:
                Debug.Log("WS ⇄ неизвестное сообщение: " + json);
                break;
        }
    }

    private void HandleLobbyList(JToken payload)
    {
        var list = new List<LobbyInfo>();
        foreach (var jTok in payload!)
        {
            list.Add(new LobbyInfo
            {
                id = jTok["id"]?.ToString(),
                name = jTok["name"]?.ToString(),
                players = jTok["players"]?.ToObject<int>() ?? 0
            });
        }

        Debug.Log($"Получен LobbyList ({list.Count})");
        OnLobbyList?.Invoke(list);
    }

    private void HandleMatchStart(JToken payload)
    {
        Debug.Log($"▶ MatchStart RAW: {payload}");

        int youAre = payload?["youAre"]?.ToObject<int>() ?? 0;

        // hand может быть, а может быть пустым массивом
        var handArr = payload?["hand"]?.ToObject<List<string>>();
        if (handArr == null)
        {
            Debug.LogWarning("⚠ MatchStart без поля hand (продолжаем без карт)");
            handArr = new List<string>();          // создаём пустой, но НЕ прерываемся
        }

        MatchSpawner.YouAre = youAre;
        MatchSpawner.HandData = handArr;

        Debug.Log($"▶ YouAre: {youAre}, карт: {handArr.Count}");

        // грузим сцену ВСЕГДА
        SceneManager.sceneLoaded += OnBattleSceneLoaded;
        SceneManager.LoadScene("BattleScene");
    }

    private void OnBattleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "BattleScene") return;

        Debug.Log("[WSClient] BattleScene загружена");

        var spawner = FindObjectOfType<MatchSpawner>();
        if (spawner != null)
        {
            spawner.OnMatchStart(); // Важно: метод должен быть public
        }
        else
        {
            Debug.LogError("[WSClient] MatchSpawner не найден на сцене!");
        }

        SceneManager.sceneLoaded -= OnBattleSceneLoaded;
    }
    
    private void HandleHandSync(JToken payload)
    {
        var cards = payload?["hand"]?.ToObject<List<string>>();
        if (cards == null) return;

        MatchSpawner.HandData = cards;
        Debug.Log($"▶ HandSync получен, карт: {cards.Count}");

        var spawner = FindObjectOfType<MatchSpawner>();
        if (spawner != null)
            spawner.RefreshHand();          // метод, который очищает и заново кладёт карты
    }

}
