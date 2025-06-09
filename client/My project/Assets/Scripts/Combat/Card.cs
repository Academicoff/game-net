using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Card : MonoBehaviour
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }

    public int value;
    public Suit suit;
    public bool faceUp = true;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    [Header("Текстуры")]
    public Texture2D atlas;
    public Sprite faceSprite;  // из атласа
    public Sprite backSprite;  // тоже из атласа

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }

    public void SetCard(Sprite face, Sprite back, int val, Suit st)
    {
        value = val;
        suit = st;
        faceSprite = face;
        backSprite = back;
        faceUp = true;

        ApplyUV();
    }

    public void Flip(bool showFace)
    {
        faceUp = showFace;
        ApplyUV();
    }

    void ApplyUV()
    {
        Sprite sprite = faceUp ? faceSprite : backSprite;
        if (sprite == null)
        {
            Debug.LogWarning("[Card] Sprite not assigned");
            return;
        }

        Rect rect = sprite.textureRect;
        Vector2[] uvs = new Vector2[4];

        float atlasWidth = sprite.texture.width;
        float atlasHeight = sprite.texture.height;

        float xMin = rect.x / atlasWidth;
        float xMax = (rect.x + rect.width) / atlasWidth;
        float yMin = rect.y / atlasHeight;
        float yMax = (rect.y + rect.height) / atlasHeight;

        uvs[0] = new Vector2(xMin, yMin); // bottom left
        uvs[1] = new Vector2(xMax, yMin); // bottom right
        uvs[2] = new Vector2(xMin, yMax); // top left
        uvs[3] = new Vector2(xMax, yMax); // top right

        var mesh = meshFilter.mesh;
        mesh.uv = uvs;
    }

    void OnMouseDown()
    {
        Debug.Log($"[Card] Clicked: {value} {suit} ({(faceUp ? "face" : "back")})");
    }
    public static Suit ParseSuit(string str)
    {
        return str.ToLower() switch
        {
            "hearts"   => Suit.Hearts,
            "diamonds" => Suit.Diamonds,
            "clubs"    => Suit.Clubs,
            "spades"   => Suit.Spades,
            _ => Suit.Hearts
        };
    }

}
