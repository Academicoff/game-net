using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyListUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject rowPrefab;

    void Awake()
    {
        WSClient.OnLobbyList += Refresh;
    }

    void OnDestroy()
    {
        WSClient.OnLobbyList -= Refresh;
    }

    void Refresh(List<WSClient.LobbyInfo> list)
    {
        Debug.Log($"[LobbyListUI] Пришло {list.Count} лобби");
        foreach (Transform c in content) Destroy(c.gameObject);

        foreach (var l in list)
        {
            Debug.Log($"[LobbyListUI] Создаю строку: {l.name} / {l.id}");
            var row = Instantiate(rowPrefab, content);
            var rb  = row.GetComponent<RowButton>();
            if (rb == null) rb = row.AddComponent<RowButton>();
            Debug.Log($"[LobbyListUI] Инициализация RowButton на {row.name}");

            rb.Init(l.id, $"{l.name}  [{l.players}/2]");
        }
    }

}
