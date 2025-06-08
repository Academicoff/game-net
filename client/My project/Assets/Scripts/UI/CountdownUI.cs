using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    [SerializeField] TMP_Text label;

    void Awake()
    {
        WSClient.OnCountdown += StartCountdown;
    }

    void OnDestroy()
    {
        WSClient.OnCountdown -= StartCountdown;
    }

    void StartCountdown(int seconds)
    {
        Debug.Log("[CountdownUI] Старт отсчёта: " + seconds);
        StopAllCoroutines();
        StartCoroutine(Countdown(seconds));
    }

    IEnumerator Countdown(int s)
    {
        label.gameObject.SetActive(true);

        for (int t = s; t > 0; t--)
        {
            label.text = t.ToString();
            yield return new WaitForSeconds(1);
        }

        label.gameObject.SetActive(false);
    }
}
