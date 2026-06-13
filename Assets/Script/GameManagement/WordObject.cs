using UnityEngine;
using TMPro;

/// <summary>
/// 単語UIとWordAssetを紐づける表示オブジェクトです。
/// </summary>
public class WordObject : MonoBehaviour
{
    [Header("このUIに対応する単語データ")]
    [SerializeField] private WordAsset wordAsset;

    private TextMeshProUGUI displayText;
    private float amplitude = 10f;
    private float speed = 5f;

    private float difference;

    private Vector3 startPosition;

    /// <summary>
    /// 起動時にWordAssetの表示文字列をUIへ反映します。
    /// </summary>
    private void Awake()
    {
        displayText = GetComponent<TextMeshProUGUI>();

        if (displayText != null && wordAsset != null)
        {
            displayText.text = wordAsset.DisplayText;
        }

        difference = Random.Range(0, 2);
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float offsetX =
            Mathf.Sin((Time.time + difference) * speed) * amplitude;

        transform.position =
            startPosition + new Vector3(offsetX, 0f, 0f);
    }

    /// <summary>
    /// 紐づいたWordAssetを取得します。
    /// </summary>
    public bool TryGetWordAsset(out WordAsset asset)
    {
        asset = wordAsset;

        this.gameObject.SetActive(false);
        return asset != null;
    }

    public void SetWordAsset(WordAsset wordAssets)
    {
        this.wordAsset = wordAssets;

        if (displayText != null && wordAsset != null)
        {
            displayText.text = wordAsset.DisplayText;
        }
    }
}