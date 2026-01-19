using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public static partial class NodeExtensions
{
    public static void SetParent(this Node node, Node parent, bool keepGlobalTransform = true)
    {
        if (node.GetParent() == null)
        {
            parent.AddChild(node);
        }
        else
        {
            node.Reparent(parent, keepGlobalTransform);
        }
    }

    public static T GetNodeInChildren<T>(this Node node, string name) where T : Node
    {
        if (node.Name == name) return node as T;

        foreach (var child in node.GetChildren())
        {
            var valid_child = child.GetNodeInChildren<T>(name);
            if (valid_child != null)
                return valid_child;
        }

        return null;
    }

    public static T GetNodeInChildren<T>(this Node node) where T : Node
    {
        if (node.TryGetNode<T>(out var result)) return result;

        foreach (var child in node.GetChildren())
        {
            T script = child.GetNodeInChildren<T>();
            if (script != null)
                return script;
        }

        return null;
    }

    public static Node GetNodeInChildren(this Node node, Type type)
    {
        if (IsNodeOfType(node, type)) return node;

        foreach (var child in node.GetChildren())
        {
            var childNode = child.GetNodeInChildren(type);
            if (childNode != null)
                return childNode;
        }

        return null;
    }

    private static bool IsNodeOfType(Node node, Type type)
    {
        var node_type = node.GetType();
        if (IsSameOrSubclass(type, node_type)) return true;
        if (node_type.GetInterface(type.Name) != null) return true;
        return false;
    }

    private static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
    {
        return potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
    }

    public static List<T> GetNodesInChildren<T>(this Node node, Func<T, bool> predicate = null)
        where T : Node
    {
        var list = new List<T>();
        Recursive(node);
        return list;

        void Recursive(Node current)
        {
            var node_as_type = current as T;
            var node_exists = node_as_type != null;
            var predicate_success = node_exists && (predicate?.Invoke(node_as_type) ?? true);

            if (predicate_success)
            {
                list.Add(node_as_type);
            }

            foreach (var child in current.GetChildren(true))
            {
                Recursive(child);
            }
        }
    }

    public static T GetNodeInParents<T>(this Node node, int max_depth = -1) where T : class
    {
        var depth = 0;
        var current = node;
        while (current != null && depth != max_depth)
        {
            if (current is T) return current as T;
            current = current.GetParent();
            depth++;
        }

        return null;
    }

    public static List<T> GetNodesInParents<T>(this Node node, int max_depth = -1) where T : class
    {
        var list = new List<T>();
        var depth = 0;
        var current = node;
        while (current != null && depth != max_depth)
        {
            if (current is T) list.Add(current as T);
            current = current.GetParent();
            depth++;
        }

        return list;
    }

    public static bool TryGetNode<T>(this Node parent, out T script) where T : Node
    {
        try
        {
            script = parent.GetNode<T>(parent.GetPath());
            return script != null;
        }
        catch
        {
            script = null;
            return false;
        }
    }

    public static bool TryGetNode<T>(this Node parent, string path, out T script) where T : Node
    {
        try
        {
            script = parent.GetNodeOrNull<T>(path);
            return script != null;
        }
        catch
        {
            script = null;
            return false;
        }
    }

    public static Coroutine StartCoroutine(this Node node, Func<IEnumerator> enumerator, string id = null)
    {
        return node.StartCoroutine(enumerator(), id);
    }

    public static Coroutine StartCoroutine(this Node node, IEnumerator enumerator, string id = null)
    {
        id = (id ?? string.Empty) + node.GetInstanceId();
        return Coroutine.Start(enumerator, id, node);
    }
}