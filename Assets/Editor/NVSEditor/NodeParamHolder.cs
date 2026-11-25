// Editor/NodeParamHolder.cs
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NVSEditor
{
    // PropertyFieldが描画するための保存データの窓口(SerializedObjectはUnityEngine.Objectにしか作れないため)。
    // ノード1個につき1つ、メモリ上だけに作る。アセットとしては保存しない。
    internal class NodeParamHolder : ScriptableObject
    {
        public List<NodeParamEntry> Params = new();

        // 箱の中身ごと複製する。
        // AnimationCurveなど参照型の値を、グラフのアセットとエディタのノードで共有しないため。
        public static List<NodeParamEntry> Clone(List<NodeParamEntry> source)
        {
            if (source == null) return new List<NodeParamEntry>();

            NodeParamHolder from = CreateInstance<NodeParamHolder>();
            NodeParamHolder to = CreateInstance<NodeParamHolder>();
            try
            {
                from.Params = source;
                EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(from), to);
                return to.Params;
            }
            finally
            {
                DestroyImmediate(from);
                DestroyImmediate(to);
            }
        }
    }
}
