using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.GuildMember
{
    internal class LogicNode
    {
        public ILogicResult Result => result != null ? result : new Continue();
        public ILogicResult RawResult => result;
        public IReadOnlyList<LogicNode> Children => nextNodes ?? Array.Empty<LogicNode>();
        public IReadOnlyList<Func<bool>> Conditions => conditions ?? Array.Empty<Func<bool>>();

        LogicNode[] nextNodes;
        Func<bool>[] conditions;
        ILogicResult result;

        public LogicNode NextNode => (nextNodes != null && NextIndex != -1) ? nextNodes[NextIndex] : null;
        public int NextIndex
        {
            get
            {
                if (nextNodes == null) return -1;
                for (int i = 0; i < nextNodes.Length; i++)
                {
                    if (conditions[i].Invoke()) return i;
                }
                return -1;
            }
        }

        public LogicNode(ILogicResult result)
        {
            nextNodes = null;
            conditions = null;
            this.result = result;
        }

        public LogicNode(LogicNode[] aNextNodes, Func<bool>[] aCondition) 
        {
            nextNodes = aNextNodes;
            conditions = aCondition;
        }

        public virtual void Execute()
        {
            //Override in child classes
        }

        public virtual void OnEnter()
        {
            //Override in child classes
        }
        public virtual void OnExit()
        {
            //Override in child classes
        }
    }
}
