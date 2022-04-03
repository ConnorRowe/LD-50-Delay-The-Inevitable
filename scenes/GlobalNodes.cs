using Godot;

namespace Inevitable
{
    public class GlobalNodes : Node
    {
        private static Tween tween;

        public override void _Ready()
        {
            base._Ready();

            tween = GetNode<Tween>("Tween");
        }

        public static void ActivateMusicDistortion()
        {
            int busIdx = AudioServer.GetBusIndex("Music");
            var distortion = (AudioEffectDistortion)AudioServer.GetBusEffect(busIdx, 0);
            var pitchShift = (AudioEffectPitchShift)AudioServer.GetBusEffect(busIdx, 1);

            tween.InterpolateProperty(distortion, "pre_gain", 0f, -13.11f, 5f);
            tween.InterpolateProperty(distortion, "drive", 0f, .72f, 5f);
            tween.InterpolateProperty(distortion, "post_gain", 0f, 2.62f, 5f);

            tween.InterpolateProperty(pitchShift, "pitch_scale", 1f, .6f, 5f);

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
    }
}