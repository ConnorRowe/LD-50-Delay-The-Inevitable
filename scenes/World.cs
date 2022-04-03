using Godot;

namespace Inevitable
{
    public class World : Node2D
    {
        public static float GameSpeedModifier = 1f;
        public static RandomNumberGenerator RNG = new RandomNumberGenerator();
        public static readonly Vector2 PetAreaMidPoint = new Vector2(225f, 201.5f);
        public const int minX = 18;
        public const int maxX = 431;
        public const int minY = 151;
        public const int maxY = 252;

        static World()
        {
            RNG.Randomize();
        }

        private static PackedScene GoldChangeEffectScene = GD.Load<PackedScene>("res://scenes/GoldChangeEffect.tscn");
        private static Texture pooTex = GD.Load<Texture>("res://textures/poo.png");
        private static PackedScene SmokePuffScene = GD.Load<PackedScene>("res://scenes/SmokePuff.tscn");

        private Pet pet;
        private AnimatedSprite petCursor;
        private ShaderMaterial skyShaderMat;

        private Label ageLabel;
        private Label goldLabel;
        private (Label itemName, Label itemDesc, Label goldLabel, PanelContainer panel) tooltip;
        private TextureProgress hungerBar;
        private TextureProgress hygieneBar;
        private TextureProgress healthBar;
        private Node2D poos;
        private Label petStatusLabel;

        private float time = 0f;
        private int daysPassed = 0;
        private Tween tween;

        public int Money { get; set; } = 0;
        public int DaysPassed { get { return daysPassed; } }

