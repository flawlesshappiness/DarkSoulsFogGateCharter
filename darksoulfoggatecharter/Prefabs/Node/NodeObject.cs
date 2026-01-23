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

    public bool IsFullyConnected => ConnectedNodes.Count >= 2;
    public Vector3 DragStartPosition { get; private set; }
    public Vector3 DragEndPosition { get; private set; }

    public static NodeObject Handled;

    public event Action OnClicked;
    public event Action OnDragStarted;
    public event Action OnDragEnded;

    protected bool IsHandled => Handled == this;
    protected bool HasMouse { get; private set; }
    protected bool MouseDown { get; private set; }
    protected bool Dragging { get; private set; }
    public bool Selected { get; private set; }

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

    public override void _UnhandledInput(InputEvent e)
    {
        base._UnhandledInput(e);

        var can_input = HasMouse || IsHandled;
        if (!can_input) return;
        if (e is InputEventMouseButton button)
        {
            if (button.ButtonIndex == MouseButton.Left)
            {
                if (PlayerInput.Select.Held)
                {
                    if (button.Pressed)
                    {
                        UndoController.Instance.StartUndoAction();
                        SelectionController.Instance.ToggleNode(this);
                        UndoController.Instance.EndUndoAction();
                        GetViewport().SetInputAsHandled();
                    }
                }
                else
                {
                    MousePressed(button.Pressed);
                    GetViewport().SetInputAsHandled();

                    if (!button.Pressed && !has_dragged)
                    {
                        SelectionController.Instance.ClearSelection();
                        OnClicked?.Invoke();
                    }
                }
            }
        }
        else if (e is InputEventMouseMotion motion && MouseDown)
        {
            if (Selected)
            {
                SelectionController.Instance.DragSelection();
            }
            else
            {
                SelectionController.Instance.ClearSelection();
                Drag();
            }

            GetViewport().SetInputAsHandled();
        }
    }

    public override void _MouseEnter()
    {
        base._MouseEnter();
        HasMouse = true;
    }

    public override void _MouseExit()
    {
        base._MouseExit();
        HasMouse = false;
    }

    private void MousePressed(bool pressed)
    {
        if (MouseDown != pressed)
        {
            MousePressedChanged(pressed);
        }

        MouseDown = pressed;
    }

    protected virtual void MousePressedChanged(bool pressed)
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

            if (Selected)
            {
                SelectionController.Instance.DragEndSelection();
            }
            else
            {
                DragEnd();
            }
        }
    }

    public void Drag()
    {
        var mouse_position = DraggableCamera.Instance.MouseWorldPosition;

        if (!Dragging)
        {
            DragStartPosition = GlobalPosition.Set(y: 0);
            drag_offset = DragStartPosition - mouse_position;

            if (!Selected)
            {
                OnDragStarted?.Invoke();
            }
        }

        has_dragged = true;
        Dragging = true;
        GlobalPosition = new Vector3(mouse_position.X, GlobalPosition.Y, mouse_position.Z) + drag_offset;
    }

    public void DragEnd()
    {
        if (!Dragging) return;
        Dragging = false;
        DragEndPosition = GlobalPosition;

        if (!Selected)
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

    public virtual void SetSelected(bool selected)
    {
        if (Selected == selected) return;
        Selected = selected;
        Mesh_Select.Visible = selected;
        UndoController.Instance.AddSelectNodeAction(NodeName, selected);
    }
}
