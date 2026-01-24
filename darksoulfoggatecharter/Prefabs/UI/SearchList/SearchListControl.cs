using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SearchListControl : MarginContainer
{
    [Export]
    public Label TitleLabel;

    [Export]
    public LineEdit SearchBar;

    [Export]
    public Button ItemButtonTemplate;

    [Export]
    public Button CancelButton;

    public string Title
    {
        get => TitleLabel.Text;
        set => TitleLabel.Text = value;
    }

    private class ButtonMap
    {
        public SearchListItemButton Button { get; set; }
        public GateNode Gate { get; set; }
    }

    private event Action<GateNode> on_gate_selected;
    private Dictionary<string, GateNode> valid_gates = new();
    private Dictionary<string, ButtonMap> maps = new();

    public override void _Ready()
    {
        base._Ready();
        ItemButtonTemplate.Hide();
        VisibilityChanged += _VisibilityChanged;
        SearchBar.TextChanged += SearchTextChanged;
        CancelButton.Pressed += Cancel_Pressed;

        InitializeGates();
    }

    public void Clear()
    {
        on_gate_selected = null;
        valid_gates.Clear();
    }

    private void InitializeGates()
    {
        foreach (var gate in GateController.Instance.Gates.Values)
        {
            CreateButton(gate);
        }
    }

    private SearchListItemButton CreateButton(GateNode gate)
    {
        var button = ItemButtonTemplate.Duplicate() as SearchListItemButton;
        ItemButtonTemplate.GetParent().AddChild(button);
        button.Show();
        button.SetGate(gate);
        button.Pressed += () => Button_Pressed(button, gate);
        maps.Add(gate.Name, new ButtonMap
        {
            Button = button,
            Gate = gate,
        });
        return button;
    }

    public void SetAction(Action<GateNode> action)
    {
        on_gate_selected = action;
    }

    public void SetGates(IEnumerable<GateNode> gates)
    {
        valid_gates = gates.ToDictionary(x => x.Name);
    }

    public new void GrabFocus()
    {
        SearchBar.GrabFocus();
    }

    private void _VisibilityChanged()
    {
        if (IsVisibleInTree())
        {
            SearchBar.Text = string.Empty;
            UpdateButtons();
            GrabFocus();
        }
        else
        {

        }
    }

    private void SearchTextChanged(string newText)
    {
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        var term = SearchBar.Text.ToLower();
        foreach (var kvp in maps)
        {
            var is_text = kvp.Key.ToLower().Contains(term);
            var is_area = kvp.Value.Gate.Area.ToLower().Contains(term);
            var is_gate = valid_gates.ContainsKey(kvp.Key);
            kvp.Value.Button.Visible = is_gate && (is_text || is_area);
        }
    }

    private void Button_Pressed(SearchListItemButton button, GateNode gate)
    {
        on_gate_selected?.Invoke(gate);
        on_gate_selected = null;
        Hide();
    }

    private void Cancel_Pressed()
    {
        Hide();
    }
}