        public override void _Ready()
        {
            base._Ready();

            pet = GetNode<Pet>("Pet");
            petCursor = GetNode<AnimatedSprite>("PetCursor");
            skyShaderMat = (ShaderMaterial)GetNode<Sprite>("Sky").Material;
            ageLabel = GetNode<Label>("AgeLabel");
            goldLabel = GetNode<Label>("GoldCoin/GoldLabel");
            tween = GetNode<Tween>("Tween");
            hungerBar = GetNode<TextureProgress>("HungerBar");
            hygieneBar = GetNode<TextureProgress>("HygieneBar");
            healthBar = GetNode<TextureProgress>("HealthBar");
            poos = GetNode<Node2D>("Poos");
            petStatusLabel = GetNode<Label>("YourPetIsLabel");

            pet.Connect(nameof(Pet.MouseOver), this, nameof(MouseOverPet));
            GetNode("UpdatePetStatusTimer").Connect("timeout", this, nameof(UpdatePetStatus));

            tooltip.itemName = GetNode<Label>("TooltipPanel/VBoxContainer/ItemName");
            tooltip.itemDesc = GetNode<Label>("TooltipPanel/VBoxContainer/Desc");
            tooltip.goldLabel = GetNode<Label>("TooltipPanel/VBoxContainer/HBoxContainer/GoldLabel");
            tooltip.panel = GetNode<PanelContainer>("TooltipPanel");

            tooltip.panel.RectScale = new Vector2(0, 1);

            AddMoney(200);

            SetGameSpeedModifier(7f);

            foreach (Node child in GetNode("NinePatchRect/InvContainer/InvVBox").GetChildren())
            {
                if (child is ItemButton itemButton)
                {
                    itemButton.Connect("mouse_entered", this, nameof(ItemButtonMouseOver), new Godot.Collections.Array() { itemButton, true });
                    itemButton.Connect("mouse_exited", this, nameof(ItemButtonMouseOver), new Godot.Collections.Array() { itemButton, false });
                    itemButton.GetNode("TextureButton").Connect("pressed", this, nameof(ItemButtonPressed), new Godot.Collections.Array() { itemButton });
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

                ageLabel.Text = $"Age: {daysPassed} days";

                if (pet.CurrentState == PetState.Idle || pet.CurrentState == PetState.Roam || pet.CurrentState == PetState.Sick)
                    AddPoo();

                if (daysPassed >= 14)
                    pet.IsOld = true;
            }

            if (daysPassed == 0)
            {
                ageLabel.Text = $"Age: {Mathf.RoundToInt(time * 24f)} hours";
            }

            skyShaderMat.SetShaderParam("time", time);

            if (petCursor.Visible)
            {
                petCursor.Position = GetLocalMousePosition();
            }

            hungerBar.Value = pet.Hunger;
            hygieneBar.Value = pet.Hygiene;
            healthBar.Value = pet.Health;
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

        public void ItemButtonPressed(ItemButton itemButton)
        {
            if (itemButton.GoldCost <= Money)
            {
                AddMoney(-itemButton.GoldCost);

                switch (itemButton.ItemName)
                {
                    case "Treat":
                        pet.AddMood(.8f);
                        pet.AddHunger(.1f);
                        break;
                    case "Meal":
                        pet.AddHunger(1f);
                        break;
                    case "Cleanup":
                        RemovePoo();
                        break;
                    case "Medicine":
                        pet.CureSickness();
                        break;
                }
            }
        }

        public void AddPoo()
        {
            Sprite newPoo = new Sprite();
            newPoo.Texture = pooTex;
            poos.AddChild(newPoo);

            Vector2 dir = new Vector2(RNG.Randf(), RNG.Randf()).Normalized();
            Vector2 pos = pet.Position + (dir * RNG.RandfRange(28f, 48f));
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            newPoo.Position = pos;
            newPoo.ZIndex = (int)pos.y;
            newPoo.Offset = new Vector2(0, -16);
            newPoo.FlipH = RNG.Randf() > .5f;

            MakeSmokePuff(pos, Colors.Brown);

            pet.UpdateHygiene();
        }

        public void RemovePoo()
        {
            int count = poos.GetChildCount();
            if (count == 0)
                return;

            var poo = (Node2D)poos.GetChild(count - 1);
            poos.RemoveChild(poo);
            poo.QueueFree();

            MakeSmokePuff(poo.Position);

            pet.UpdateHygiene();
        }

        public int GetPooCount()
        {
            return poos.GetChildCount();
        }

        public void MakeSmokePuff(Vector2 pos)
        {
            MakeSmokePuff(pos, Colors.White);
        }

        public void MakeSmokePuff(Vector2 pos, Color tint)
        {
            Particles2D newSmokePuff = SmokePuffScene.Instance<Particles2D>();
            newSmokePuff.Modulate = tint;
            AddChild(newSmokePuff);
            newSmokePuff.Position = pos;
            newSmokePuff.Emitting = true;
            GetTree().CreateTimer(.8f).Connect("timeout", newSmokePuff, "queue_free");
        }

        private void UpdatePetStatus()
        {
            string status = "";

            switch (pet.CurrentState)
            {
                case PetState.Idle:
                case PetState.Roam:
                    if (Mathf.IsEqualApprox(pet.Hunger, 0))
                    {
                        status = "Starving";
                    }
                    else if (Mathf.IsEqualApprox(pet.Mood, 0))
                    {
                        status = "Depressed";
                    }
                    else if (pet.Hunger < .25f)
                    {
                        status = "Hungry";
                    }
                    else if (pet.Mood < .33f)
                    {
                        status = "Sad";
                    }
                    else if (pet.Mood > .66f)
                    {
                        status = "Happy";
                    }
                    else if (pet.Hygiene <= .25f)
                    {
                        status = "Filthy";
                    }
                    else
                    {
                        status = "Okay";
                    }

                    if (pet.IsOld)
                        status = "Old and " + status;

                    break;
                case PetState.Sick:
                    status = $"{(pet.IsOld ? "Old and " : "")}Sick";
                    break;
                case PetState.Dying:
                    status = "Dying";
                    break;
                case PetState.Dead:
                    status = "Dead";
                    break;
                case PetState.Bloat:
                case PetState.Decay:
                    status = "Rotting";
                    break;
                case PetState.Dry:
                case PetState.Skeletal:
                    status = "Gone";
                    break;
            }
            petStatusLabel.Text = $"Your pet is: {status}";
        }
    }
}