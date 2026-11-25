using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using NovelGameDialogue;
using System.Threading.Tasks;
using MyTween;



public class NovelEventManager : MonoBehaviour
{
    [Header("会話データ設定")]
    [SerializeField] private List<NovelGameData> novelGameSettings;

    [Header("共通表示設定")]
    [SerializeField] private NovelResourceSettings commonResourceSettings;

    [Header("CharacterKey変換DB")]
    [SerializeField] private CharacterKeyDatabase characterKeyDatabase;

    [Header("ナレーション用")]
    [SerializeField] private TMPro.TextMeshProUGUI NarrationText;

    [Header("キャラ会話用")]
    [SerializeField] private TMPro.TextMeshProUGUI CharacterDialogueText;

    [Header("キャラ名表示(未使用なら未設定でOK)")]
    [SerializeField] private TMPro.TextMeshProUGUI CharacterNamePlate;

    [Header("BGM/SE再生用ハブ")]
    [SerializeField] private SoundHub soundHub;

    [Header("BGM音量上限(クロスフェード時にこの値を超えない)")]
    [SerializeField, Range(0f, 1f)] private float bgmVolumeCap = 1f;

    public bool isEvent;

    [Header("終わった後に進むシーン")]
    [SerializeField] private string SceneName;

    [Header("早送り設定")]
    [SerializeField] private KeyCode fastForwardKey = KeyCode.LeftControl;
    [SerializeField] [Range(0f, 0.5f)] private float fastForwardTypingSpeed = 0f;

    [Header("マウスポインタ")]
    [SerializeField] private MouseControllBase mouse;

    private TextDialogueOption textOption;
    private SpriteDictionary spriteDictionary;
    private Dictionary<CharacterKey, CharacterSpriteRenderer> characterRegistry;
    private MassageExecuter mExecuter;
    private NovelAnimationManager animationManager;
    private CancellationTokenSource dialogueCts;
    private Task currentDialogueTask;
    private float defaultTypingSpeed;
    private bool isFastForwarding;


    public bool IsPlaying { get; private set; }
    public event Action OnDialogueFinished;

    private void Awake()
    {
        // commonResourceSettingsはシーンをまたいで永続するScriptableObjectなので、
        // 前のシーンで破棄されたTextMeshPro参照が残っていないよう必ずクリアする。
        commonResourceSettings?.option?.Clear();

        if (commonResourceSettings != null && commonResourceSettings.option != null)
        {
            defaultTypingSpeed = commonResourceSettings.option.typingSpeed;
        }

        // このシーン自身のテキストUIも、前回の残留テキスト・アクティブ状態を引きずらないよう初期化する。
        ResetTextUI(NarrationText);
        ResetTextUI(CharacterDialogueText);
        ResetTextUI(CharacterNamePlate);

        if (characterKeyDatabase != null)
        {
            characterKeyDatabase.BuildLookup();
        }

        spriteDictionary = new SpriteDictionary();

        if (commonResourceSettings != null
            && commonResourceSettings.option != null
            && commonResourceSettings.option.Character != null)
        {
            foreach (CharactorToList character in commonResourceSettings.option.Character)
            {
                if (character == null || character.spriteData == null)
                {
                    continue;
                }

                spriteDictionary.Allocation(character.spriteData);
            }
        }

        characterRegistry = new Dictionary<CharacterKey, CharacterSpriteRenderer>();

        CharacterSpriteRenderer[] renderers =
            FindObjectsByType<CharacterSpriteRenderer>(FindObjectsSortMode.None);

        foreach (CharacterSpriteRenderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            CharacterKey key = renderer.GetCharacterKey();

            if (key == null)
            {
                Debug.LogWarning($"{renderer.name} に CharacterKey が設定されていません");
                continue;
            }

            if (characterRegistry.ContainsKey(key))
            {
                Debug.LogWarning(
                    $"CharacterKey 重複: {key.name}\n" +
                    $"既存: {characterRegistry[key].name}\n" +
                    $"新規: {renderer.name}");
            }

            characterRegistry[key] = renderer;
        }

        // HideAllCharacters();

        mExecuter = new MassageExecuter(
            spriteDictionary,
            characterRegistry,
            characterKeyDatabase);

                    if(mouse != null)
        {
            mouse.IsControll = false;
        }
        
    }

    private static void ResetTextUI(TMPro.TextMeshProUGUI text)
    {
        if (text == null)
        {
            return;
        }

        text.text = string.Empty;
        text.gameObject.SetActive(false);
    }

    private void HideAllCharacters()
    {
        foreach (CharacterSpriteRenderer renderer in characterRegistry.Values)
        {
            if (renderer == null)
            {
                continue;
            }

            SpriteRenderer sr = renderer.GetSpriteRenderer();

            if (sr == null)
            {
                continue;
            }

            sr.enabled = false;
        }
    }

    private void Start()
    {
        animationManager = new NovelAnimationManager(
            UnityEngine.Object.FindObjectsByType<NovelAnimationObject>(FindObjectsSortMode.None),
            () => mExecuter != null && mExecuter.IsSkip);

        if(isEvent) return;
        _ = Novel();
    }

#region PlayAsync

