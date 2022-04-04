using Godot;

namespace Inevitable
{
    public class TutorialScreen : MenuScreen
    {
        private (Label itemName, Label itemDesc, Label goldLabel, PanelContainer panel) tooltip;
		private Tween tween;

        public override void _Ready()
        {
            base._Ready();

			tween = GetNode<Tween>("Tween");

            tooltip.itemName = GetNode<Label>("Content/TooltipPanel/VBoxContainer/ItemName");
            tooltip.itemDesc = GetNode<Label>("Content/TooltipPanel/VBoxContainer/Desc");
            tooltip.goldLabel = GetNode<Label>("Content/TooltipPanel/VBoxContainer/HBoxContainer/GoldLabel");
            tooltip.panel = GetNode<PanelContainer>("Content/TooltipPanel");

            tooltip.panel.RectScale = new Vector2(0, 1);

            foreach (Node child in GetNode("Content/NinePatchRect/InvContainer/InvVBox").GetChildren())
            {
                if (child is ItemButton itemButton)
                {
                    itemButton.Connect("mouse_entered", this, nameof(ItemButtonMouseOver), new Godot.Collections.Array() { itemButton, true });
                    itemButton.Connect("mouse_exited", this, nameof(ItemButtonMouseOver), new Godot.Collections.Array() { itemButton, false });
                }
            }
        }

        private void ItemButtonMouseOver(ItemButton itemButton, bool show)
        {
            if (show)
            {
                tooltip.itemName.Text = itemButton.ItemName;
                tooltip.itemDesc.Text = itemButton.ItemDesc;
                tooltip.goldLabel.Text = $"{itemButton.GoldCost}";

                tween.InterpolateProperty(tooltip.panel, "rect_scale", tooltip.panel.RectScale, Vector2.One, .3f, Tween.TransitionType.Cubic);
                tween.InterpolateProperty(tooltip.panel, "rect_position", tooltip.panel.RectPosition, new Vector2(tooltip.panel.RectPosition.x, Mathf.Min(itemButton.RectGlobalPosition.y, 221f)), .3f, Tween.TransitionType.Elastic);
                tween.Start();
            }
            else
            {
                tween.InterpolateProperty(tooltip.panel, "rect_scale", tooltip.panel.RectScale, new Vector2(0, 1), .29f, Tween.TransitionType.Elastic);
                tween.Start();
            }
        }
    }
}