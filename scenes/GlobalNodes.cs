using Godot;

namespace Inevitable
{
    public class GlobalNodes : Node
    {
        public static string PetName { get; set; } = "";
        public static GlobalNodes INSTANCE;
        private static AudioStreamSample[] fartSamples = new AudioStreamSample[5] { GD.Load<AudioStreamSample>("res://audio/fart_1.wav"), GD.Load<AudioStreamSample>("res://audio/fart_2.wav"), GD.Load<AudioStreamSample>("res://audio/fart_3.wav"), GD.Load<AudioStreamSample>("res://audio/fart_4.wav"), GD.Load<AudioStreamSample>("res://audio/fart_5.wav") };

        private static Tween tween;
        private static AudioStreamPlayer fartPlayer;
        private static AudioStreamPlayer uiClickPlayer;
        private static AudioStreamPlayer puffPlayer;
        private static AudioStreamPlayer chompPlayer;

        public override void _Ready()
        {
            base._Ready();

            tween = GetNode<Tween>("Tween");
            fartPlayer = GetNode<AudioStreamPlayer>("FartPlayer");
            uiClickPlayer = GetNode<AudioStreamPlayer>("UIClickPlayer");
            puffPlayer = GetNode<AudioStreamPlayer>("PuffPlayer");
            chompPlayer = GetNode<AudioStreamPlayer>("ChompPlayer");

            INSTANCE = this;
        }

        public static void ActivateMusicDistortion()
        {
            int busIdx = AudioServer.GetBusIndex("Music");
            var distortion = (AudioEffectDistortion)AudioServer.GetBusEffect(busIdx, 0);
            var pitchShift = (AudioEffectPitchShift)AudioServer.GetBusEffect(busIdx, 1);

            tween.InterpolateProperty(distortion, "pre_gain", 0f, -13.11f, 10f);
            tween.InterpolateProperty(distortion, "drive", 0f, .72f, 10f);
            tween.InterpolateProperty(distortion, "post_gain", 0f, 2.62f, 10f);

            tween.InterpolateProperty(pitchShift, "pitch_scale", 1f, .6f, 10f);

            tween.Start();
        }

        public static void ResetMusicDistortion()
        {
            int busIdx = AudioServer.GetBusIndex("Music");
            var distortion = (AudioEffectDistortion)AudioServer.GetBusEffect(busIdx, 0);
            var pitchShift = (AudioEffectPitchShift)AudioServer.GetBusEffect(busIdx, 1);

            distortion.PreGain = distortion.Drive = distortion.PostGain = 0f;
            pitchShift.PitchScale = 1f;
        }

        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventKey ek && ek.Pressed && ek.Scancode == (int)KeyList.F11)
            {
                OS.WindowFullscreen = !OS.WindowFullscreen;
            }
        }

        public static void PlayRandomFart()
        {
            fartPlayer.Stream = fartSamples[World.RNG.RandiRange(0, 4)];
            fartPlayer.Play();
        }

        public static void PlayUIClick()
        {
            uiClickPlayer.Play();
        }

        public static void PlayPuff()
        {
            if (!puffPlayer.Playing)
                puffPlayer.Play();
        }

        public static void PlayChomp()
        {
            chompPlayer.Play();
        }
    }
}