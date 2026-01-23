using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SelectionController : SingletonController
{
    public static SelectionController Instance => Singleton.Get<SelectionController>();
    public override string Directory => "Selection";
    public bool Dragging { get; private set; }
    public Vector2 ViewportDragStart { get; private set; }
    public Vector3 DragStart { get; private set; }
    public Vector3 DragEnd { get; private set; }

    private List<NodeObject> selected_nodes = new();

    public override void _UnhandledInput(InputEvent e)
    {
        base._UnhandledInput(e);

        if (e is InputEventMouseButton button)
        {
            if (button.Pressed)
            {
                ViewportDragStart = DraggableCamera.Instance.MousePosition;
                DragStart = DraggableCamera.Instance.MouseWorldPosition;
                Dragging = true;
            }
            else
            {
                DragEnd = DraggableCamera.Instance.MouseWorldPosition;
                Dragging = false;
                SelectNodesInArea();
            }

            GetViewport().SetInputAsHandled();
        }
    }

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
            if (!selected_nodes.Contains(node))
            {
                selected_nodes.Add(node);
                node.SetSelected(true);
            }
        }
        else
        {
            if (selected_nodes.Contains(node))
            {
                selected_nodes.Remove(node);
                node.SetSelected(false);
            }
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

    private void SelectNodesInArea()
    {
        var lx = Mathf.Min(DragStart.X, DragEnd.X);
        var hx = Mathf.Max(DragStart.X, DragEnd.X);
        var lz = Mathf.Min(DragStart.Z, DragEnd.Z);
        var hz = Mathf.Max(DragStart.Z, DragEnd.Z);

        var current_selection = selected_nodes.ToList();
        var all_nodes = NodeController.Instance.GetNodes();
        var inside = all_nodes.Where(x => Inside(x.GlobalPosition)).ToList();
        var except = current_selection.Except(inside).ToList();

        UndoController.Instance.StartUndoAction();

        inside.ForEach(x => SelectNode(x, true));

        if (!PlayerInput.Select.Held)
        {
            except.ForEach(x => SelectNode(x, false));
        }

        UndoController.Instance.EndUndoAction();

        bool Inside(Vector3 position)
        {
            return position.X > lx
                && position.X < hx
                && position.Z > lz
                && position.Z < hz;
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
