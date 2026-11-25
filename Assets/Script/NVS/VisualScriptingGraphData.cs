// Script/Animation/VisualScriptingGraphData.cs
using System;
using System.Collections.Generic;
using UnityEngine;

// 引数1個分の値を、型を持ったまま入れておく箱。
// 実体はParamValue<T>で、NodeParamEntryから[SerializeReference]で参照される。
[Serializable]
public abstract class ParamValue
{
    public abstract Type ValueType { get; }
    public abstract object Get();
}

[Serializable]
public class ParamValue<T> : ParamValue
{
    public T value;

    public override Type ValueType => typeof(T);
    public override object Get() => value;
}

[Serializable]
public class NodeParamEntry
{
    public string Key;
    [SerializeReference] public ParamValue Value;
}

// ノード種別ごとのデータ(StartNodeData/ActionNodeData/ConditionNodeData)の共通基底。
// Params は汎用のキー・値ストア。メソッド引数など、種別固有フィールドで表現しきれない
// 追加データをここに載せる。
[Serializable]
public abstract class BaseNodeData
{
    public string Guid;
    public string Title;
    public Rect Position;

    public List<NodeParamEntry> Params = new();
}

[Serializable]
public class StartNodeData : BaseNodeData
{
}

[Serializable]
public class ActionNodeData : BaseNodeData
{
    // NodeExecutor.InvokeActionで呼び出すメソッド名
    public string MethodKey;
}

[Serializable]
public class ConditionNodeData : BaseNodeData
{
    // NodeExecutor.InvokeConditionで呼び出すメソッド名
    public string MethodKey;
}

[Serializable]
public class GetMemberNodeData : BaseNodeData
{
    // 参照するメンバ変数名(VisualScriptingGraphData.Membersのキー)
    public string MemberName;
}

[Serializable]
public class SetMemberNodeData : BaseNodeData
{
    // 書き込み先のメンバ変数名(VisualScriptingGraphData.Membersのキー)
    public string MemberName;
}

[Serializable]
public class EdgeData
{
    public string OutputNodeGuid;
    public string OutputPortName;
    public string InputNodeGuid;
    public string InputPortName;
}

public enum MemberKind
{
    Component,
    Value
}

// グラフが持つメンバ変数の宣言(Variablesパネルで管理する想定)。
// 型(Component型/プリミティブ型)はSystem.Typeを直接シリアライズできないため、AssemblyQualifiedNameで保持する。
[Serializable]
public class MemberVariableData
{
    public string Name;
    public MemberKind Kind;
    public string TypeName;
    public string DefaultValue; // Kind == Value の場合のみ使用
}

[CreateAssetMenu(fileName = "GraphData", menuName = "VS/Graph Data")]
public class VisualScriptingGraphData : ScriptableObject
{
    [SerializeReference]
    public List<BaseNodeData> Nodes = new();
    public List<EdgeData> Edges = new();
    public List<MemberVariableData> Members = new();

    // このグラフがAction/Condition/Getterの候補を探す対象クラス(INodeActionSource実装)。
    // Systemの型は直接シリアライズできないため、AssemblyQualifiedNameで保持する。
    public string TargetTypeName;
}
