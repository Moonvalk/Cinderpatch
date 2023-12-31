using System;
using Godot;
using Moonvalk.Animation;
using Moonvalk.Components;

namespace Moonvalk.Nodes
{
    /// <summary>
    /// Handles displaying a custom cursor that follows mouse position with the use of spring animations.
    /// </summary>
    public class SpringCursorController : Node
    {
        #region Data Fields
        /// <summary>
        /// Parameters used to dictate how the cursor will move.
        /// </summary>
        [Export] public MoonSpringParams MoveParameters { get; protected set; }

        /// <summary>
        /// Reference to the cursor display node.
        /// </summary>
        private Control _cursor;

        /// <summary>
        /// A spring used to handle moving the cursor on screen. This reference is local to this controller
        /// as we want to continue applying previous forces rather than snapping immediately.
        /// </summary>
        private MoonSpringVec2 _movementSpring;

        /// <summary>
        /// The target position to move towards that will update to new mouse positions.
        /// </summary>
        private Vector2 _targetPosition;

        /// <summary>
        /// The current position which we will animate and snap the Cursor to each frame.
        /// </summary>
        private Vector2 _currentPosition;
        #endregion

        #region Godot Events
        /// <summary>
        /// Called when this object is first initialized.
        /// </summary>
        public override void _Ready()
        {
            _cursor = this.GetComponent<TextureRect>();

            Input.MouseMode = Input.MouseModeEnum.Hidden;
            _targetPosition = _currentPosition = GetTree()
                .Root.GetMousePosition();
            _cursor.RectPosition = _targetPosition;

            _movementSpring = new MoonSpringVec2(() => ref _currentPosition.x, () => ref _currentPosition.y)
                { StartOnTargetAssigned = true };
            _movementSpring.SetParameters(MoveParameters ?? new MoonSpringParams())
                .OnUpdate(() => { _cursor.RectPosition = _currentPosition - _cursor.RectPivotOffset; });
        }

        /// <summary>
        /// Called each game tick.
        /// </summary>
        /// <param name="delta_">The time elapsed between last and current frame.</param>
        public override void _Process(float delta_)
        {
            var mouse = GetTree()
                .Root.GetMousePosition();
            const float tolerance = 0.01f;
            if (Math.Abs(_targetPosition.x - mouse.x) > tolerance || Math.Abs(_targetPosition.y - mouse.y) > tolerance)
            {
                _targetPosition = mouse;
                _movementSpring.To(_targetPosition);
            }
        }
        #endregion
    }
}