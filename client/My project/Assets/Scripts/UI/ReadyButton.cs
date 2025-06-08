using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ReadyButton : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    async void OnClick()
    {
        Debug.Log("[ReadyButton] Нажата кнопка READY");

        var json = "{\"type\":\"Ready\",\"payload\":{\"ready\":true}}";
        await WSClient.Instance.SendRaw(json);
    }
}
