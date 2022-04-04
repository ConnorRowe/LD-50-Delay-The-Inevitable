using Godot;

namespace Inevitable
{
    public class NamePetScreen : MenuScreen
    {
        private LineEdit lineEdit;

        public override void _Ready()
        {
            base._Ready();

            lineEdit = GetNode<LineEdit>("Content/LineEdit");
        }

        protected override void NextScene()
        {
            GlobalNodes.PetName = lineEdit.Text.Length == 0 ? "Your pet" : lineEdit.Text;

            base.NextScene();
        }
    }
}