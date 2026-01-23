using System.Collections.Generic;

public partial class SelectionController : SingletonController
{
    public static SelectionController Instance => Singleton.Get<SelectionController>();
    public override string Directory => "Selection";

    private List<NodeObject> selected_nodes = new();

    public void ClearSelection()
    {
        foreach (var node in selected_nodes)
        {
            node.SetSelected(false);
        }

        selected_nodes.Clear();
    }

    public void SelectNode(string name, bool selected)
    {
        var node = NodeController.Instance.GetNode(name);
        if (node == null) return;

        SelectNode(node, selected);
    }

    public void SelectNode(NodeObject node, bool selected)
    {
        if (selected)
        {
            selected_nodes.Add(node);
            node.SetSelected(true);
        }
        else
        {
            selected_nodes.Remove(node);
            node.SetSelected(false);
        }
    }

    public void ToggleNode(NodeObject node)
    {
        if (selected_nodes.Contains(node))
        {
            SelectNode(node, false);
        }
        else
        {
            SelectNode(node, true);
        }
    }

    public void DragSelection()
    {
        foreach (var node in selected_nodes)
        {
            node.Drag();
        }
    }

    public void DragEndSelection()
    {
        UndoController.Instance.StartUndoAction();
        foreach (var node in selected_nodes)
        {
            node.DragEnd();
            UndoController.Instance.AddMoveNodeAction(node.NodeName, node.DragStartPosition, node.DragEndPosition);
        }
        UndoController.Instance.EndUndoAction();
    }
}
