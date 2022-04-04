using Godot;

namespace Inevitable
{
    public enum PetState
    {
        Idle,
        Roam,
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
        private static Texture sickTex = GD.Load<Texture>("res://textures/sick.png");
        private static Texture dyingTex = GD.Load<Texture>("res://textures/dying.png");
        private static Texture deadTex = GD.Load<Texture>("res://textures/dead.png");
        private static Texture decayTex = GD.Load<Texture>("res://textures/decay.png");
        private static Texture advDecayTex = GD.Load<Texture>("res://textures/adv_decay.png");
        private static Texture dryTex = GD.Load<Texture>("res://textures/dry.png");
        private static Texture skeletonTex = GD.Load<Texture>("res://textures/skeleton.png");
        private static Texture graveTex = GD.Load<Texture>("res://textures/grave.png");

        public float Mood { get; set; } = 1f;
        public float Hunger { get; set; } = 1f;
        public float Hygiene { get; set; } = 1f;
        public bool BeingStroked { get; set; } = false;
        public bool IsOld { get; set; } = false;
        public float Health { get { return health; } }

        private float health = 2f;
        private float countdown = 0f;

        public PetState CurrentState { get; set; } = PetState.Roam;

        private Timer behaviourTimer;
        private Sprite sprite;
        private Label debug;
        private Vector2 goalPosition = World.PetAreaMidPoint;
        private World world;
        private Particles2D hearts;
        private Particles2D flies;
        private Particles2D maggots;


        public override void _Ready()
        {
            base._Ready();

            world = (World)Owner;
            behaviourTimer = GetNode<Timer>("BehaviourTimer");
            sprite = GetNode<Sprite>("Sprite");
            debug = GetNode<Label>("debug");
            hearts = GetNode<Particles2D>("Hearts");
            flies = GetNode<Particles2D>("Flies");
            maggots = GetNode<Particles2D>("Maggots");

            GetNode("Area2D").Connect("mouse_entered", this, "emit_signal", new Godot.Collections.Array() { nameof(MouseOver), true });
            GetNode("Area2D").Connect("mouse_exited", this, "emit_signal", new Godot.Collections.Array() { nameof(MouseOver), false });
            behaviourTimer.Connect("timeout", this, nameof(BehaviourTimeout));

            BehaviourTimeout();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            debug.Text = $"{CurrentState.ToString()}\nhealth={health}";

            if (BeingStroked)
            {
                AddMood(.03f * delta * World.GameSpeedModifier);
            }

            hearts.Emitting = BeingStroked;

            // Passively deplete mood & hunger
            AddMood(-.005f * delta * World.GameSpeedModifier);
            AddHunger(-.01f * delta * World.GameSpeedModifier);

            // Lower health and mood with bad hygiene
            if (Hygiene <= .25f)
            {
                // The lower the hygiene, the sadder the pet gets
                float hygieneScale = 1f - (Hygiene * 4f);

                AddMood(-.05f * delta * hygieneScale * World.GameSpeedModifier);
                health -= .02f * delta * hygieneScale * World.GameSpeedModifier;
                health = Mathf.Clamp(health, 0f, 2f);
            }

            // Lower health with bad mood
            if (Mood <= .33f)
            {
                float moodScale = 1f - (Mood * 3f);

                health -= .02f * delta * moodScale * World.GameSpeedModifier;
                health = Mathf.Clamp(health, 0f, 2f);
            }

            // Lower health if starving
            if (Hunger <= .05f)
            {
                health -= .02f * delta * World.GameSpeedModifier;
                health = Mathf.Clamp(health, 0f, 2f);
            }

            // Raise health with good mood, hunger, & hygiene
            if (Mood >= .66f && Hygiene >= .75f && Hunger > .5f)
            {
                health += .05f * delta * World.GameSpeedModifier;
                health = Mathf.Clamp(health, 0f, 2f);
            }

            // If the pet is old, its health will drop no matter what
            if (IsOld)
            {
                health -= (.02f + (world.DaysPassed - 14) * 0.01f) * delta * World.GameSpeedModifier;
                health = Mathf.Clamp(health, 0f, 2f);
            }

            // If health drops to 0, pet gets sick
            // after a bit pet will start dying if not given medicine
            if ((CurrentState == PetState.Idle || CurrentState == PetState.Roam) && health <= 0.0f)
            {
                CurrentState = PetState.Sick;
                countdown = 4f;

                sprite.Texture = sickTex;
            }

            if (countdown > 0f)
                countdown -= delta * World.GameSpeedModifier * .01f;

            switch (CurrentState)
            {
                case PetState.Idle:
                    break;
                case PetState.Roam:
                    Position = Position.LinearInterpolate(goalPosition, delta * .5f * World.GameSpeedModifier);
                    break;
                case PetState.Sick:
                    if (Mood > .66f)
                        Mood = .66f;

                    if (countdown <= 0f)
                    {
                        CurrentState = PetState.Dying;

                        countdown = .5f;

                        GlobalNodes.ActivateMusicDistortion();
                        world.ShowNoiseOverlay();
                        world.HideBars();

                        sprite.Texture = dyingTex;
                    }
                    break;
                case PetState.Dying:
                    if (Mood > .33f)
                        Mood = .33f;

                    if (countdown <= 0f)
                    {
                        CurrentState = PetState.Dead;

                        countdown = 1f;

                        sprite.Texture = deadTex;

                        flies.Emitting = true;
                    }
                    break;
                case PetState.Dead:
                    if (countdown <= 0f)
                    {
                        CurrentState = PetState.Bloat;

                        countdown = 1f;

                        sprite.Texture = decayTex;

                        flies.Emitting = false;
                        maggots.Emitting = true;
                    }
                    break;
                case PetState.Bloat:
                    if (countdown <= 0f)
                    {
                        CurrentState = PetState.Decay;

                        countdown = 1f;

                        sprite.Texture = advDecayTex;
                    }
                    break;
                case PetState.Decay:
                    if (countdown <= 0f)
                    {
                        CurrentState = PetState.Dry;

                        countdown = 1f;

                        sprite.Texture = dryTex;

                        maggots.Emitting = false;
                        flies.Emitting = true;
                    }
                    break;
                case PetState.Dry:
                    if (countdown <= 0f)
                    {
                        CurrentState = PetState.Skeletal;

                        sprite.Texture = skeletonTex;
                    }
                    break;
                case PetState.Skeletal:
                    break;
            }

            ZIndex = (int)Position.y;
        }

        private void BehaviourTimeout()
        {
            switch (CurrentState)
            {
                case PetState.Idle:

                    if (World.RNG.Randf() > .25f)
                        CurrentState = PetState.Roam;
                    break;
                case PetState.Roam:

                    goalPosition = new Vector2(World.RNG.RandfRange(World.minX, World.maxX), World.RNG.RandfRange(World.minY, World.maxY));

                    if (World.RNG.Randf() > .5f)
                        CurrentState = PetState.Idle;
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

            if (CurrentState != PetState.Idle && CurrentState != PetState.Roam)
                return;

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

        public void AddHunger(float value)
        {
            if (value > 0f)
                GlobalNodes.PlayChomp();

            Hunger += value;

            Hunger = Mathf.Clamp(Hunger, 0f, 1f);
        }

        public void UpdateHygiene()
        {
            Hygiene = Mathf.Clamp(1f - (world.GetPooCount() * .25f), 0f, 1f);
        }

        public void CureSickness()
        {
            if (CurrentState == PetState.Sick)
            {
                CurrentState = PetState.Idle;
                health = 1f;
            }
        }

        public void Bury()
        {
            sprite.Texture = graveTex;
        }
    }
}