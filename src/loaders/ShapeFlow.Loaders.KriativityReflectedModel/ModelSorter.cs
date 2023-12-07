using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using QuickGraph;
using QuickGraph.Algorithms;

namespace ShapeFlow.Loaders.KriativityReflectedModel
{
    public static class ModelSorter
    {
        public static IEnumerable<T> Sort<T>(IEnumerable<T> values)
            where T : ReflectedObject
        {
            var graph = new AdjacencyGraph<string, Edge<string>>(true);

            foreach (var typeName in values)
            {
                var type = typeName.Type;

                //this type's base types
                AddTypeDependencies(graph, type);
            }

            var sortedNodes = graph.TopologicalSort();

            var allTypesSortedNodes = sortedNodes.Where(root => values.Any(v => v.Type == root)).ToList();

            var verticesDictionary = graph.Vertices.ToDictionary(s => s,
                s =>
                {
                    graph.TryGetOutEdges(s, out var outEdges);
                    return outEdges;
                });

            var nodesWithoutParents = allTypesSortedNodes.Where(node => verticesDictionary[node] == null || !verticesDictionary[node].Any()).ToList();
            allTypesSortedNodes.RemoveAll(s => nodesWithoutParents.Contains(s));
            allTypesSortedNodes.AddRange(nodesWithoutParents);

            var notAnalyzedTypes = values.Where(s => !allTypesSortedNodes.Contains(s.Type)).Select(t => t.Type).ToList();
            allTypesSortedNodes.AddRange(notAnalyzedTypes);

            List<T> results = new List<T>();

            foreach (var sorted in allTypesSortedNodes)
            {
                results.Add(values.First(v => v.Type == sorted));
            }

            return results;
        }

        private static void AddTypeDependencies(IMutableVertexAndEdgeListGraph<string, Edge<string>> graph, string type)
        {
            //if (type.IsGeneric())
            //{
            //    if (type.GetGenericArguments().Length > 0)
            //    {
            //        if (typeof(IEnumerable).IsAssignableFrom(type))
            //        {                        
            //            var genericTypeDef = type.GetGenericTypeDefinition();
            //            if (!graph.ContainsVertex(type.FullName))
            //            {
            //                graph.AddVertex(type.FullName);                     
            //            }

            //            if (IsValid(genericTypeDef) && genericTypeDef != type)
            //            {
            //                AddTypeDependencies(graph, genericTypeDef);

            //                if (!graph.ContainsEdge(genericTypeDef.FullName, type.FullName))
            //                {
            //                    if (genericTypeDef.FullName != type.FullName)
            //                    {
            //                        graph.AddEdge(new Edge<string>(genericTypeDef.FullName, type.FullName));                                    
            //                    }
            //                }
            //            }

            //            foreach (var typeArg in type.GetGenericArguments())
            //            {
            //                if(!IsValid(genericTypeDef))
            //                {
            //                    continue;
            //                }

            //                AddTypeDependencies(graph, typeArg);
            //                string typeArgName = type.FullName;
            //                if (!graph.ContainsEdge(typeArgName, type.FullName))
            //                {
            //                    if (typeArgName != type.FullName)
            //                    {
            //                        graph.AddEdge(new Edge<string>(typeArgName, type.FullName));                                    
            //                    }
            //                }

            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    if (!IsValid(type))
            //        return;
                
            //    if (!graph.ContainsVertex(type.FullName))
            //    {
            //        graph.AddVertex(type.FullName);
                    
            //        var bt = type.BaseType;
            //        if (IsValid(type.BaseType))
            //        {                    
            //            AddTypeDependencies(graph, type.BaseType);
                                             
            //            if (graph.ContainsVertex(bt.FullName))
            //            {                            
            //                if (!graph.ContainsEdge(bt.FullName, type.FullName))
            //                {
            //                    graph.AddEdge(new Edge<string>(bt.FullName, type.FullName));                                
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private static bool IsValid(Type type)
        {
            if (type == null || type.FullName == null) return false;

            return true;
        }
    }
}
