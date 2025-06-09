using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Спавнит карты из руки игрока и соперника при старте боя
/// </summary>
public class MatchSpawner : MonoBehaviour
{
    [Header("Расположение рук")]
    public HandLayout handBottom;
    public HandLayout handTop;

    public static int YouAre = -1;
    public static List<string> HandData;

    void Start()
    {
        OnMatchStart(); // вызывается после загрузки сцены BattleScene
    }

    public void OnMatchStart()
        {
        if (HandData == null)                  // оставляем только null-check
        {
            Debug.LogError("[MatchSpawner] HandData null");
            return;
        }

        // если список пуст – просто ничего не рисуем
        if (HandData.Count == 0)
        {
            Debug.Log("[MatchSpawner] Рука пустая – показывать нечего");
            return;
        }

        var myHand = YouAre == 0 ? handBottom : handTop;
        var enemyHand = YouAre == 0 ? handTop : handBottom;

        Debug.Log($"[MatchSpawner] YouAre: {YouAre} | Карт в руке: {HandData.Count}");

        // Спавн твоих карт лицом вверх
        foreach (var code in HandData)
        {
            var card = CardFactory.Create(code, faceUp: true);
            myHand.AddCard(card.transform);
        }

        // Спавн рубашек противника
        for (int i = 0; i < HandData.Count; i++)
        {
            var backCard = CardFactory.Create("Back", faceUp: false);
            enemyHand.AddCard(backCard.transform);
        }

        Debug.Log("[MatchSpawner] Спавн карт завершён");
    }

    public void RefreshHand()
    {
        if (HandData == null) return;

        // убрать старые карты
        handBottom.Clear();
        handTop.Clear();

        OnMatchStart();          // повторно выложить (уже с картами)
    }
}
