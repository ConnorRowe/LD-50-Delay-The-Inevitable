using Godot;

namespace Inevitable
{
    public class VolumeSlider : HSlider
    {
        int busIdx = 0;

        public override void _Ready()
        {
            base._Ready();

            busIdx = AudioServer.GetBusIndex("Master");

            Connect("value_changed", this, nameof(ValueChanged));
			Connect("mouse_entered", GlobalNodes.INSTANCE, nameof(GlobalNodes.PlayUIClick));

            Value = GD.Db2Linear(AudioServer.GetBusVolumeDb(busIdx));
        }

        private void ValueChanged(float vol)
        {
            AudioServer.SetBusVolumeDb(busIdx, GD.Linear2Db(vol));
        }
    }
}