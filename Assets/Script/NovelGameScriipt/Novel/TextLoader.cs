using UnityEngine;
using TMPro;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace NovelGameDialogue
{
    public enum DialogueType
    {
        Talk,
        Narration
    }

    // Dictionary for resolving sprites by CharacterKey or CharacterId.
    public class SpriteDictionary
    {
        public Dictionary<CharacterKey, Dictionary<string, Sprite>> CharacterSpriteByKey =
            new Dictionary<CharacterKey, Dictionary<string, Sprite>>();

        public Dictionary<string, Dictionary<string, Sprite>> CharacterSpriteById =
            new Dictionary<string, Dictionary<string, Sprite>>(StringComparer.OrdinalIgnoreCase);

        private static string NormalizeKey(string key)
        {
            return string.IsNullOrWhiteSpace(key) ? string.Empty : key.Trim();
        }

        public void Allocation(List<SpriteData> data)
        {
            foreach (SpriteData sp in data)
            {
                if (sp == null || sp.CharacterKey == null)
                {
                    continue;
                }

                string characterId = NormalizeKey(sp.CharacterKey.CharacterId);
                string stateKey = NormalizeKey(sp.KeyEnum);

                if (stateKey.Length == 0 || sp.CharactorSprite == null)
                {
                    continue;
                }

                if (!CharacterSpriteByKey.TryGetValue(sp.CharacterKey, out var keyDict))
                {
                    keyDict = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
                    CharacterSpriteByKey[sp.CharacterKey] = keyDict;
                }
                keyDict[stateKey] = sp.CharactorSprite;

                if (characterId.Length > 0)
                {
                    if (!CharacterSpriteById.TryGetValue(characterId, out var idDict))
                    {
                        idDict = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
                        CharacterSpriteById[characterId] = idDict;
                    }
                    idDict[stateKey] = sp.CharactorSprite;
                }
            }
        }
    }

    [System.Serializable]
    public class TextDialogueOption
    {
        [NonSerialized] public TextMeshProUGUI CharacterText;
        [NonSerialized] public TextMeshProUGUI dialogueText;

        [Header("テキストの更新スピード")]
        [Range(0, 0.5f)] public float typingSpeed;

        [Header("キャラクターのデータ")]
        public List<CharactorToList> Character;

        // シーン間で永続するため、遷移前に破棄済みの参照を持ち越さないようクリアする。
        public void Clear()
        {
            CharacterText = null;
            dialogueText = null;
        }
    }

    public static class DialogueDataValidator
    {
        private static bool IsNarrationMarker(string characterId, string stateKey)
        {
            if (stateKey.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return characterId.Equals("narration", StringComparison.OrdinalIgnoreCase)
                || characterId.Equals("naration", StringComparison.OrdinalIgnoreCase);
        }

        public static void Validate(List<DialogueData> dialogue, SpriteDictionary spriteDict)
        {
            foreach (var d in dialogue)
            {
                string characterId = string.IsNullOrWhiteSpace(d.CharacterId) ? string.Empty : d.CharacterId.Trim();
                string stateKey = string.IsNullOrWhiteSpace(d.State) ? string.Empty : d.State.Trim();

                // Narration lines can legitimately omit sprite resolution.
                if (characterId.Length == 0 || stateKey.Length == 0 || IsNarrationMarker(characterId, stateKey))
                {
                    continue;
                }

                bool found = false;

                if (d.CharacterKey != null
                    && spriteDict.CharacterSpriteByKey.TryGetValue(d.CharacterKey, out var keyDict)
                    && keyDict.ContainsKey(stateKey))
                {
                    found = true;
                }

                if (!found
                    && characterId.Length > 0
                    && spriteDict.CharacterSpriteById.TryGetValue(characterId, out var idDict)
                    && idDict.ContainsKey(stateKey))
                {
                    found = true;
                }

                if (!found)
                {
                    Debug.LogWarning($"Sprite not found: {d.CharacterId} / {d.State}");
                }
            }
        }
    }

    public class MassageExecuter
    {
        private static bool IsNarrationMarker(string characterId, string stateKey)
        {
            if (stateKey.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return characterId.Equals("narration", StringComparison.OrdinalIgnoreCase)
                || characterId.Equals("naration", StringComparison.OrdinalIgnoreCase);
        }

        private readonly SpriteDictionary characterSprite;
        private readonly Dictionary<CharacterKey, CharacterSpriteRenderer> characterRegistry;
        private readonly Dictionary<string, CharacterSpriteRenderer> characterRegistryById;
        private readonly CharacterKeyDatabase characterKeyDatabase;

        // 早送り中かどうかをNovelEventManager側からSetSkipで明示的に制御する。
        public bool IsSkip { get; private set; }

        public void SetSkip(bool isSkip)
        {
            IsSkip = isSkip;
        }

        public MassageExecuter(
            SpriteDictionary spriteDictionary,
            Dictionary<CharacterKey, CharacterSpriteRenderer> registry,
            CharacterKeyDatabase characterKeyDatabase = null)
        {
            characterSprite = spriteDictionary;
            characterRegistry = registry;
            characterRegistryById = BuildCharacterRegistryById(registry);
            this.characterKeyDatabase = characterKeyDatabase;
        }

        private static Dictionary<string, CharacterSpriteRenderer> BuildCharacterRegistryById(
            Dictionary<CharacterKey, CharacterSpriteRenderer> registry)
        {
            var byId = new Dictionary<string, CharacterSpriteRenderer>(StringComparer.OrdinalIgnoreCase);
            if (registry == null)
            {
                return byId;
            }

            foreach (var pair in registry)
            {
                CharacterKey key = pair.Key;
                CharacterSpriteRenderer renderer = pair.Value;
                if (key == null || renderer == null)
                {
                    continue;
                }

                string characterId = string.IsNullOrWhiteSpace(key.CharacterId) ? string.Empty : key.CharacterId.Trim();
                if (characterId.Length == 0)
                {
                    continue;
                }

                byId[characterId] = renderer;
            }

            return byId;
        }

        public void StartDialogue(TextDialogueOption optionData, List<DialogueData> gameData)
        {
            _ = StartDialogueAsync(optionData, gameData);
        }

        public async Task StartDialogueAsync(TextDialogueOption optionData, List<DialogueData> gameData, CancellationToken cancellationToken = default)
        {
            DialogueDataValidator.Validate(gameData, characterSprite);
            await ShowDialogueSequenceAsync(optionData, gameData, cancellationToken);
        }

        public async Task ShowDialogueSequenceAsync(TextDialogueOption optionData, List<DialogueData> data, CancellationToken cancellationToken = default)
        {
            foreach (DialogueData d in data)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                string characterId = string.IsNullOrWhiteSpace(d.CharacterId) ? string.Empty : d.CharacterId.Trim();
                string stateKey = string.IsNullOrWhiteSpace(d.State) ? string.Empty : d.State.Trim();
                bool requiresSpriteUpdate =
                    characterId.Length > 0 &&
                    stateKey.Length > 0 &&
                    !IsNarrationMarker(characterId, stateKey);

                CharacterSpriteRenderer character = null;
                if (requiresSpriteUpdate
                    && d.CharacterKey != null
                    && characterRegistry.TryGetValue(d.CharacterKey, out var rendererByKey)
                    && rendererByKey != null)
                {
                    character = rendererByKey;
                }

                if (requiresSpriteUpdate
                    && character == null
                    && characterRegistryById.TryGetValue(characterId, out var rendererById)
                    && rendererById != null)
                {
                    character = rendererById;
                }

                Sprite resolvedSprite = null;
                if (requiresSpriteUpdate
                    && d.CharacterKey != null
                    && characterSprite.CharacterSpriteByKey.TryGetValue(d.CharacterKey, out var keyDict)
                    && TryResolveSprite(keyDict, stateKey, characterId, out var spriteByKey))
                {
                    resolvedSprite = spriteByKey;
                }
                else if (requiresSpriteUpdate
                    && characterSprite.CharacterSpriteById.TryGetValue(characterId, out var idDict)
                    && TryResolveSprite(idDict, stateKey, characterId, out var spriteById))
                {
                    resolvedSprite = spriteById;
                }

                if (requiresSpriteUpdate && character != null && resolvedSprite != null)
                {
                    character.Visible = true;
                    character.Sprite = resolvedSprite;
                }
                else if (requiresSpriteUpdate)
                {
                    Debug.LogWarning($"Sprite update skipped: id={d.CharacterId}, state={d.State}, renderer={(character != null)}, sprite={(resolvedSprite != null)}");
                }

                if (optionData.dialogueText != null && !optionData.dialogueText.gameObject.activeInHierarchy)
                {
                    optionData.dialogueText.gameObject.SetActive(true);


                    Transform parent = optionData.dialogueText.transform.parent;

                    if (parent != null)
                    {
                        parent.gameObject.SetActive(true);
                    }
                }

                if (optionData.CharacterText != null && !optionData.CharacterText.gameObject.activeSelf)
                {
                    optionData.CharacterText.gameObject.SetActive(true);
                }

                string line = d.Dialogue;

                if (optionData.dialogueText != null)
                {
                    optionData.dialogueText.text = string.Empty;
                    Debug.Log($"[ColorDebug] ShowLine obj={optionData.dialogueText.name}(id={optionData.dialogueText.GetInstanceID()}) " +
                        $"active={optionData.dialogueText.gameObject.activeInHierarchy} colorNow={optionData.dialogueText.color}");
                }

                if (optionData.CharacterText != null)
                {
                    optionData.CharacterText.text = ResolveDisplayName(d.CharacterKey, characterId, d.CharacterId);
                }

                foreach (char c in line)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (optionData.dialogueText != null)
                    {
                        optionData.dialogueText.text += c;
                    }

                    await WaitForSecondsScaledAsync(optionData.typingSpeed);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await Task.Yield();

                if (!IsSkip)
                {
                    await WaitUntilAsync(() =>
                        Input.GetMouseButton(0)
                        || IsSkip
                        || cancellationToken.IsCancellationRequested);
                    await WaitUntilAsync(() => !Input.GetMouseButton(0) || cancellationToken.IsCancellationRequested);
                }

                if (character != null)
                {
                    character.Sprite = null;
                    character.Visible = false;


                }
            }

            if (optionData.dialogueText != null)
            {
                optionData.dialogueText.text = string.Empty;
                optionData.dialogueText.gameObject.SetActive(false);
              
                Transform parent = optionData.dialogueText.transform.parent;    
                Canvas  canvas = parent.gameObject.GetComponent<Canvas>();

                if(parent != null && canvas == null)
                {
                    parent.gameObject.SetActive(false);
                }
            }

            if (optionData.CharacterText != null)
            {
                optionData.CharacterText.text = string.Empty;
                optionData.CharacterText.gameObject.SetActive(false);
            }

            TextFinished();
        }

        // CharacterKey(スプライト解決結果)が無い/名前未設定でも、データベースから直接名前を引く。
        private string ResolveDisplayName(CharacterKey key, string characterId, string fallbackId)
        {
            if (key != null && !string.IsNullOrWhiteSpace(key.DisplayName))
            {
                return key.DisplayName;
            }

            if (characterKeyDatabase != null)
            {
                CharacterKey fromDatabase = characterKeyDatabase.Get(characterId);
                if (fromDatabase != null && !string.IsNullOrWhiteSpace(fromDatabase.DisplayName))
                {
                    return fromDatabase.DisplayName;
                }
            }

            return fallbackId;
        }

        private static bool TryResolveSprite(
            Dictionary<string, Sprite> spriteMap,
            string stateKey,
            string characterId,
            out Sprite sprite)
        {
            sprite = null;
            if (spriteMap == null || spriteMap.Count == 0)
            {
                return false;
            }

            if (spriteMap.TryGetValue(stateKey, out sprite))
            {
                return true;
            }

            // Compatibility fallback for existing data where KeyEnum is characterId.
            if (!string.IsNullOrWhiteSpace(characterId) && spriteMap.TryGetValue(characterId, out sprite))
            {
                return true;
            }

            // Common default aliases.
            if (spriteMap.TryGetValue("normal", out sprite))
            {
                return true;
            }

            if (spriteMap.TryGetValue("default", out sprite))
            {
                return true;
            }

            // Last resort: if only one sprite exists, use it.
            if (spriteMap.Count == 1)
            {
                foreach (var pair in spriteMap)
                {
                    sprite = pair.Value;
                    return sprite != null;
                }
            }

            return false;
        }

        private static async Task WaitForSecondsScaledAsync(float seconds)
        {
            if (seconds <= 0f)
            {
                await Task.Yield();
                return;
            }

            float elapsed = 0f;
            while (elapsed < seconds)
            {
                await Task.Yield();
                elapsed += Time.deltaTime;
            }
        }

        public static async Task WaitUntilAsync(Func<bool> predicate)
        {
            while (!predicate())
            {
                await Task.Yield();
            }
        }

        public virtual void TextFinished()
        {
        }
    }
}