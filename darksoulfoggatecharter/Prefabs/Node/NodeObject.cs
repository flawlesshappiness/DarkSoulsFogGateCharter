using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class NodeObject : Area3D
{
    [Export]
    public MeshInstance3D Mesh;

    [Export]
    public MeshInstance3D Mesh_Select;

    [Export]
    public Label3D Label;

    public virtual string NodeName => string.Empty;

    public Dictionary<string, NodeObject> ConnectedNodes = new();

    public static NodeObject Handled { get; private set; }
    public static NodeObject Hovered { get; private set; }
    public bool IsFullyConnected => ConnectedNodes.Count >= 2;
    public Vector3 DragStartPosition { get; private set; }
    public Vector3 DragEndPosition { get; private set; }
    protected bool IsHandled => Handled == this;
    protected bool IsHovered => Hovered == this;
    protected bool IsPressed { get; private set; }
    protected bool IsDragging { get; private set; }
    protected bool IsDestroying { get; set; }
    public bool IsSelected { get; private set; }
    protected List<NodeRelation> Relations { get; private set; } = new();

    public event Action OnClicked;
    public event Action OnDragStarted;
    public event Action OnDragEnded;

    private bool has_dragged;
    private Vector3 drag_offset;
    private StandardMaterial3D material;
    private ShaderMaterial material_glow;

    public override void _Ready()
    {
        base._Ready();
        InitializeMesh();

        NodeController.Instance.OnNodeCreated += Node_Created;
        NodeController.Instance.OnNodeRemoved += Node_Removed;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        NodeController.Instance.OnNodeCreated -= Node_Created;
        NodeController.Instance.OnNodeRemoved -= Node_Removed;
    }

    protected virtual void InitializeOtherNodes()
    {
        foreach (var node in NodeController.Instance.GetNodes())
        {
            if (node == this) continue;
            Node_Created(node);
        }
    }

    protected virtual void Node_Created(NodeObject node)
    {

    }

    protected virtual void Node_Removed(NodeObject node)
    {
        RemoveRelation(node.NodeName);
    }

    private void InitializeMesh()
    {
        material = Mesh.GetActiveMaterial(0).Duplicate() as StandardMaterial3D;
        Mesh.SetSurfaceOverrideMaterial(0, material);
        Mesh_Select.Hide();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        var fdelta = Convert.ToSingle(delta);
        Process_Relations(fdelta);
    }

    public override void _MouseEnter()
    {
        base._MouseEnter();
        if (IsDestroying) return;
        Hovered = this;
    }

    public override void _MouseExit()
    {
        base._MouseExit();
        if (Hovered == this)
        {
            Hovered = null;
        }
    }

    public void MousePressed(bool pressed, bool ctrl)
    {
        if (IsDestroying) return;

        if (IsPressed != pressed)
        {
            MousePressedChanged(pressed, ctrl);
        }

        IsPressed = pressed;
    }

    protected virtual void MousePressedChanged(bool pressed, bool ctrl)
    {
        if (pressed)
        {
            Handled = this;
            has_dragged = false;
            GlobalPosition = GlobalPosition.Set(y: 1);
        }
        else
        {
            Handled = null;
            GlobalPosition = GlobalPosition.Set(y: 0);

            if (ctrl)
            {
                ToggleSelected();
            }
            else if (!has_dragged)
            {
                SelectionController.Instance.ClearSelection();
                OnClicked?.Invoke();
            }

            if (has_dragged)
            {
                if (IsSelected)
                {
                    SelectionController.Instance.StopDragSelection();
                }
                else
                {
                    DragEnd();
                }
            }
        }
    }

    public void Drag()
    {
        var mouse_position = DraggableCamera.Instance.MouseWorldPosition;

        if (!IsDragging)
        {
            DragStartPosition = GlobalPosition.Set(y: 0);
            drag_offset = DragStartPosition - mouse_position;

            if (!IsSelected)
            {
                OnDragStarted?.Invoke();
            }
        }

        has_dragged = true;
        IsDragging = true;
        GlobalPosition = new Vector3(mouse_position.X, GlobalPosition.Y, mouse_position.Z) + drag_offset;
    }

    public void DragEnd()
    {
        if (!IsDragging) return;
        IsDragging = false;
        DragEndPosition = GlobalPosition;

        if (!IsSelected)
        {
            OnDragEnded?.Invoke();
        }
    }

    public virtual void AddConnection(string id, NodeObject node)
    {
        if (HasConnection(id)) return;
        ConnectedNodes.Add(id, node);
    }

    public void RemoveConnection(string id)
    {
        var node = ConnectedNodes.TryGetValue(id, out var result) ? result : null;
        if (node == null) return;

        ConnectedNodes.Remove(id);
    }

    public bool HasConnection(string id)
    {
        return ConnectedNodes.ContainsKey(id);
    }

    protected void SetColor(Color color)
    {
        material.AlbedoColor = color;
    }

    public virtual void DestroyNode()
    {
        IsDestroying = true;
        QueueFree();
    }

    public void ToggleSelected()
    {
        UndoController.Instance.StartUndoAction($"Node {NodeName} selected");
        SelectionController.Instance.ToggleNode(this);
        UndoController.Instance.EndUndoAction();
    }

    public virtual void SetSelected(bool selected)
    {
        if (IsSelected == selected) return;
        IsSelected = selected;
        Mesh_Select.Visible = selected;
        UndoController.Instance.AddSelectNodeAction(NodeName, selected);
    }

    protected NodeRelation GetOrCreateRelation(NodeObject node)
    {
        var relation = Relations.FirstOrDefault(x => x.Node.NodeName == node.NodeName);
        if (relation == null)
        {
            relation = new NodeRelation
            {
                Node = node
            };
            Relations.Add(relation);
        }

        return relation;
    }

    protected void RemoveRelation(string name)
    {
        var relation = Relations.FirstOrDefault(x => x.Node.NodeName == name);
        if (relation == null) return;

        Relations.Remove(relation);
    }

    private void Process_Relations(float delta)
    {
        if (IsHandled) return;

        var position = GlobalPosition;
        var velocity = Vector3.Zero;
        var iterations = 0;
        foreach (var relation in Relations)
        {
            if (relation.Node.IsHandled) continue;

            var dir = position - relation.Node.GlobalPosition;
            var length = dir.Length();

            if (length < (relation.MinDistance ?? 0))
            {
                var min_distance = relation.MinDistance ?? 0f;
                var ease_range = min_distance - 2f;
                var t = 1f - Mathf.Clamp((length - ease_range) / (min_distance - ease_range), 0f, 1f);
                velocity += dir.Normalized() * t;
                iterations++;
            }
            else if (length > (relation.MaxDistance ?? float.MaxValue))
            {
                var ease_range = 2f;
                var max_distance = relation.MaxDistance ?? 0f;
                var t = Mathf.Clamp((length - max_distance) / ease_range, 0f, 1f);
                velocity += -dir.Normalized() * t;
                iterations++;
            }
        }

        if (iterations > 0)
        {
            velocity *= 1f / iterations;
        }
        GlobalPosition += velocity * 15f * delta;
    }
}
