using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using NovelGameDialogue;
using UnityEngine.UI;
using MyTween;

///<summary>
/// この終わってるクラスのハードコードをやったやつは死刑
/// 解読不可！！
/// <summary>

[System.Serializable]
public class TutorealNovelData
{
    [Header("最初のやつ")]
    public NovelGameSettings a;

    [Header("ここで文字スロット表示")]
    public NovelAnimationSettings SlotActive;

    [Header("文字強調")]
    public NovelAnimationSettings TextEmphasis;

    [Header("レスだ抜き解説２")]
    public NovelGameSettings c;

    [Header("レスだぬきブちぎれ")]
    public NovelGameSettings d;

    [Header("レスだ抜きほめちぎり")]
    public NovelGameSettings e;

    [Header("二文字目")]
    public NovelGameSettings f;

    [Header("に文字とも正解")]
    public NovelGameSettings g;

    [Header("演出")]
    public NovelAnimationSettings WinEffect;

    [Header("最後の方の会話")]
    public NovelGameSettings h;


    [Header("やり発射前")]
    public NovelGameSettings before;

    [Header("やり発射あと")]
    public NovelGameSettings After;
}

public class TutorealManagere : MonoBehaviour
{
    [Header("会話手順")]
    [SerializeField] private TutorealNovelData tutorealData;

    [Header("ゲーム設定")]
    [SerializeField] private GameSequenceData gameData; 

    [Header("会話データ設定")]
    [SerializeField] private List<NovelGameSettings> novelGameSettings;

    [Header("会話パート用NovelEventManager")]
    [SerializeField] private NovelEventManager novelEventManager;

    [Header("敵の文字表示・制限時間管理")]
    [SerializeField] private TextScaleAnimation textObject;

    [Header("反論組み立て用テキスト")]
    [SerializeField] private Image sentenceText;

    [Header("カード表示用テキスト(選んだ順に入る)")]
    [SerializeField] private List<TextMeshProUGUI> sentenceSlots;

    [Header("プレイヤーライフ管理")]
    [SerializeField] private LifeManager lifeManager;

    [Header("単語選択用マウスカーソル")]
    [SerializeField] private MouseControllBase mouse;

    [Header("正解用WordAssets")]
    [SerializeField] private WordAsset Answer_1;

    [Header("正解用WordAssets")]
    [SerializeField] private WordAsset Answer_2;

    [Header("トーン♡")]
    [SerializeField] private Image t1;

    [Header("クリア文字フェード表示用")]
    [SerializeField] private TextMeshProUGUI crossfadeTextTo;
    [SerializeField] private float crossfadeDuration = 0.5f;

    [Header("クリア文字表示(Answer_1/Answer_2 正解時)")]
    [SerializeField] private string clearTextAnswer1;
    [SerializeField] private string clearTextAnswer2;

    [Header("後アニメーション(再生タイミングはPlayPostAnimation()呼び出し側で指定)")]
    [SerializeField] private DataSequence postAnimation;

    private RoundManager roundManager;
    private NovelAnimationManager animationManager;

    private WordAsset Answer;

    private bool bAnswer_1 = false;
    private bool bAnswer_2 = false;
    private bool GetWord = false;

private void Awake()
{
    animationManager = new NovelAnimationManager(
        UnityEngine.Object.FindObjectsByType<NovelAnimationObject>(FindObjectsSortMode.None));

    if (crossfadeTextTo != null) crossfadeTextTo.alpha = 0f;
}

    /// <summary>
    /// 既存のMyTween Fadeインフラを使ってクリア文字をフェード表示する
    /// (BattleManager.RevealClearTextAsync()と同じ方式。EnemyText側への依存は無い)。
    /// </summary>
    private async Task PlayCrossfadeText(string text)
    {
        if (crossfadeTextTo == null) return;

        crossfadeTextTo.text = text;

        var fadeData = new FadeRuntimeData
        {
            Target = new TextMeshProUGUIAdapter(crossfadeTextTo),
            Start = crossfadeTextTo.alpha,
            End = 1f,
            Duration = crossfadeDuration
        };
        await new FadeRuntimeCommand(fadeData).ExecuteAsync();
    }
    public async Task PlayPostAnimation()
    {
        if (postAnimation == null) return;

        await postAnimation.PlayAsync(animationManager);
    }

    private const float ShatterForce = 15f;

    /// <summary>
    /// BattleManager.ShatterCurrentTextObject()の移植。textObjectにSpriteRendererが
    /// 存在すること(SpriteRenderer前提のExplodable/SpriteExploderを使うため)が必須。
    /// </summary>
    private void ShatterTextObject()
    {
        if (textObject == null) return;

        GameObject target = textObject.gameObject;
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogWarning("ShatterTextObject: textObjectにSpriteRenderer(またはSprite)がありません。破砕をスキップします。");
            return;
        }

        Vector3 center = target.transform.position;

        BoxCollider2D box = target.AddComponent<BoxCollider2D>();
        box.size = sr.sprite.bounds.size;

        Explodable explodable = target.AddComponent<Explodable>();
        explodable.allowRuntimeFragmentation = true;
        explodable.extraPoints = 5;
        explodable.explode();

