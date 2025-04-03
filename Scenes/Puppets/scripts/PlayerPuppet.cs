using Godot;

public partial class PlayerPuppet : PuppetTemplate
{
    public static new readonly string LoadPath = "res://Scenes/Puppets/PlayerPuppet.tscn";
    public PlayerStats Stats;

    public static Color NoteColor = new Color(1, 0.43f, 0.26f);

    public override void _Ready()
    {
        Stats = StageProducer.PlayerStats ?? new PlayerStats();

        CurrentHealth = Stats.CurrentHealth;
        MaxHealth = Stats.MaxHealth;

        UniqName = "Player";

        base._Ready();
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        Stats.CurrentHealth = CurrentHealth;
    }

    public override void Heal(int amount)
    {
        base.Heal(amount);
        Stats.CurrentHealth = CurrentHealth;
    }
}
