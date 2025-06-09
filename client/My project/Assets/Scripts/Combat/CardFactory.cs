using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Создаёт карты по строке вида "10_Hearts", "Q_Spades", "Back"
/// Использует спрайты из атласа (лицо) + отдельный спрайт для рубашки
/// </summary>
public static class CardFactory
{
    private static Dictionary<string, Sprite> spriteMap;
    private static Sprite backSprite;
    private static GameObject cardPrefab;

    static CardFactory()
    {
        LoadResources();
    }

    private static void LoadResources()
    {
        spriteMap = new Dictionary<string, Sprite>();

        // Загружаем все спрайты из нарезанного атласа pixelcards.png
        Sprite[] sprites = Resources.LoadAll<Sprite>("CardFaces/pixelcards");

        foreach (var sprite in sprites)
        {
            spriteMap[sprite.name] = sprite;
        }

        // Отдельный спрайт рубашки
        backSprite = Resources.Load<Sprite>("CardBack/card_back");
        if (backSprite == null)
        {
            Debug.LogError("[CardFactory] Не найден спрайт рубашки по пути Resources/CardBack/card_back.png");
        }

        cardPrefab = Resources.Load<GameObject>("Card3D");
        if (cardPrefab == null)
        {
            Debug.LogError("[CardFactory] Префаб Card3D не найден в Resources/");
        }
    }

    public static Card Create(string code, bool faceUp)
    {
        if (cardPrefab == null || spriteMap == null || backSprite == null)
            LoadResources();

        GameObject obj = Object.Instantiate(cardPrefab);
        Card card = obj.GetComponent<Card>();

        // Карта с рубашкой (или faceUp = false)
        if (code == "Back" || !faceUp)
        {
            card.SetCard(null, backSprite, 0, Card.Suit.Hearts); // Значения можно не задавать
            card.Flip(false);
            return card;
        }

        // Разбор строки: например, "10_Hearts"
        var parts = code.Split('_');
        if (parts.Length != 2)
        {
            Debug.LogWarning($"[CardFactory] Неверный код карты: {code}");
            return card;
        }

        int value = ParseValue(parts[0]);
        Card.Suit suit = Card.ParseSuit(parts[1]);

        if (!spriteMap.TryGetValue(code, out var faceSprite))
        {
            Debug.LogWarning($"[CardFactory] Спрайт '{code}' не найден в атласе pixelcards");
            return card;
        }

        card.SetCard(faceSprite, backSprite, value, suit);
        return card;
    }

    private static int ParseValue(string face)
    {
        return face switch
        {
            "J" => 11,
            "Q" => 12,
            "K" => 13,
            "A" => 14,
            _ => int.TryParse(face, out var v) ? v : 2
        };
    }
}
