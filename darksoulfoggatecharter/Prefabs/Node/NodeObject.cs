using Godot;
using System;
using System.Collections.Generic;

public partial class NodeObject : Area3D
{
    [Export]
    public MeshInstance3D Mesh;

    [Export]
    public MeshInstance3D Mesh_Glow;

    [Export]
    public Label3D Label;

    public virtual string NodeName => string.Empty;

    public Dictionary<string, NodeObject> ConnectedNodes = new();

    public bool IsFullyConnected => ConnectedNodes.Count >= 2;
    public Vector3 DragStartPosition { get; private set; }
    public Vector3 DragEndPosition { get; private set; }

    public static NodeObject Handled;

    public event Action OnRightClick;
    public event Action OnDragStarted;
    public event Action OnDragEnded;

    protected bool IsHandled => Handled == this;
    protected bool HasMouse { get; private set; }
    protected bool MouseDown { get; private set; }
    protected bool Dragging { get; private set; }

    private Vector3 drag_offset;
    private StandardMaterial3D material;
    private ShaderMaterial material_glow;

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
        else if (e is InputEventMouseMotion motion && MouseDown)
        {
            Drag();
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
            GlobalPosition = GlobalPosition.Set(y: 1);
        }
        else
        {
            Handled = null;
            GlobalPosition = GlobalPosition.Set(y: 0);
            DragEnd();
        }
    }

    protected void Drag()
    {
        if (!Dragging)
        {
            DragStartPosition = GlobalPosition.Set(y: 0);
            OnDragStarted?.Invoke();
        }

        Dragging = true;
        var position = DraggableCamera.Instance.MouseWorldPosition;
        GlobalPosition = new Vector3(position.X, GlobalPosition.Y, position.Z);
    }

    protected void DragEnd()
    {
        Dragging = false;
        DragEndPosition = GlobalPosition;
        OnDragEnded?.Invoke();
    }

    public void AddConnection(string id, NodeObject node)
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

    private void InitializeMesh()
    {
        material = Mesh.GetActiveMaterial(0).Duplicate() as StandardMaterial3D;
        Mesh.SetSurfaceOverrideMaterial(0, material);

        material_glow = Mesh_Glow.GetActiveMaterial(0).Duplicate() as ShaderMaterial;
        Mesh_Glow.SetSurfaceOverrideMaterial(0, material_glow);
    }

    protected void SetColor(Color color)
    {
        material.AlbedoColor = color;
    }

    protected void SetGlow(Color color)
    {
        material_glow.SetShaderParameter("color_circle", color);
    }

    public void DestroyNode()
    {
        QueueFree();
    }
}
