using UnityEngine;
using System;
using System.Collections.Generic;
using NovelGameDialogue;
using System.Threading.Tasks;
using MyTween;

public class NovelEventManager : MonoBehaviour
{
    [Header("会話データ設定")]
    [SerializeField] private List<NovelGameSettings> novelGameSettings;

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
        if(isEvent) return;
        _ = Novel();
    }

    private async Task Novel()//Todoこれをループにまとめられると思うのでそうしろ！！演出含め！！これ書いたあほ間抜けは重罪
    {
        // for (int i = 0; i < novelGameSettings.Count; i++)
        // {
        //     if (novelGameSettings[i] == null)
        //         return;

        //     if (commonResourceSettings == null || commonResourceSettings.option == null)
        //         return;

        //     Debug.Log("NovelGameData is Found");

        //     DialogueCSV = novelGameSettings[i].DialogueCSV;
        //     textOption = commonResourceSettings.option;

        //     textOption.CharacterText = CharacterNamePlate;

        //     if (novelGameSettings[i].dialogueType == DialogueType.Narration) textOption.dialogueText = NarrationText;
        //     else if (novelGameSettings[i].dialogueType == DialogueType.Talk) textOption.dialogueText = CharacterDialogueText;
        //     else Debug.LogError("想定外の会話タイプが設定されています");

        //     await PlayAsync(DialogueCSV);

        //     Debug.Log("EndTask");
        // }

        

        //処理１

        //処理２

        // 処理1 : 会話

        if (novelGameSettings[0] != null)
        {
            DialogueCSV = novelGameSettings[0].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[0].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[0].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);


        }

        SpriteRenderer spriteRenderer = GameObject.FindWithTag("FIrestFeedObject").GetComponent<SpriteRenderer>();
        Debug.Log("Feed");
            
                    
        while (true)
        {
            Color color = spriteRenderer.color;

            Debug.Log("Feed");
            color.a -= color.a / 15;

            if (color.a <= 0.01f)
            {
                color.a = 0f;
                spriteRenderer.color = color;
                break;
            }

            spriteRenderer.color = color;

            await Task.Delay(10);
        }


        // 処理2 : アニメーション
        // await PlayAnimationAsync(animationSettings[0]);

        // 処理3 : 会話
        if (novelGameSettings[1] != null)
        {
            DialogueCSV = novelGameSettings[1].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[1].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[1].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
        
#region Naration2

        spriteRenderer.color = new Vector4(0, 0, 0, 0);//ごり押し

        while (true)
        {
            Color color = spriteRenderer.color;

            Debug.Log("Feed");
            color.a += (1f - color.a) / 15f;

            if (color.a >= 0.99f)
            {
                color.a = 1f;
                spriteRenderer.color = color;
                break;
            }

            spriteRenderer.color = color;

            await Task.Delay(10);
        }

        while (true)
        {
            Color color = spriteRenderer.color;

            Debug.Log("Feed");
            color.a -= color.a / 15;

            if (color.a <= 0.01f)
            {
                color.a = 0f;
                spriteRenderer.color = color;
                break;
            }

            spriteRenderer.color = color;

            await Task.Delay(10);
        }

        // NarrationText.color = Color.white;

        if (novelGameSettings[2] != null)
        {
            DialogueCSV = novelGameSettings[2].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[2].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[2].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion

#region Talk3

        if (novelGameSettings[3] != null)
        {
            DialogueCSV = novelGameSettings[3].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[3].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[3].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion



#region Talk4

        if (novelGameSettings[4] != null)
        {
            DialogueCSV = novelGameSettings[4].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[4].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[4].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion

#region Naration5

        if (novelGameSettings[5] != null)
        {
            DialogueCSV = novelGameSettings[5].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[5].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[5].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion

#region Talk6

        if (novelGameSettings[6] != null)
        {
            DialogueCSV = novelGameSettings[6].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[6].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[6].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion

    Color c = spriteRenderer.color;
    c.a = 1f;
    spriteRenderer.color = c;

    await Task.Delay(250);

    c.a = 0f;
    spriteRenderer.color = c;


#region Talk7

        if (novelGameSettings[7] != null)
        {
            DialogueCSV = novelGameSettings[7].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[7].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[7].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion

        NarrationText.color = Color.white;
        CharacterDialogueText.color = Color.white;

        while (true)
        {
            Color color = spriteRenderer.color;

            Debug.Log("Feed");
            color.a += (1f - color.a) / 15f;

            if (color.a >= 0.99f)
            {
                color.a = 1f;
                spriteRenderer.color = color;
                break;
            }

            spriteRenderer.color = color;

            await Task.Delay(10);
        }

#region Talk8

        if (novelGameSettings[8] != null)
        {
            DialogueCSV = novelGameSettings[8].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[8].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[8].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion

#region Talk9

        if (novelGameSettings[9] != null)
        {
            DialogueCSV = novelGameSettings[9].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[9].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[9].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion

#region Talk10

        if (novelGameSettings[10] != null)
        {
            DialogueCSV = novelGameSettings[10].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[10].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[10].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion

#region Talk11

        if (novelGameSettings[11] != null)
        {
            DialogueCSV = novelGameSettings[11].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[11].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[11].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion

#region Talk12

        if (novelGameSettings[12] != null)
        {
            DialogueCSV = novelGameSettings[12].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[12].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[12].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion

            spriteRenderer.color = Color.white;
            await Task.Delay(250);

            spriteRenderer.color = Color.black;
            await Task.Delay(250);
        
#region Talk13

        if (novelGameSettings[13] != null)
        {
            DialogueCSV = novelGameSettings[13].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[13].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[13].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion
#region Talk14

        if (novelGameSettings[14] != null)
        {
            DialogueCSV = novelGameSettings[14].DialogueCSV;
            textOption = commonResourceSettings.option;

            textOption.CharacterText = CharacterNamePlate;

            if (novelGameSettings[14].dialogueType == DialogueType.Narration)
                textOption.dialogueText = NarrationText;
            else if (novelGameSettings[14].dialogueType == DialogueType.Talk)
                textOption.dialogueText = CharacterDialogueText;

            await PlayAsync(DialogueCSV);
        }
#endregion

        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleGameScene");
    }

    private async Task PlayAsync(TextAsset csv)
    {   
        if (csv == null)
            return;

        if (!EnsureTextOptionForPlay())
        {
            Debug.LogError("NovelEventManager: text option is not configured for Play().");
            return;
        }

        IsPlaying = true;

        List<DialogueData> dialogueData = DialogueCSVLoader.LoadCSV(csv, characterKeyDatabase);

        await mExecuter.StartDialogueAsync(
            textOption,
            dialogueData);

        IsPlaying = false;

        OnDialogueFinished?.Invoke();
        OnDialogueFinished = null;
    }

    public void Play(TextAsset csv, Action finishAction)
    {
        // Debug.Log(csv);//何が呼び出されているか
        OnDialogueFinished += finishAction;
        _ = PlayAsync(csv);
    }

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

    public async Task PlayAnimationAsync(AnimationActionData actionData)
    {
        if (novelScheduler == null || actionData == null)
        {
            return;
        }

        await novelScheduler.ExecuteAnimationAsync(actionData);
    }
}
