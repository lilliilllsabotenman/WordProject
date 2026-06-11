using UnityEngine;
using System.Collections.Generic;

namespace NovelGameDialogue
{
    [System.Serializable]
    public class SpriteData
    {
        public CharacterKey CharacterKey;
        public string KeyEnum;
        public Sprite CharactorSprite;
    }

    [System.Serializable]
    public class DialogueData
    {
        public CharacterKey CharacterKey;
        public string CharacterId;
        public string State;
        public string Dialogue;
    }

    [System.Serializable]
    public class CharactorToList
    {
        [Header("CharacterÇÃSpriteRenderer")]
        public CharacterKey sr;

        [Header("SpriteÇÃÉfÅ[É^")]
        public List<SpriteData> spriteData;
    }

    [System.Serializable]
    public class DialogueCSVLoader
    {
        public static List<DialogueData> LoadCSV(TextAsset csvFile, CharacterKeyDatabase characterKeyDatabase)
        {
            List<DialogueData> result = new List<DialogueData>();

            string[] lines = csvFile.text.Split('\n');

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] cols = line.Split(',');
                if (cols.Length < 3) continue;

                DialogueData data = new DialogueData();

                data.CharacterId = cols[0].Trim();
                data.State = cols[1].Trim();
                data.Dialogue = cols[2].Trim();
                data.CharacterKey = characterKeyDatabase != null
                    ? characterKeyDatabase.Get(data.CharacterId)
                    : null;

                result.Add(data);
            }

            return result;
        }
    }
}
