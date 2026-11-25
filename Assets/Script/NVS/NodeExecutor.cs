// Script/NVS/NodeExecutor.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

// 1個のノードを実際にどう実行するか(引数解決・メソッド呼び出し・メンバ変数の読み書き)を担う。
// グラフ探索(次にどのノードへ進むか)はRuntimeNodeExecutorの責務で、こちらは探索について何も知らない。
public class NodeExecutor
{
    private readonly VisualScriptingGraphData _graphData;
    private readonly INodeActionSource _target;
    private readonly Dictionary<string, MethodInfo> _methodCache;
    private readonly Dictionary<string, object> _members;

    // 入力ポート("ノードGuid:ポート名") → そこへ入ってくるエッジ。引数解決のたびにEdgesを線形探索しないための索引。
    private readonly Dictionary<string, EdgeData> _incomingEdges = new Dictionary<string, EdgeData>();
    private readonly Dictionary<string, BaseNodeData> _nodeMap = new Dictionary<string, BaseNodeData>();

    // 1回のグラフ実行中、同じActionノードの戻り値を使い回すためのキャッシュ。
    private readonly Dictionary<string, object> _actionResultCache = new Dictionary<string, object>();

    public NodeExecutor(VisualScriptingGraphData graphData, INodeActionSource target)
    {
        _graphData = graphData;
        _target = target;

        _methodCache = target.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .GroupBy(m => m.Name)
            .ToDictionary(g => g.Key, g => g.First());

        _members = BuildMembers(graphData, target);

        foreach (BaseNodeData node in graphData.Nodes)
        {
            _nodeMap[node.Guid] = node;
        }

        foreach (EdgeData edge in graphData.Edges)
        {
            string key = edge.InputNodeGuid + ":" + edge.InputPortName;
            // 同じ入力ポートに複数のエッジがある場合は、従来(List.Find)と同じく先頭を優先する。
            if (!_incomingEdges.ContainsKey(key)) _incomingEdges[key] = edge;
        }
    }

    #region 公開エントリーポイント(RuntimeNodeExecutorから呼ばれる)

    // 制御フロー上のノードを1つ実行する直前に呼ばれる(RuntimeNodeExecutor.RunFromAsync)。
    // キャッシュの寿命を「制御フローのノード1個分」にすることで、ループ中のGetterが毎周評価し直される。
    public void ClearActionResultCache()
    {
        _actionResultCache.Clear();
    }

    // 制御フロー上のActionノードを実行する。メソッドがTask/Task<T>を返した場合は完了まで待つ。
    // Task<T>の場合は結果をキャッシュに入れるため、後続ノードのデータ配線からも値を参照できる。
    public async Task<object> InvokeActionAsync(ActionNodeData actionData)
    {
        if (_actionResultCache.TryGetValue(actionData.Guid, out object cachedResult))
        {
            return cachedResult;
        }

        MethodInfo method = GetMethod(actionData.MethodKey);
        if (method == null) return null;

        object result = method.Invoke(_target, ResolveArguments(method, actionData));

        if (result is Task task)
        {
            await task;
            result = GetTaskResult(method, task);
        }

        _actionResultCache[actionData.Guid] = result;
        return result;
    }

    // Action/Getterノードのメソッドを呼び出し、戻り値を返す(データ配線から呼ばれる同期版)。
    public object InvokeAction(ActionNodeData actionData)
    {
        if (_actionResultCache.TryGetValue(actionData.Guid, out object cachedResult))
        {
            return cachedResult;
        }

        MethodInfo method = GetMethod(actionData.MethodKey);
        if (method == null) return null;

        object result = method.Invoke(_target, ResolveArguments(method, actionData));
        _actionResultCache[actionData.Guid] = result;
        return result;
    }

    // Conditionノードのメソッドを呼び出し、進む先の出力ポート名(ConditionParameterの名前)を返す。
    public string InvokeCondition(ConditionNodeData conditionData)
    {
        MethodInfo method = GetMethod(conditionData.MethodKey);
        if (method == null) return null;

        return ((ConditionParameter)method.Invoke(_target, ResolveArguments(method, conditionData))).ToString();
    }

    // SetMemberノード: "Value"ポートに配線された値をメンバ変数へ書き込む。
    public void ExecuteSetMember(SetMemberNodeData setMemberData)
    {
        if (!TryResolveWiredValue(setMemberData.Guid, "Value", out object value)) return;

        SetMember(setMemberData.MemberName, value);
    }