        foreach (GameObject frag in explodable.fragments)
        {
            Rigidbody2D rb = frag.GetComponent<Rigidbody2D>();
            Vector2 dir = ((Vector2)frag.transform.position - (Vector2)center).normalized;
            if (dir == Vector2.zero) dir = UnityEngine.Random.insideUnitCircle.normalized;
            rb.AddForce(dir * ShatterForce, ForceMode2D.Impulse);
        }
    }

    private void Clear()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("End");
    }

    private void GameOver()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("BeginPlay");
    }

    private void Start()
    {
        _ = Tutoreal();
    }

    
    private async Task Tutoreal()//Todoこれをループにまとめられると思うのでそうしろ！！演出含め！！これ書いたあほ間抜けは重罪
    {
        mouse.IsControll = false;
        // ===== 最初のやつ =====
        await novelEventManager.PlayAsync(tutorealData.a);

        await MassageExecuter.WaitUntilAsync(() => Input.GetMouseButtonDown(0));

        // ===== ここで文字スロット表示 =====
        t1.gameObject.SetActive(false);       
        

        // ===== 文字強調 =====
        while (true)
        {
            Color color = sentenceText.color;

            color.a += (1f - color.a) / 60f;

            if (color.a >= 0.95f)
            {
                color.a = 1f;
                sentenceText.color = color;
                break;
            }

            sentenceText.color = color;

            await Task.Yield();
        }
        
        await MassageExecuter.WaitUntilAsync(() => Input.GetMouseButtonDown(0));

        t1.gameObject.SetActive(true); 
        t1.transform.SetSiblingIndex(7); 

        await MassageExecuter.WaitUntilAsync(() => Input.GetMouseButtonDown(0));


        // ===== レスだ抜き解説２ =====
        await novelEventManager.PlayAsync(tutorealData.c);

        
        WordObject[] words = UnityEngine.Object.FindObjectsByType<WordObject>(FindObjectsSortMode.None);
        List<GameObject> wObj = new ();
        foreach(WordObject w in words)
        {
            wObj.Add(w.gameObject);
        }

        foreach(GameObject w in wObj)
        {
            w.SetActive(true);
        }


        Answer = Answer_1;
        mouse.MouseSubscrive(JudgmentWord);

        mouse.IsControll = true;
        await MassageExecuter.WaitUntilAsync(() => GetWord);
        GetWord = false;

        // ===== レスだぬきブちぎれ =====
        while(!bAnswer_1)
        {
            foreach(GameObject w in wObj)
            {
                w.SetActive(true);
            }

            mouse.IsControll = false;
            await novelEventManager.PlayAsync(tutorealData.d);

            mouse.IsControll = true;
            await MassageExecuter.WaitUntilAsync(() => GetWord);
            GetWord = false;
        }

        mouse.IsControll = false;

        // t1.gameObject.SetActive(false);
        t1.transform.SetSiblingIndex(9);
        Debug.Log("Tutoreal2");
        // ===== レスだ抜きほめちぎり =====
        await novelEventManager.PlayAsync(tutorealData.e);

        GameObject aaa = null;

        t1.gameObject.SetActive(false); 

        foreach(GameObject w in wObj)
        {
            if(!w.activeSelf)
            {
                aaa = w;
            }
        }

        wObj.Remove(aaa);

        await MassageExecuter.WaitUntilAsync(() => Input.GetMouseButtonDown(0));
        Debug.Log("Tutoreal3");
        // ===== 二文字目 =====


        Answer = Answer_2;

        t1.gameObject.SetActive(false);

        mouse.IsControll = true;
        await MassageExecuter.WaitUntilAsync(() => GetWord);
        GetWord = false;

        while(!bAnswer_2)
        {
            foreach(GameObject w in wObj)
            {
                w.SetActive(true);
            }

            mouse.IsControll = false;
            await novelEventManager.PlayAsync(tutorealData.d);

            mouse.IsControll = true;
            await MassageExecuter.WaitUntilAsync(() => GetWord);
            GetWord = false;
        }

        await novelEventManager.PlayAsync(tutorealData.before);

        await PlayPostAnimation();
        ShatterTextObject();
        await PlayCrossfadeText(clearTextAnswer2);

        mouse.IsControll = false;



        t1.gameObject.SetActive(true);

        await novelEventManager.PlayAsync(tutorealData.After);

        Debug.Log("syuuryou");

        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    private void JudgmentWord(WordAsset word)
    {
        GetWord = true;

        Debug.Log($"[JudgmentWord] clicked={(word != null ? word.name : "null")}, Answer={(Answer != null ? Answer.name : "null")}, match={word == Answer}, mouseScreenPos={Input.mousePosition}");

        if(word == Answer)
        {
            if(Answer == Answer_1) bAnswer_1 = true;
            if(Answer == Answer_2) bAnswer_2 = true;

            int slotIndex = Answer == Answer_1 ? 0 : Answer == Answer_2 ? 1 : -1;
            if (sentenceSlots != null && slotIndex >= 0 && slotIndex < sentenceSlots.Count && sentenceSlots[slotIndex] != null)
            {
                sentenceSlots[slotIndex].text = word.DisplayText;
            }
        }
    }
}