using Godot;

namespace Inevitable
{
    public class GoldChangeEffect : Label
    {
        private float lifetime = 2f;

        public override void _Ready()
        {
            base._Ready();

            ShowBehindParent = true;
        }

        public override void _Process(float delta)
        {
            lifetime -= delta;

            RectPosition = RectPosition + new Vector2(0, -10f * delta);

            Modulate = new Color(1f, 1f, 1f, lifetime / 2f);

            if (lifetime <= 0f)
                QueueFree();
        }

        public void SetGoldValue(int value)
        {
            Text = $"{(value >= 0 ? "+" : "-")} {value}";

            Set("custom_colors/font_color", value >= 0 ? new Color("63c74d") : new Color("9e2835"));
        }
    }
}