    #endregion

    #region 引数・値の解決

    private object[] ResolveArguments(MethodInfo method, BaseNodeData nodeData)
    {
        ParameterInfo[] parameters = method.GetParameters();
        object[] args = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            args[i] = ResolveArgument(parameters[i], nodeData);
        }
        return args;
    }

    // 入力ポートに配線があれば配線元ノードをその場で実行して戻り値を使い、
    // 無ければParamsの箱に入った値を使う。
    private object ResolveArgument(ParameterInfo parameter, BaseNodeData nodeData)
    {
        if (TryResolveWiredValue(nodeData.Guid, parameter.Name, out object wiredValue))
        {
            return wiredValue;
        }

        NodeParamEntry entry = nodeData.Params?.Find(p => p.Key == parameter.Name);
        ParamValue box = entry?.Value;
        if (box != null && box.ValueType == parameter.ParameterType) return box.Get();

        // 未設定、または箱の型が引数の型と合わない場合は既定値。
        Type type = parameter.ParameterType;
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    // 入力ポート(nodeGuid, portName)に、値を返せるノード(Action/GetMember)が配線されていれば、その値を解決する。
    // 配線が無ければfalseを返すので、呼び出し側はリテラル値へのフォールバックなどを行える。
    private bool TryResolveWiredValue(string nodeGuid, string portName, out object value)
    {
        value = null;

        if (!_incomingEdges.TryGetValue(nodeGuid + ":" + portName, out EdgeData edge)) return false;
        if (!_nodeMap.TryGetValue(edge.OutputNodeGuid, out BaseNodeData sourceNode)) return false;

        switch (sourceNode)
        {
            case ActionNodeData sourceActionData:
                value = InvokeAction(sourceActionData);
                return true;
            case GetMemberNodeData sourceMemberData:
                value = ResolveMember(sourceMemberData.MemberName);
                return true;
            default:
                return false;
        }
    }

    // 完了済みTaskから結果を取り出す。判定は実体の型ではなくメソッドの宣言上の戻り値型で行う。
    // (async Taskメソッドの実体はTask<VoidTaskResult>の派生になっており、実体の型で見ると誤判定するため)
    private static object GetTaskResult(MethodInfo method, Task task)
    {
        Type returnType = method.ReturnType;
        if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(Task<>)) return null;

        return returnType.GetProperty("Result")?.GetValue(task);
    }

    private MethodInfo GetMethod(string methodName)
    {
        if (string.IsNullOrEmpty(methodName)) return null;

        _methodCache.TryGetValue(methodName, out MethodInfo method);
        return method;
    }

    private static object ConvertLiteral(string value, Type type)
    {
        if (string.IsNullOrEmpty(value))
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        if (type == typeof(bool))
        {
            return bool.TryParse(value, out bool result) && result;
        }

        return Convert.ChangeType(value, type);
    }

    #endregion

    #region メンバ変数

    private static Dictionary<string, object> BuildMembers(VisualScriptingGraphData graphData, INodeActionSource target)
    {
        Dictionary<string, object> members = new Dictionary<string, object>();

        foreach (MemberVariableData member in graphData.Members)
        {
            Type type = string.IsNullOrEmpty(member.TypeName) ? null : Type.GetType(member.TypeName);
            if (type == null)
            {
                Debug.LogWarning($"メンバ '{member.Name}' の型を解決できません: {member.TypeName}");
                members[member.Name] = null;
                continue;
            }

            if (member.Kind == MemberKind.Component)
            {
                Component resolved = target.Owner.GetComponent(type);
                if (resolved == null)
                {
                    Debug.LogWarning($"メンバ '{member.Name}' に対応する {type.Name} が {target.Owner.name} に見つかりません");
                }

                members[member.Name] = resolved;
            }
            else
            {
                members[member.Name] = ConvertLiteral(member.DefaultValue, type);
            }
        }

        return members;
    }

    private object ResolveMember(string name)
    {
        if (!_members.TryGetValue(name, out object value))
        {
            throw new KeyNotFoundException($"未定義のメンバです: {name}");
        }

        return value;
    }

    private void SetMember(string name, object value)
    {
        if (!_members.ContainsKey(name))
        {
            throw new KeyNotFoundException($"未定義のメンバです: {name}");
        }

        _members[name] = value;
    }

    #endregion
}
