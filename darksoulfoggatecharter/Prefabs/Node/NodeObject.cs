using Godot;
using System;
using System.Collections.Generic;

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
    public bool IsSelected { get; private set; }

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
    }

    private void InitializeMesh()
    {
        material = Mesh.GetActiveMaterial(0).Duplicate() as StandardMaterial3D;
        Mesh.SetSurfaceOverrideMaterial(0, material);
        Mesh_Select.Hide();
    }

    public override void _MouseEnter()
    {
        base._MouseEnter();
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
}
