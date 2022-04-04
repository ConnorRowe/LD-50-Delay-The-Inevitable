using Godot;

namespace Inevitable
{
    public class GameEndScreen : MenuScreen
    {
        public override void _Ready()
        {
            base._Ready();
            int year = System.DateTime.Now.Year;
            GetNode<Label>("Content/TextureRect/Label").Text = $"R.I.P\n{GlobalNodes.PetName}\n\n{year} - {year}";
        }

        protected override void NextScene()
        {
            GlobalNodes.ResetMusicDistortion();

            base.NextScene();
        }
    }
}