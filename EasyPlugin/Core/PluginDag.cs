using EasyPlugin.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyPlugin.Core
{
    /// <summary>
    /// DAG管理器，负责图的拓扑排序和环检测
    /// </summary>
    public class PluginDag
    {
        private readonly List<PluginBase> _nodes = new List<PluginBase>();

        public IReadOnlyList<PluginBase> Nodes => _nodes.AsReadOnly();

        public void AddNode(PluginBase node)
        {
            if (_nodes.Any(n => n.Id == node.Id))
                throw new ArgumentException($"Node with ID '{node.Id}' already exists");

            node.Validate();
            _nodes.Add(node);
        }
        /// <summary>
        /// 使用节点id连接两个节点
        /// </summary>
        /// <param name="fromNodeId">前置节点id</param>
        /// <param name="toNodeId">后置节点id</param>
        /// <exception cref="ArgumentException"></exception>
        public void ConnectNodes(string fromNodeId, string toNodeId)
        {
            var fromNode = _nodes.FirstOrDefault(n => n.Id == fromNodeId);
            var toNode = _nodes.FirstOrDefault(n => n.Id == toNodeId);

            if (fromNode == null || toNode == null)
                throw new ArgumentException("One or both nodes not found");

            fromNode.AddNext(toNode);
        }
        public void ConnectNodes(PluginBase fromNode, PluginBase toNode)
        {
            if (fromNode == null || toNode == null)
            {
                throw new ArgumentException("One or both nodes is null");
            }
            if (!_nodes.Contains(fromNode))
            {
                throw new ArgumentException($"Node with ID '{fromNode.Id}' not found, please add it first");
            }
            if (!_nodes.Contains(toNode))
            {
                throw new ArgumentException($"Node with ID '{toNode.Id}' not found, please add it first");
            }

            fromNode.AddNext(toNode);
        }

        /// <summary>
        /// 检测环并执行拓扑排序
        /// </summary>
        /// <returns>排序后的节点列表</returns>
        /// <exception cref="CyclicDependencyException">循环依赖异常</exception>
        private List<PluginBase> TopologicalSort()
        {
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();
            var result = new Stack<PluginBase>();

            foreach (var node in _nodes.Where(n => !n.Previous.Any()))
            {
                if (HasCycle(node, visited, visiting, result))
                {
                    throw new CyclicDependencyException();
                }
            }

            // 检查是否所有节点都被访问
            if (visited.Count != _nodes.Count)
            {
                throw new CyclicDependencyException();
            }

            return result.ToList();
        }

        private bool HasCycle(PluginBase node, HashSet<string> visited, HashSet<string> visiting, Stack<PluginBase> result)
        {
            if (visiting.Contains(node.Id))
                return true; // 发现环

            if (visited.Contains(node.Id))
                return false;

            visiting.Add(node.Id);

            foreach (var dependent in node.Next)
            {
                if (HasCycle(dependent, visited, visiting, result))
                    return true;
            }

            visiting.Remove(node.Id);
            visited.Add(node.Id);
            result.Push(node);

            return false;
        }

        /// <summary>
        /// 获取可并行执行的节点组
        /// </summary>
        /// <returns></returns>
        public List<List<PluginBase>> GetParallelGroups()
        {
            var groups = new List<List<PluginBase>>();
            var processed = new HashSet<PluginBase>(); // 记录已处理的节点
            var remaining = new Queue<PluginBase>(_nodes); // 记录未处理的节点

            while (remaining.Count > 0)
            {
                var currentGroup = new List<PluginBase>();
                var toRemove = new List<PluginBase>();

                // 找出所有依赖都已满足的节点
                foreach (var node in remaining)
                {
                    // 检查该节点的所有依赖是否都已处理
                    var canExecute = node.Previous.All(dep => processed.Contains(dep));

                    if (canExecute)
                    {
                        currentGroup.Add(node);
                        toRemove.Add(node);
                    }
                }

                if (currentGroup.Count == 0)
                {
                    throw new CyclicDependencyException();
                }

                groups.Add(currentGroup);

                // 更新处理状态
                foreach (var node in currentGroup)
                {
                    processed.Add(node);
                    // 从队列中移除已处理的节点
                    var newQueue = new Queue<PluginBase>();
                    while (remaining.Count > 0)
                    {
                        var item = remaining.Dequeue();
                        if (!toRemove.Contains(item))
                        {
                            newQueue.Enqueue(item);
                        }
                    }
                    remaining = newQueue;
                }
            }

            return groups;
        }

        private string GetGraphStructure()
        {
            try
            {
                var topologicalOrder = TopologicalSort();
                var sb = new StringBuilder();

                // 构建节点层级关系
                var nodeLevels = CalculateNodeLevels(topologicalOrder);
                var maxLevel = nodeLevels.Values.Any() ? nodeLevels.Values.Max() : 0;

                // 按层级打印
                for (int level = 0; level <= maxLevel; level++)
                {
                    var nodesInLevel = nodeLevels.Where(kv => kv.Value == level)
                                               .Select(kv => kv.Key)
                                               .ToList();

                    sb.Append($"Level {level}: ");

                    foreach (var node in nodesInLevel)
                    {
                        sb.Append($"[{node.Id}]");

                        // 打印指向下一层的连接
                        var dependents = node.Next.Where(d => nodeLevels[d] == level + 1).ToList();
                        if (dependents.Any())
                        {
                            sb.Append($" → {string.Join(",", dependents.Select(d => d.Id))}");
                        }

                        sb.Append("   ");
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            catch (CyclicDependencyException ex)
            {
                return $"图形化显示失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 计算每个节点的层级
        /// </summary>
        private Dictionary<PluginBase, int> CalculateNodeLevels(List<PluginBase> topologicalOrder)
        {
            var levels = new Dictionary<PluginBase, int>();

            foreach (var node in topologicalOrder)
            {
                if (!node.Previous.Any())
                {
                    levels[node] = 0;
                }
                else
                {
                    var maxParentLevel = node.Previous.Max(dep => levels.ContainsKey(dep) ? levels[dep] : -1);
                    levels[node] = maxParentLevel + 1;
                }
            }

            return levels;
        }
        
        public List<string> GetGraphNodeInofs()
        {
            var infos = new List<string>();
            foreach (var node in Nodes)
            {
                infos.Add(node.ToString());
            }
            return infos;
        }
        public override string ToString()
        {
            return GetGraphStructure();
        }
    }
}
