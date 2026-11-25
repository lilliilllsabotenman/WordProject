// Script/NVS/RuntimeNodeExecutor.cs
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum ConditionParameter
{
    True,
    False
}

// グラフの探索(次にどのノードへ進むか)と、ノード種別→NodeExecutorの呼び出しの振り分けだけを担う「実行マネージャー」。
// 各ノードを実際にどう実行するか(引数解決・メソッド呼び出し)はNodeExecutorの責務。
// 依存はRuntimeNodeExecutor → NodeExecutor の一方向のみ。
public class RuntimeNodeExecutor
{
    private readonly NodeExecutor _nodeExecutor;

    // ノード種別 → そのノードを実行し、進む先の出力ポート名を返す処理。
    // ノード種別を増やすときはここに1行追加する。nullを返すとそこで実行を終える。
    private readonly Dictionary<Type, Func<BaseNodeData, Task<string>>> _handlers;

    // グラフの制御フロー(次にどのノードへ進むか)。
    private readonly Dictionary<string, BaseNodeData> _nodeMap = new Dictionary<string, BaseNodeData>();
    private readonly Dictionary<string, string> _edgeMap = new Dictionary<string, string>();
    private string _startGuid;

    // 二重実行防止用。実行中にRunAsyncが再度呼ばれた場合は警告して無視する。
    private bool _isRunning;

    // 待機の無いループの警告を、このインスタンスで1回だけ出すためのフラグ。
    private bool _syncLoopWarned;

    public RuntimeNodeExecutor(VisualScriptingGraphData graphData, INodeActionSource target)
    {
        BuildNodeMap(graphData);
        BuildEdgeMap(graphData);

        _nodeExecutor = new NodeExecutor(graphData, target);

        _handlers = new Dictionary<Type, Func<BaseNodeData, Task<string>>>
        {
            [typeof(StartNodeData)] = _ => Task.FromResult("Out"),
            [typeof(ActionNodeData)] = async node => { await _nodeExecutor.InvokeActionAsync((ActionNodeData)node); return "Out"; },
            [typeof(SetMemberNodeData)] = node => { _nodeExecutor.ExecuteSetMember((SetMemberNodeData)node); return Task.FromResult("Out"); },
            [typeof(ConditionNodeData)] = node => Task.FromResult(_nodeExecutor.InvokeCondition((ConditionNodeData)node)),
        };
    }

    // グラフをStartノードから最後まで実行する。Taskを返すノードは完了まで待つ。
    // tokenがキャンセルされると、次のノードへ進む前にOperationCanceledExceptionで中断する。
    // ノードで発生した例外は戻り値のTaskに格納されるため、呼び出し側はawaitして受け取ること。
    public async Task RunAsync(CancellationToken token = default)
    {
        if (_isRunning)
        {
            Debug.LogWarning("RuntimeNodeExecutor: グラフは実行中です。RunAsyncの再呼び出しを無視しました。");
            return;
        }

        _isRunning = true;
        try
        {
            await RunFromAsync(_startGuid, token);
        }
        finally
        {
            _isRunning = false;
        }
    }

    // startGuidから、次のノードが無くなるまでノードを辿り続ける。
    private async Task RunFromAsync(string startGuid, CancellationToken token)
    {
        string currentGuid = startGuid;

        // 最後に実際の待機が発生してから通過したノード。
        // 待機を挟まずに同じノードへ戻ってきた場合のみ「待機の無いループ」とみなし、1フレーム待ってから続ける。
        // 直線のグラフは同じノードを2回通らないため、ノード数に関係なく待機なしで最後まで実行される。
        HashSet<string> visitedSinceWait = new HashSet<string>();

        while (!string.IsNullOrEmpty(currentGuid))
        {
            token.ThrowIfCancellationRequested();

            if (!_nodeMap.TryGetValue(currentGuid, out BaseNodeData currentNode))
            {
                Debug.LogError($"RuntimeNodeExecutor: ノードが見つかりません: {currentGuid}");
                return;
            }

            if (!visitedSinceWait.Add(currentGuid))
            {
                if (!_syncLoopWarned)
                {
                    Debug.LogWarning($"RuntimeNodeExecutor: 待機の無いループを検出しました({currentNode.Title})。1周ごとに1フレーム待機します。");
                    _syncLoopWarned = true;
                }

                await Awaitable.NextFrameAsync(token);
                visitedSinceWait.Clear();
                visitedSinceWait.Add(currentGuid);
            }

            _nodeExecutor.ClearActionResultCache();

            Task<string> step = ExecuteNodeAsync(currentNode);
            if (!step.IsCompleted)
            {
                // 実際に待機が発生する。ここまでの通過記録は不要になる。
                visitedSinceWait.Clear();
            }

            string outputPort = await step;
            if (string.IsNullOrEmpty(outputPort)) return;

            // 出力ポートに配線が無ければ正常終了。
            if (!_edgeMap.TryGetValue(currentGuid + ":" + outputPort, out currentGuid)) return;
        }
    }

    private async Task<string> ExecuteNodeAsync(BaseNodeData node)
    {
        if (!_handlers.TryGetValue(node.GetType(), out Func<BaseNodeData, Task<string>> handler))
        {
            Debug.LogError($"RuntimeNodeExecutor: 未対応のノード種別です: {node.GetType().Name}");
            return null;
        }

        try
        {
            return await handler(node);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            // リフレクション呼び出しの例外はTargetInvocationExceptionに包まれるため、中身を取り出す。
            Exception inner = e is TargetInvocationException tie && tie.InnerException != null ? tie.InnerException : e;
            throw new Exception($"ノード '{node.Title}' ({node.Guid}) の実行中に例外が発生しました: {inner.Message}", inner);
        }
    }

    private void BuildNodeMap(VisualScriptingGraphData graphData)
    {
        foreach (BaseNodeData nodeData in graphData.Nodes)
        {
            _nodeMap[nodeData.Guid] = nodeData;
            if (nodeData is StartNodeData)
            {
                _startGuid = nodeData.Guid;
            }
        }
    }

    // 出力ポート("ノードGuid:ポート名") → 次に進むノードのGuid。
    private void BuildEdgeMap(VisualScriptingGraphData graphData)
    {
        foreach (EdgeData edgeData in graphData.Edges)
        {
            _edgeMap[edgeData.OutputNodeGuid + ":" + edgeData.OutputPortName] = edgeData.InputNodeGuid;
        }
    }
}
