using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
 
public static class DialogueCharacterEnumerator
{
    private const string OutputPath = "Assets/Editor/DialogueCharacters.txt";
 
    [MenuItem("Tools/Dialogue/Enumerate Used Characters")]
    public static void Enumerate()
    {
        var characters = new SortedSet<char>();
 
        foreach (var csvPath in Directory.EnumerateFiles("Assets/Scenes", "*.csv", SearchOption.AllDirectories))
        {
            string text = File.ReadAllText(csvPath, Encoding.UTF8);
 
            foreach (char c in text)
            {
                if (char.IsControl(c)) continue;
                characters.Add(c);
            }
        }
 
        string result = new string(characters.ToArray());
        File.WriteAllText(OutputPath, result, new UTF8Encoding(false));
        AssetDatabase.Refresh();
 
        EditorGUIUtility.systemCopyBuffer = result;
 
        Debug.Log($"[DialogueCharacterEnumerator] CSVダイアログ全体から {characters.Count} 種類の文字を検出し、{OutputPath} に書き出しました。クリップボードにもコピー済みです。Font Asset Creatorの「Character Sequence (Hex)」または「Custom Characters」欄に貼り付けてください。");
    }
}