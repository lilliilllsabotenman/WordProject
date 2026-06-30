using UnityEngine;

[CreateAssetMenu(menuName = "WordBox")]
public class PlayerTextSpriteData : ScriptableObject
{
    public Sprite sprite;

    [Tooltip("TMPの<sprite scale=X>に渡すuniform scale値。XY個別にサイズを変えたい場合はこの方式では対応不可")]
    [Range(0.1f, 3.0f)] // TODO: プロジェクトの仕様に応じて上下限を調整すること
    public float Scale = 1f;

    public string GetSpriteName()
    {
        return sprite != null ? sprite.name : string.Empty;
    }
    }