    public async Task PlayAsync(NovelGameSettings setting)
    {
        if (setting == null)
            return;

        if (!EnsureTextOptionForPlay())
        {
            Debug.LogError("NovelEventManager: text option is not configured for Play().");
            return;
        }

        textOption.dialogueText =
            setting.dialogueType switch
            {
                DialogueType.Narration => NarrationText,
                DialogueType.Talk => CharacterDialogueText,
                _ => null
            };

        if (textOption.dialogueText == null)
        {
            // Debug.LogError($"想定外の会話タイプ : {setting.dialogueType}");
            return;
        }

        textOption.dialogueText.color = setting.textColor;

        Debug.Log($"[ColorDebug] PlayAsync asset={setting.name} type={setting.dialogueType} " +
            $"targetObj={textOption.dialogueText.name}(id={textOption.dialogueText.GetInstanceID()}) " +
            $"wantColor={setting.textColor} appliedColor={textOption.dialogueText.color}");

        IsPlaying = true;

        List<DialogueData> dialogueData =
            DialogueCSVLoader.LoadCSV(
                setting.DialogueCSV,
                characterKeyDatabase);

        dialogueCts = new CancellationTokenSource();
        currentDialogueTask = mExecuter.StartDialogueAsync(
            textOption,
            dialogueData,
            dialogueCts.Token);
        await currentDialogueTask;

        IsPlaying = false;

        OnDialogueFinished?.Invoke();
        OnDialogueFinished = null;
    }

    private async Task PlayAsync(TextAsset csv)
    {
        if (csv == null)
            return;

        if (!EnsureTextOptionForPlay())
            return;

        IsPlaying = true;

        var dialogueData =
            DialogueCSVLoader.LoadCSV(
                csv,
                characterKeyDatabase);

        dialogueCts = new CancellationTokenSource();
        currentDialogueTask = mExecuter.StartDialogueAsync(
            textOption,
            dialogueData,
            dialogueCts.Token);
        await currentDialogueTask;

        IsPlaying = false;

        OnDialogueFinished?.Invoke();
        OnDialogueFinished = null;
    }

#endregion

#region Play

    public void Play(NovelGameSettings setting, Action finishAction)
    {
        OnDialogueFinished += finishAction;
        _ = PlayAsync(setting);
    }

    public void Play(TextAsset csv, Action finishAction)
    {
        OnDialogueFinished += finishAction;
        _ = PlayAsync(csv);
    }
#endregion

    private bool EnsureTextOptionForPlay()
    {
        if (commonResourceSettings == null || commonResourceSettings.option == null)
        {
            return false;
        }

        textOption = commonResourceSettings.option;
        textOption.CharacterText = CharacterNamePlate;

        if (textOption.dialogueText == null)
        {
            textOption.dialogueText = CharacterDialogueText != null ? CharacterDialogueText : NarrationText;
        }

        return textOption.dialogueText != null;
    }

#region 会話ループ
    private async Task Novel()
    {
        if(mouse != null)
        {
            mouse.IsControll = false;
        }
        
        foreach (NovelGameData setting in novelGameSettings)
        {
            
            Debug.Log("Run Text Loop");
            if (setting is NovelGameSettings csv)
                await PlayAsync(csv);
            if (setting is NovelAnimationSettings anim)
                await animationManager.PlayAnimation(anim);
            if (setting is SoundData sound)
                soundHub.Play(sound, bgmVolumeCap);
        }

        if(mouse != null)
        {
            mouse.IsControll = true;
        }


        if (string.IsNullOrEmpty(SceneName))
        {
           // Debug.LogError("SceneName が設定されていません。");
            return;
        }

        int buildIndex = UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath(SceneName);
        if (buildIndex < 0)
        {
            Debug.LogError($"シーン '{SceneName}' が Build Settings に登録されていません。");
            return;
        }


        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
    }

#endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _ = SkipToSceneAsync();
        }

        HandleFastForward();
    }

    private void HandleFastForward()
    {
        if (commonResourceSettings == null || commonResourceSettings.option == null)
        {
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Tutorial") return;

        bool holding = Input.GetKey(fastForwardKey);

        mExecuter?.SetSkip(holding);

        if (holding && !isFastForwarding)
        {
            isFastForwarding = true;
            commonResourceSettings.option.typingSpeed = fastForwardTypingSpeed;
        }
        else if (!holding && isFastForwarding)
        {
            isFastForwarding = false;
            commonResourceSettings.option.typingSpeed = defaultTypingSpeed;
        }
    }

    private void OnDisable()
    {
        if (isFastForwarding && commonResourceSettings != null && commonResourceSettings.option != null)
        {
            commonResourceSettings.option.typingSpeed = defaultTypingSpeed;
        }

        isFastForwarding = false;
        mExecuter?.SetSkip(false);
    }

    private async Task SkipToSceneAsync()
    {
        // 実行中の会話コルーチンにキャンセルを伝え、実際に止まるまで待ってから遷移する。
        // これをせずにLoadSceneだけ呼ぶと、裏で会話ループがゾンビ状態のまま動き続け、
        // 新しいシーンのテキストに書き込みが競合して文字が割り込むことがある。
        dialogueCts?.Cancel();

        if (currentDialogueTask != null)
        {
            try
            {
                await currentDialogueTask;
            }
            catch
            {
                // キャンセルによる例外は無視する。
            }
        }
        

        // 次のシーンに破棄済みのTextMeshPro参照などを持ち越さないよう、共有データを解放してから遷移する。
        commonResourceSettings?.option?.Clear();
        IsPlaying = false;
        OnDialogueFinished = null;

    }

}