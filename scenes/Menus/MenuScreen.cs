using Godot;

namespace Inevitable
{
    public class MenuScreen : PanelContainer
    {
        [Export]
        private PackedScene nextScene;
        private Tween tween;
        private Button nextSceneButton;
        private Control content;

        public override void _Ready()
        {
            base._Ready();

            tween = GetNode<Tween>("Tween");
            nextSceneButton = GetNode<Button>("Content/NextSceneButton");
            content = GetNode<Control>("Content");

            nextSceneButton.Connect("pressed", this, nameof(NextScene));
            nextSceneButton.Connect("mouse_entered", GlobalNodes.INSTANCE, nameof(GlobalNodes.PlayUIClick));

            content.Modulate = Colors.Transparent;

            tween.InterpolateProperty(content, "modulate", Colors.Transparent, Colors.White, .2f);
            tween.Start();
        }

        protected async virtual void NextScene()
        {
            nextSceneButton.Disconnect("pressed", this, nameof(NextScene));

            tween.InterpolateProperty(content, "modulate", Colors.White, Colors.Transparent, .5f);
            tween.Start();

            var timer = GetTree().CreateTimer(.5f);

            await ToSignal(timer, "timeout");

            GetTree().ChangeSceneTo(nextScene);
            QueueFree();
        }
    }
}