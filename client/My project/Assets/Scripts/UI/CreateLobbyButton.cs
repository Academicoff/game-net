using System.Threading.Tasks;
using NativeWebSocket;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CreateLobbyButton : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private async void OnClick()
    {
        string lobbyName = string.IsNullOrWhiteSpace(nameInput?.text)
            ? "New Lobby" : nameInput.text;

        // формируем JSON через JObject
        var msg = new JObject
        {
            ["type"]    = "CreateLobby",
            ["payload"] = new JObject { ["name"] = lobbyName }
        };

        await WSClient.Instance.SendRaw(msg.ToString());
    }
}
