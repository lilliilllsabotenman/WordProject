using UnityEngine;
using System.Collections.Generic;
using System;

namespace NovelGameDialogue
{
    [CreateAssetMenu(menuName = "Novel/Character Key Database")]
    public class CharacterKeyDatabase : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public string characterId;
            public CharacterKey characterKey;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        private Dictionary<string, CharacterKey> lookup;

        public void BuildLookup()
        {
            lookup = new Dictionary<string, CharacterKey>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.characterId) || entry.characterKey == null)
                {
                    continue;
                }

                string normalizedId = entry.characterId.Trim();
                lookup[normalizedId] = entry.characterKey;
            }
        }

        public CharacterKey Get(string characterId)
        {
            if (lookup == null)
            {
                BuildLookup();
            }

            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            lookup.TryGetValue(characterId.Trim(), out var key);
            return key;
        }
    }
}
