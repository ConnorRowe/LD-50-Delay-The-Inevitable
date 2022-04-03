using Godot;

namespace Inevitable
{
    public class World : Node2D
    {
        public static float GameSpeedModifier = 1f;

        private static PackedScene GoldChangeEffectScene = GD.Load<PackedScene>("res://scenes/GoldChangeEffect.tscn");

        private Pet pet;
        private AnimatedSprite petCursor;
        private ShaderMaterial skyShaderMat;

        private Label ageLabel;
        private Label goldLabel;
        private (Label itemName, Label itemDesc, Label goldLabel, PanelContainer panel) tooltip;

        private float time = 0f;
        private int daysPassed = 0;
        private Tween tween;

        public int Money { get; set; } = 0;

        public override void _Ready()
        {
            base._Ready();

            pet = GetNode<Pet>("Pet");
            petCursor = GetNode<AnimatedSprite>("PetCursor");
            skyShaderMat = (ShaderMaterial)GetNode<Sprite>("Sky").Material;
            ageLabel = GetNode<Label>("AgeLabel");
            goldLabel = GetNode<Label>("GoldCoin/GoldLabel");
            tween = GetNode<Tween>("Tween");

            pet.Connect(nameof(Pet.MouseOver), this, nameof(MouseOverPet));

            tooltip.itemName = GetNode<Label>("TooltipPanel/VBoxContainer/ItemName");
            tooltip.itemDesc = GetNode<Label>("TooltipPanel/VBoxContainer/Desc");
            tooltip.goldLabel = GetNode<Label>("TooltipPanel/VBoxContainer/HBoxContainer/GoldLabel");
            tooltip.panel = GetNode<PanelContainer>("TooltipPanel");

            tooltip.panel.RectScale = new Vector2(0, 1);

            AddMoney(100);

            SetGameSpeedModifier(10f);

            foreach (Node child in GetNode("NinePatchRect/InvContainer/InvVBox").GetChildren())
            {
                if (child is ItemButton itemButton)
                {
                    itemButton.Connect("mouse_entered", this, nameof(ItemButtonMouseOver), new Godot.Collections.Array() { itemButton, true });
                    itemButton.Connect("mouse_exited", this, nameof(ItemButtonMouseOver), new Godot.Collections.Array() { itemButton, false });
                }
            }
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            time += delta * .01f * GameSpeedModifier;
            if (time > 1f)
            {
                time--;
                daysPassed++;

                AddMoney(50);
            }

            if (daysPassed == 0)
            {
                ageLabel.Text = $"Age: {Mathf.RoundToInt(time * 24f)} hours";
            }
            else
            {
                ageLabel.Text = $"Age: {daysPassed} days";
            }

            skyShaderMat.SetShaderParam("time", time);

            if (petCursor.Visible)
            {
                petCursor.Position = GetLocalMousePosition();
            }
        }

        public override void _Input(InputEvent evt)
        {
            base._Input(evt);

            if (petCursor.Visible && evt is InputEventMouseButton emb && emb.ButtonIndex == (int)ButtonList.Left)
            {
                if (emb.Pressed)
                {
                    petCursor.Play("default");
                    pet.BeingStroked = true;
                }
                else
                {
                    petCursor.Stop();
                    petCursor.Frame = 0;
                    pet.BeingStroked = false;
                }
            }
        }

        private void MouseOverPet(bool isOver)
        {
            petCursor.Visible = isOver;

            Input.SetMouseMode(isOver ? Input.MouseMode.Hidden : Input.MouseMode.Visible);

            if (!isOver)
            {
                pet.BeingStroked = false;
            }
        }

        private void SetGameSpeedModifier(float speed)
        {
            GameSpeedModifier = speed;

            skyShaderMat.SetShaderParam("game_speed", speed);
        }

        public void AddMoney(int amount)
        {
            Money += amount;

            GoldChangeEffect goldChangeEffect = GoldChangeEffectScene.Instance<GoldChangeEffect>();
            goldLabel.AddChild(goldChangeEffect);
            goldChangeEffect.SetGoldValue(amount);
            goldChangeEffect.RectPosition = new Vector2(-11, 0);

            goldLabel.Text = $"{Money}";
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