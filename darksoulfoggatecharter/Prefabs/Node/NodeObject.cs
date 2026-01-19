using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class NodeObject : Area3D
{
    [Export]
    public MeshInstance3D Mesh;

    [Export]
    public Label3D Label;

    public virtual string NodeName => string.Empty;

    public Dictionary<string, NodeObject> ConnectedNodes = new();

    public bool IsFullyConnected => ConnectedNodes.Count >= 2;

    public static NodeObject Handled;

    public event Action OnRightClick;

    protected bool IsHandled => Handled == this;
    protected bool HasMouse { get; private set; }
    protected bool Dragging { get; private set; }

    private Vector3 drag_offset;
    private StandardMaterial3D material;

    public override void _Ready()
    {
        base._Ready();
        InitializeMesh();
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
                MousePressed(button.Pressed);
                GetViewport().SetInputAsHandled();
            }
            else if (button.ButtonIndex == MouseButton.Right)
            {
                OnRightClick?.Invoke();
                GetViewport().SetInputAsHandled();
            }
        }
        else if (e is InputEventMouseMotion motion && Dragging)
        {
            var position = DraggableCamera.Instance.MouseWorldPosition;
            GlobalPosition = new Vector3(position.X, GlobalPosition.Y, position.Z);
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
        if (Dragging != pressed)
        {
            MousePressedChanged(pressed);
        }

        Dragging = pressed;
    }

    protected virtual void MousePressedChanged(bool pressed)
    {
        if (pressed)
        {
            Handled = this;
            GlobalPosition = GlobalPosition.Set(y: 1);
        }
        else
        {
            Handled = null;
            GlobalPosition = GlobalPosition.Set(y: 0);
        }
    }

    public void AddConnection(string id, NodeObject node)
    {
        if (IsConnectedTo(node)) return;
        ConnectedNodes.Add(id, node);
    }

    public void RemoveConnection(string id)
    {
        var node = ConnectedNodes.TryGetValue(id, out var result) ? result : null;
        if (node == null) return;

        ConnectedNodes.Remove(id);
    }

    public bool IsConnectedTo(NodeObject node)
    {
        return ConnectedNodes.Values.Contains(node);
    }

    private void InitializeMesh()
    {
        material = Mesh.GetActiveMaterial(0).Duplicate() as StandardMaterial3D;
        Mesh.SetSurfaceOverrideMaterial(0, material);
    }

    protected void SetColor(Color color)
    {
        material.AlbedoColor = color;
    }
}
