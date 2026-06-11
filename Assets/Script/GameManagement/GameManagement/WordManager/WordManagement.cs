using System.Collections.Generic;
using System.Linq;

public class WordManager
{
    private readonly List<WordObject> wordObjects;
    private readonly List<WordAsset> wordPool;

    private List<WordAsset> answerWords = new();

    public WordManager(List<WordObject> objects, List<WordAsset> wordPool)
    {
        wordObjects = objects;
        this.wordPool = wordPool;
    }

    public void Initialize(SentenceTemplate template)
    {
        answerWords = template == null
            ? new List<WordAsset>()
            : template.elements
                .Where(e => e.IsValidInputSlot())
                .Select(e => e.GetSlotWord())
                .Where(w => w != null)
                .ToList();

        ActivateObjects();
        AssignWordAssets();
    }

    private void ActivateObjects()
    {
        foreach (var obj in wordObjects)
        {
            obj.gameObject.SetActive(true);
        }
    }

    private void AssignWordAssets()
    {
        List<WordAsset> spawnWords = new();
        spawnWords.AddRange(answerWords);

        int remain = wordObjects.Count - spawnWords.Count;

        List<WordAsset> shuffledPool = new(wordPool);
        Shuffle(shuffledPool);

        for (int i = 0; i < remain; i++)
        {
            if (i >= shuffledPool.Count)
            {
                break;
            }

            spawnWords.Add(shuffledPool[i]);
        }

        Shuffle(spawnWords);

        for (int i = 0; i < wordObjects.Count; i++)
        {
            if (i >= spawnWords.Count)
            {
                break;
            }

            wordObjects[i].SetWordAsset(spawnWords[i]);
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }
}
