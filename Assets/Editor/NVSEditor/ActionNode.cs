// Editor/Nodes/ActionNode.cs
using UnityEditor.Experimental.GraphView;
using System;
using System.Collections.Generic;

namespace NVSEditor
{
    public class ActionNode : BaseNode
    {
        private string _actionKey;

        public string ActionKey
        {
            get { return _actionKey; }
            set
            {
                _actionKey = value;
                if (!string.IsNullOrEmpty(value)) title = value;
            }
        }

        public ActionNode(string initialMethodName, List<MethodParamInfo> param = null, Type returnType = null, List<NodeParamEntry> savedParams = null, string displayName = null) : base("Action")
        {
            inputContainer.Add(CreateExecPort(Direction.Input, Port.Capacity.Multi));
            outputContainer.Add(CreateExecPort(Direction.Output, Port.Capacity.Multi));

            if (param != null)
            {
                foreach (MethodParamInfo p in param)
                {
                    NodeParamEntry saved = savedParams?.Find(e => e.Key == p.Name);
                    AddInput(p.Name, p.Type, saved);
                }
            }

            if (returnType != null && returnType != typeof(void))
            {
                AddOutput("Result", returnType);
            }

            ActionKey = initialMethodName;
            if (!string.IsNullOrEmpty(displayName)) title = displayName;

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
