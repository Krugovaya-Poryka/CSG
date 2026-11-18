using UnityEngine;

// Atlas regions from Lamoot's RPG GUI construction kit (CC BY 3.0).
public static class ClassicStrategySkin
{
    private static Sprite wood, frame;
    public static Sprite Wood
    {
        get
        {
            if (wood == null)
            {
                Texture2D texture = Resources.Load<Texture2D>("StrategyUI/wood-panel");
                if (texture != null) wood = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 100);
            }
            return wood;
        }
    }
    public static Sprite Frame
    {
        get
        {
            if (frame == null)
            {
                Texture2D atlas = Resources.Load<Texture2D>("StrategyUI/classic-ui-atlas");
                if (atlas != null) frame = Sprite.Create(atlas, new Rect(121, atlas.height - 529, 75, 75), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect, new Vector4(5, 5, 5, 5));
            }
            return frame;
        }
    }
}
