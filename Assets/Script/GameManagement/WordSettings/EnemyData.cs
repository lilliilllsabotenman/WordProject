using UnityEngine;

[CreateAssetMenu(menuName = "Sentence/SentenceTemplate")]
public class SentenceTemplate : ScriptableObject
{
    public System.Collections.Generic.List<SentenceElement> elements = new();
}
