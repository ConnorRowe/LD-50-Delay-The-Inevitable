using Godot;

namespace Inevitable
{
    public enum PetState
    {
        Idle,
        Roam,
        Poo,
        Sick,
        Dying,
        Dead,
        Bloat,
        Decay,
        Dry,
        Skeletal
    }

    public class Pet : Node2D
    {
        [Signal]
        public delegate void MouseOver(bool isOver);

        private static Texture happyTex = GD.Load<Texture>("res://textures/happy.png");
        private static Texture neutralTex = GD.Load<Texture>("res://textures/neutral.png");
        private static Texture sadTex = GD.Load<Texture>("res://textures/sad.png");
        private static RandomNumberGenerator rng = new RandomNumberGenerator();
        private static readonly Vector2 middle = new Vector2(225f, 201.5f);
        private const int minX = 18;
        private const int maxX = 431;
        private const int minY = 151;
        private const int maxY = 252;

        static Pet()
        {
            rng.Randomize();
        }

        public float Mood { get; set; } = .5f;
        public float Hunger { get; set; } = .5f;
        public float Hygiene { get; set; } = .5f;
        public bool BeingStroked { get; set; } = false;

        public PetState CurrentState { get; set; } = PetState.Roam;

        private Timer behaviourTimer;
        private Sprite sprite;
        private Label debug;

        private Vector2 goalPosition = middle;

        public override void _Ready()
        {
            base._Ready();

            behaviourTimer = GetNode<Timer>("BehaviourTimer");
            sprite = GetNode<Sprite>("Sprite");
            debug = GetNode<Label>("debug");

            GetNode("Area2D").Connect("mouse_entered", this, "emit_signal", new Godot.Collections.Array() { nameof(MouseOver), true });
            GetNode("Area2D").Connect("mouse_exited", this, "emit_signal", new Godot.Collections.Array() { nameof(MouseOver), false });
            behaviourTimer.Connect("timeout", this, nameof(BehaviourTimeout));

            BehaviourTimeout();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            debug.Text = $"{CurrentState.ToString()}";

            if (BeingStroked)
            {
                AddMood(.03f * delta * World.GameSpeedModifier);
            }

            // Passively deplete mood
            AddMood(-.01f * delta * World.GameSpeedModifier);

            switch (CurrentState)
            {
                case PetState.Idle:
                    break;
                case PetState.Roam:
                    Position = Position.LinearInterpolate(goalPosition, delta * .5f * World.GameSpeedModifier);
                    break;
                case PetState.Poo:
                    break;
                case PetState.Sick:
                    break;
                case PetState.Dying:
                    break;
                case PetState.Dead:
                    break;
                case PetState.Bloat:
                    break;
                case PetState.Decay:
                    break;
                case PetState.Dry:
                    break;
                case PetState.Skeletal:
                    break;
            }
        }

        private void BehaviourTimeout()
        {
            switch (CurrentState)
            {
                case PetState.Idle:

                    if (rng.Randf() > .25f)
                        CurrentState = PetState.Roam;
                    break;
                case PetState.Roam:

                    goalPosition = new Vector2(rng.RandfRange(minX, maxX), rng.RandfRange(minY, maxY));

                    if (rng.Randf() > .5f)
                        CurrentState = PetState.Idle;
                    break;
                case PetState.Poo:
                    break;
                case PetState.Sick:
                    break;
                case PetState.Dying:
                    break;
                case PetState.Dead:
                    break;
                case PetState.Bloat:
                    break;
                case PetState.Decay:
                    break;
                case PetState.Dry:
                    break;
                case PetState.Skeletal:
                    break;
            }
        }

        public void AddMood(float value)
        {
            Mood += value;

            Mood = Mathf.Clamp(Mood, 0f, 1f);

            if (Mood < .333f)
            {
                sprite.Texture = sadTex;
            }
            else if (Mood < .666f)
            {
                sprite.Texture = neutralTex;
            }
            else
            {
                sprite.Texture = happyTex;
            }
        }
    }
}