using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class NodeObject : Area3D
{
    public virtual string NodeName => string.Empty;
    public virtual string NodeArea => string.Empty;
    public virtual bool IsGroup => false;

    public Dictionary<string, NodeObject> ConnectedNodes = new();

    public static NodeObject Handled { get; private set; }
    public static NodeObject Hovered { get; private set; }
    public bool IsFullyConnected => IsGroup || ConnectedNodes.Count >= 2;
    public Vector3 DragStartPosition { get; private set; }
    public Vector3 DragEndPosition { get; private set; }
    protected bool IsHandled => Handled == this;
    protected bool IsHovered => Hovered == this;
    protected bool IsPressed { get; private set; }
    protected bool IsDragging { get; private set; }
    protected bool IsDestroying { get; set; }
    protected bool IsDisplayingValid { get; set; } = true;
    public bool IsSelected { get; private set; }
    protected List<NodeRelation> Relations { get; private set; } = new();
    protected Node3D ImageMapTarget { get; set; }

    public event Action OnClicked;
    public event Action OnDragStarted;
    public event Action OnDragEnded;

    private bool has_dragged;
    private float time_handled;
    private Vector3 drag_offset;

    public override void _Ready()
    {
        base._Ready();
        NodeController.Instance.OnNodeCreated += Node_Created;
        NodeController.Instance.OnNodeRemoved += Node_Removed;
        SearchController.Instance.OnSearchChanged += Search_Changed;
        SearchController.Instance.OnOpenGateToggled += OpenGate_Toggled;
        ImageMapController.Instance.OnImageMapModeChanged += ImageMapMode_Changed;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        NodeController.Instance.OnNodeCreated -= Node_Created;
        NodeController.Instance.OnNodeRemoved -= Node_Removed;
        SearchController.Instance.OnSearchChanged -= Search_Changed;
        SearchController.Instance.OnOpenGateToggled -= OpenGate_Toggled;
        ImageMapController.Instance.OnImageMapModeChanged -= ImageMapMode_Changed;
    }

    public virtual void Initialize(string name)
    {
        InitializeOtherNodes();

        if (ShouldDisplayValid())
        {
            LoadColorPalette();
        }
        else
        {
            DisplayValid(false);
        }
    }

    protected virtual void InitializeOtherNodes()
    {
        foreach (var node in NodeController.Instance.GetNodes())
        {
            if (node == this) continue;
            Node_Created(node);
        }
    }

    private void Search_Changed(string term)
    {
        var valid = string.IsNullOrEmpty(term) || NodeName.ToLower().Contains(term.Trim().ToLower());
        Search_Changed(valid);
    }

    protected virtual void Search_Changed(bool valid)
    {
        UpdateValidDisplay();
    }

    protected virtual void OpenGate_Toggled(bool toggled)
    {
        UpdateValidDisplay();
    }

    protected virtual void ImageMapMode_Changed(bool enabled)
    {
    }

    protected virtual void Node_Created(NodeObject node)
    {
    }

    public virtual void Connection_Changed()
    {
        UpdateValidDisplay();
    }

    protected virtual void Node_Removed(NodeObject node)
    {
        RemoveRelation(node.NodeName);
    }

    private void UpdateValidDisplay()
    {
        var valid = ShouldDisplayValid();
        if (valid == IsDisplayingValid) return;
        DisplayValid(valid);
    }

    private bool ShouldDisplayValid()
    {
        var term = SearchController.Instance.CurrentSearchTerm;
        var open_gate = SearchController.Instance.OpenGateToggled;
        var valid_search = string.IsNullOrEmpty(term) || NodeName.ToLower().Contains(term.Trim().ToLower());
        var valid_open_gate = !open_gate || !IsFullyConnected;
        return valid_search && valid_open_gate;
    }

    protected virtual void DisplayValid(bool valid)
    {
        IsDisplayingValid = valid;
    }

    protected void LoadColorPalette()
    {
        var info = ColorPaletteController.Instance.GetInfo(NodeArea);
        LoadColorPalette(info);
    }

    protected virtual void LoadGreyedOutColorPalette()
    {
    }

    protected virtual void LoadColorPalette(ColorPaletteInfo info)
    {
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        var fdelta = Convert.ToSingle(delta);

        if (ImageMapController.Instance.ImageMapModeEnabled)
        {
            Process_ImageMapMode(fdelta);
        }
        else
        {
            Process_Relations(fdelta);
        }
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
            time_handled = GameTime.Time;
            GlobalPosition = GlobalPosition.Set(y: 1);
        }
        else
        {
            Handled = null;
            GlobalPosition = GlobalPosition.Set(y: 0);

            var duration_handled = GameTime.Time - time_handled;

            if (ctrl)
            {
                ToggleSelected();
            }
            else if (!has_dragged || duration_handled < 0.2f)
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

    protected virtual void SetColor(Color color)
    {
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
        UndoController.Instance.AddSelectNodeAction(NodeName, selected);
    }

    public void SetTarget(Node3D target)
    {
        ImageMapTarget = target;
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

    private void Process_ImageMapMode(float delta)
    {
        if (ImageMapTarget == null) return;
        if (IsHandled) return;

        GlobalPosition = GlobalPosition.Lerp(ImageMapTarget.GlobalPosition.Set(y: GlobalPosition.Y), 20f * delta);
    }
}
