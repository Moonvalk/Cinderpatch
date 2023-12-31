using Godot;
using Godot.Collections;
using Moonvalk.Animation;

namespace Moonvalk.UI
{
    /// <summary>
    /// Base class for an extended button with hover animations.
    /// </summary>
    public class MoonButton : Button
    {
        #region Godot Events
        /// <summary>
        /// Called when this object is first initialized.
        /// </summary>
        public override void _Ready()
        {
            _container = GetNode<TextureRect>(PContainer);
            this.CenterPivot();
            _container.CenterPivot();

            Connect("pressed", this, nameof(HandlePress));
            Connect("mouse_entered", this, nameof(HandleChangeFocus), new Array
                { true });
            Connect("focus_entered", this, nameof(HandleChangeFocus), new Array
                { true });
            Connect("focus_exited", this, nameof(HandleChangeFocus), new Array
                { false });
        }
        #endregion

        #region Data Fields
        /// <summary>
        /// Stores the path to the container element.
        /// </summary>
        [Export] protected NodePath PContainer { get; private set; }

        /// <summary>
        /// The scale used when hovering this button element.
        /// </summary>
        [Export] public float HoveredScale { get; private set; } = 1.2f;

        /// <summary>
        /// Stores reference to the container element.
        /// </summary>
        private TextureRect _container;

        /// <summary>
        /// Flag that determines if this button is focused.
        /// </summary>
        private bool _isFocused;

        /// <summary>
        /// Signal that is emitted once focus has entered on this element.
        /// </summary>
        [Signal]
        public delegate void OnFocusEnter();

        /// <summary>
        /// Signal that is emitted once focus has exited on this element.
        /// </summary>
        [Signal]
        public delegate void OnFocusExit();
        #endregion

        #region Private Methods
        /// <summary>
        /// Handles updating the focused state of this button when an event occurs.
        /// </summary>
        /// <param name="isFocused_">Flag that determines if this button is currently focused or not.</param>
        protected void HandleChangeFocus(bool isFocused_)
        {
            if (_isFocused == isFocused_)
            {
                return;
            }

            _isFocused = isFocused_;
            if (_isFocused)
            {
                GrabFocus();
                _container.ScaleTo(Vector2.One * HoveredScale, new MoonTweenParams
                    { Duration = 0.5f, EasingType = Easing.Types.ElasticOut });
                _container.ColorTo(new Color(1.1f, 1.1f, 1.1f), new MoonTweenParams
                    { Duration = 0.25f });
            }
            else
            {
                _container.ScaleTo(Vector2.One, new MoonTweenParams
                    { Duration = 0.25f });
                _container.ColorTo(new Color(0.9f, 0.9f, 0.9f), new MoonTweenParams
                    { Duration = 0.25f });
            }

            EmitSignal(_isFocused ? nameof(OnFocusEnter) : nameof(OnFocusExit));
        }

        /// <summary>
        /// Called to handle press animations when this button is pressed.
        /// </summary>
        protected void HandlePress()
        {
            _isFocused = true;
            _container.RectScale = Vector2.One * HoveredScale;
            _container.ScaleTo(Vector2.One * 0.9f, new MoonTweenParams
                {
                    Duration = 0.5f, EasingFunction = Easing.Elastic.Out
                })
                .Then(() => { HandleChangeFocus(false); });
        }
        #endregion
    }
}