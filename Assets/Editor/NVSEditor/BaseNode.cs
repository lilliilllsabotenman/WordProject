// Editor/Nodes/BaseNode.cs
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

namespace NVSEditor
{
    public abstract class BaseNode : Node
    {
        public string NodeGuid;

        // 引数の値は保管庫(NodeParamHolder)に置き、PropertyFieldで編集する。
        private readonly NodeParamHolder _holder;
        private readonly SerializedObject _serializedHolder;
        public List<NodeParamEntry> Params => _holder.Params;

        protected VisualElement ParamsContainer;

        protected BaseNode(string title)
        {
            this.title = title;
            NodeGuid = System.Guid.NewGuid().ToString();

            ParamsContainer = new VisualElement();
            extensionContainer.Add(ParamsContainer);

            _holder = ScriptableObject.CreateInstance<NodeParamHolder>();
            _holder.hideFlags = HideFlags.HideAndDontSave;
            _serializedHolder = new SerializedObject(_holder);
        }

        // Execポートを生成するヘルパー
        protected Port CreateExecPort(Direction direction, Port.Capacity capacity)
        {
            var port = Port.Create<Edge>(
                Orientation.Horizontal,
                direction,
                capacity,
                typeof(bool) // Exec接続はboolで型を統一
            );
            port.portName = direction == Direction.Input ? "In" : "Out";
            return port;
        }

        // BaseNode
        protected Port CreateDataPort(Direction direction, string portName, Type dataType, Port.Capacity capacity = Port.Capacity.Single)
        {
            var port = Port.Create<Edge>(
                Orientation.Horizontal,
                direction,
                capacity,
                dataType
            );
            port.portName = portName;
            return port;
        }


        protected void AddInput(string key, Type type, NodeParamEntry saved = null)
        {
            Params.Add(new NodeParamEntry { Key = key, Value = CreateBox(key, type, saved) });
            _serializedHolder.Update();

            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            Port port = CreateDataPort(Direction.Input, key, type);
            row.Add(port);

            SerializedProperty valueProp =
                _serializedHolder.FindProperty($"Params.Array.data[{Params.Count - 1}].Value.value");
            if (valueProp == null)
            {
                // Unityが保存できない型(Dictionary、interfaceなど)
                row.Add(new Label("未対応の型"));
            }
            else
            {
                PropertyField field = new PropertyField(valueProp, key);
                field.Bind(_serializedHolder);
                row.Add(field);
            }

            ParamsContainer.Add(row);

            RefreshExpandedState();
            RefreshPorts();
        }

        // 保存済みの箱の型が今の引数と同じならそれを使い、違えば警告して新しい箱を作る。
        private static ParamValue CreateBox(string key, Type type, NodeParamEntry saved)
        {
            if (saved?.Value != null)
            {
                if (saved.Value.ValueType == type) return saved.Value;
                Debug.LogWarning($"引数 '{key}' の型が {saved.Value.ValueType.Name} から {type.Name} に変わったため、値を初期化しました。");
            }

            return (ParamValue)Activator.CreateInstance(typeof(ParamValue<>).MakeGenericType(type));
        }

        protected void AddOutput(string key, Type type)
        {
            Port port = CreateDataPort(Direction.Output, key, type, Port.Capacity.Multi);
            outputContainer.Add(port);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
