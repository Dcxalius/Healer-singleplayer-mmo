using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.GuildMember
{
    internal class LogicTree
    {
        LogicNode rootNode;
        LogicNode currentNode;
        internal LogicNode RootNode => rootNode;
        public LogicTree(LogicNode aRootNode)
        {
            rootNode = aRootNode;
            currentNode = rootNode;
        }
        public void FindNextResult()
        {
            currentNode.OnExit();
            LogicNode node = currentNode.NextNode;
            
            if (node == null)
            {
                currentNode = rootNode;
                return;
            }
            currentNode = node;
            currentNode.OnEnter();
        }

        public ILogicResult GetNextResult()
        {
            rootNode.OnEnter();
            int failsafe = 0;
            const int maxIterations = 128;
            while (failsafe++ < maxIterations)
            {
                ILogicResult result = currentNode.Result;
                if (result is Continue)
                {
                    LogicNode before = currentNode;
                    FindNextResult();
                    if (before == currentNode)
                    {
                        currentNode = rootNode;
                        return result;
                    }
                }
                else
                {
                    currentNode = rootNode;
                    return result;
                }
            }

            currentNode = rootNode;
            return new Continue();
        }
    }
}
