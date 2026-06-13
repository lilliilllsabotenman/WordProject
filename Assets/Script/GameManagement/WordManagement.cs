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

        UnityEngine.Debug.Log(wordPool.Count);
    }

    public void Initialize(SentenceTemplate template)
    {
        Initialize(template, false);
    }

    public void Initialize(SentenceTemplate template, bool allowDuplicate)
    {
        answerWords = template.GetWordAssetsList();

        if (answerWords.Count > wordObjects.Count)
        {
            UnityEngine.Debug.LogError("回答数がWordObject数を超えています");
            return;
        }

        ActivateObjects();
        AssignWordAssets(allowDuplicate);
    }

    private void ActivateObjects()
    {
        foreach (var obj in wordObjects)
        {
            obj.gameObject.SetActive(true);
        }
    }

    private void AssignWordAssets(bool allowDuplicate)
    {
        List<WordAsset> spawnWords = new();
        spawnWords.AddRange(answerWords);

        int remain = wordObjects.Count - spawnWords.Count;

        List<WordAsset> shuffledPool = allowDuplicate
            ? new List<WordAsset>(wordPool)
            : new List<WordAsset>(wordPool.Where(x => !answerWords.Contains(x)));

        Shuffle(shuffledPool);

        for (int i = 0; i < remain && i < shuffledPool.Count; i++)
        {
            spawnWords.Add(shuffledPool[i]);
        }

        Shuffle(spawnWords);

        UnityEngine.Debug.Log($"Spawn Words Count : {spawnWords.Count}");

        for (int i = 0; i < wordObjects.Count && i < spawnWords.Count; i++)
        {
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