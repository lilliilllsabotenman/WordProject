using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ViewUIText
{
    private TextMeshProUGUI Text;
    private SentenceTemplate template;
    private List<WordAsset> words = new();

    public ViewUIText(TextMeshProUGUI Text)
    {
        this.Text = Text;
    }

    public void TextInitialize(SentenceTemplate template)
    {
        this.template = template;
        words.Clear();
        
        if(Text == null) Debug.LogError("ViewText : TextMeshProの参照がありません。Inspectorを確認してください");

        UpdateText();
    }

    private void UpdateText()
    {
        string viewText = "";

        int SlotCount = 0;

        foreach(SentenceElement e in template.elements)
        {
            switch(e.type)
            {
                case SentenceElementType.FixedText:
                    viewText += e.fixedText;
                    break;
                case SentenceElementType.Slot:
                        
                    if(words.Count > SlotCount) 
                    {    
                        viewText += words[SlotCount].DisplayText;
                        SlotCount ++;
                    }
                    else viewText += "■"; 
                    break;
            }             
        }
        Debug.Log(viewText);


        Text.text = viewText;
    }

    public void AddWord(WordAsset word)
    {
        words.Add(word);
        UpdateText();
    }
}
