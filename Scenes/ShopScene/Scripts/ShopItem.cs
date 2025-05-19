using System;
using Godot;

public partial class ShopItem : VBoxContainer
{
    public static readonly string LoadPath = "res://Scenes/ShopScene/ShopItem.tscn";

    [Export]
    public DisplayButton DisplayButton;

    [Export]
    public Label Cost;
    public int Price;
    public int BaseCost;

    public void Display(
        int baseCost,
        int cost,
        Texture2D texture,
        string name,
        bool focusHandling = false
    )
    {
        DisplayButton.Display(texture, name, focusHandling);
        BaseCost = baseCost;
        Price = cost;
        Cost.Text = cost.ToString();
    }

    public void UpdateCost(int cost)
    {
        Price = cost;
        Cost.Text = cost.ToString();
    }
}
