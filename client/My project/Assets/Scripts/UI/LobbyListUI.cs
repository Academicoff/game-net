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
        // очистить старые
        foreach (Transform child in content) Destroy(child.gameObject);

        foreach (var lobby in list)
        {
            var row = Instantiate(rowPrefab, content);
            row.GetComponentInChildren<TMP_Text>().text =
                $"{lobby.name}  [{lobby.players}/2]";
        }
    }
}
