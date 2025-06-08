using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class RowButton : MonoBehaviour
{
    private string lobbyId;

    public void Init(string id, string text)
    {
        lobbyId = id;
        GetComponentInChildren<TMP_Text>().text = text;

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners(); // важно
        btn.onClick.AddListener(OnClick);

        Debug.Log($"[RowButton] Init — {lobbyId}");
    }

    private async void OnClick()
    {
        Debug.Log($"[RowButton] КЛИК по лобби ID: {lobbyId}");

        string json = $"{{\"type\":\"JoinLobby\",\"payload\":{{\"lobbyId\":\"{lobbyId}\"}}}}";
        await WSClient.Instance.SendRaw(json);
    }
}
