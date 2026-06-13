using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Sentence/SentenceTemplate")]
public class SentenceTemplate : ScriptableObject
{
    public List<SentenceElement> elements = new();

    public List<WordAsset> GetWordAssetsList()
    {
        List<WordAsset> result = new();

        foreach(SentenceElement element in elements)
        {
            if(element.type == SentenceElementType.Slot)
            {
                if(element.slotDefinition == null) Debug.LogError("使用されているSentenceTextに型と内容の不整合があります。");
                
                result.Add(element.slotDefinition);
            } 

        }
        
        return result;
    }
}
