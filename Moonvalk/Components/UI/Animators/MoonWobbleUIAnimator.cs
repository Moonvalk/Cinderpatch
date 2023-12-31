using Godot;
using Moonvalk.Animation;

namespace Moonvalk.Components
{
    /// <summary>
    /// Animates a UI component with wobble animations on load.
    /// </summary>
    public class MoonWobbleUIAnimator : Control
    {
        /// <summary>
        /// The path to the UI element inherited from a Control node.
        /// </summary>
        [Export] protected NodePath PUiElement { get; private set; }

        /// <summary>
        /// Properties for the movement wobble animation.
        /// </summary>
        [Export] public MoonWobbleParams WobbleMoveParams { get; private set; }

        /// <summary>
        /// The direction of the wobble movement animation.
        /// </summary>
        [Export] public Vector2 WobbleMoveDirection { get; private set; } = Vector2.Up;

        /// <summary>
        /// Properties for the scaling wobble animation.
        /// </summary>
        [Export] public MoonWobbleParams WobbleScaleParams { get; private set; }

        /// <summary>
        /// Properties for the rotation wobble animation.
        /// </summary>
        [Export] public MoonWobbleParams WobbleRotateParams { get; private set; }

        /// <summary>
        /// Stores reference to the element that will be animated.
        /// </summary>
        private Control _element;

        /// <summary>
        /// Called when this object is first initialized.
        /// </summary>
        public override void _Ready()
        {
            _element = GetNode<Control>(PUiElement);
            _element.CenterPivot();
            _element.WobbleMove(WobbleMoveDirection, WobbleMoveParams)
                .Start();
            _element.WobbleScale(Vector2.One, WobbleScaleParams)
                .Start();
            _element.WobbleRotation(1f, WobbleRotateParams)
                .Start();
        }
    }
}