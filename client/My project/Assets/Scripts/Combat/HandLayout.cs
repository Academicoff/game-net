using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Кладёт карты дугой от -angle до +angle.
/// Добавляй карты через AddCard(), можно Clear() или Remove().
/// </summary>
public class HandLayout : MonoBehaviour
{
    [Header("Настройки дуги")]
    [SerializeField] float radius = 6f;
    [SerializeField] float maxFanAngle = 40f;
    [SerializeField] int fanLimit = 8;

    readonly List<Transform> cards = new();

    public void AddCard(Transform card)
    {
        cards.Add(card);
        card.SetParent(transform, false);
        UpdateLayout();
    }

    public void RemoveCard(Transform card)
    {
        cards.Remove(card);
        UpdateLayout();
    }

    public void Clear()
    {
        foreach (var c in cards) Destroy(c.gameObject);
        cards.Clear();
    }

    void UpdateLayout()
    {
        int n = cards.Count;
        if (n == 0) return;

        float fanAngle = Mathf.Min(maxFanAngle, n * 6f);
        float startAngle = -fanAngle * 0.5f;

        for (int i = 0; i < n; i++)
        {
            float t = n == 1 ? 0.5f : (float)i / (n - 1);
            float angleDeg = startAngle + t * fanAngle;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(
                Mathf.Sin(angleRad) * radius,
                0,
                -Mathf.Cos(angleRad) * radius
            );

            Transform c = cards[i];
            c.localPosition = pos;
            c.localRotation = Quaternion.Euler(90, 0, -angleDeg);
        }
    }
}
