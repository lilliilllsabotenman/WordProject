using UnityEngine;
using System;
using System.Collections.Generic;
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


    public bool isEvent;

    private TextAsset DialogueCSV;
    private TextDialogueOption textOption;
    private SpriteDictionary spriteDictionary;
    private Dictionary<CharacterKey, CharacterSpriteRenderer> characterRegistry;
    private MassageExecuter mExecuter;
    private NovelScheduler novelScheduler;
    private NovelAnimationManager animationManager;

    
    public bool IsPlaying { get; private set; }
    public event Action OnDialogueFinished;

   private void Awake()
    {
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
            characterRegistry);

        novelScheduler = new NovelScheduler(
            new CharacterAnimationTargetResolver(characterRegistry),
            new AnimationConverter(),
            new RuntimeCommandConverter(),
            new AnimationExecuter());
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
            UnityEngine.Object.FindObjectsByType<NovelAnimationObject>(FindObjectsSortMode.None));

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

        IsPlaying = true;

        List<DialogueData> dialogueData =
            DialogueCSVLoader.LoadCSV(
                setting.DialogueCSV,
                characterKeyDatabase);

        await mExecuter.StartDialogueAsync(
            textOption,
            dialogueData);

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

        await mExecuter.StartDialogueAsync(
            textOption,
            dialogueData);

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
        foreach (NovelGameData setting in novelGameSettings)
        {
            if(setting is NovelGameSettings csv)
                await PlayAsync(csv);
            if(setting is NovelAnimationSettings anim)
                await animationManager.PlayAnimation(anim);
                
        }

        // UnityEngine.SceneManagement.SceneManager.LoadScene("SampleGameScene");
    }

#endregion

}