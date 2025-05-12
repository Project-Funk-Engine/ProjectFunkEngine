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

    public void Display(int cost, Texture2D texture, string name, bool focusHandling = false)
    {
        DisplayButton.Display(texture, name, focusHandling);
        Price = cost;
        Cost.Text = cost.ToString();
    }
}
