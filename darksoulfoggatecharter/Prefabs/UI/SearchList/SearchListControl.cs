using Godot;
using System;
using System.Collections.Generic;

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

    private List<Button> items = new();

    public override void _Ready()
    {
        base._Ready();
        ItemButtonTemplate.Hide();
        VisibilityChanged += _VisibilityChanged;
        SearchBar.TextChanged += SearchTextChanged;
        CancelButton.Pressed += Cancel_Pressed;
    }

    public void Clear()
    {
        foreach (var item in items)
        {
            item.QueueFree();
        }
        items.Clear();
    }

    public void AddItem(string text, Action action)
    {
        var button = ItemButtonTemplate.Duplicate() as Button;
        ItemButtonTemplate.GetParent().AddChild(button);
        button.Show();
        button.Text = text;
        button.Pressed += Hide;
        button.Pressed += action;
        items.Add(button);
    }

    public new void GrabFocus()
    {
        SearchBar.GrabFocus();
    }

    private void _VisibilityChanged()
    {
        if (IsVisibleInTree())
        {
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
        foreach (var item in items)
        {
            var text = item.Text.ToLower();
            item.Visible = text.Contains(term);
        }
    }

    private void Cancel_Pressed()
    {
        Hide();
    }
}
