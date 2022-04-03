using Godot;

namespace Inevitable
{
    public class ItemButton : PanelContainer
    {
        [Export]
        public int GoldCost = 0;
        [Export]
        public string ItemName = "";
        [Export]
        public string ItemDesc = "";

        public override void _Ready()
        {
            base._Ready();
        }
    }
}