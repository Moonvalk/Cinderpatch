using System;
using System.Collections.Generic;
using Godot;
using Moonvalk.Animation;
using Moonvalk.UI;
using Moonvalk.Utilities;

namespace Moonvalk.Components.UI
{
    /// <summary>
    /// Base class for a menu page controller. This handles all elements displayed on any
    /// particular page found within a menu scene.
    /// </summary>
    public class BaseMenuPageController : Control
    {
        #region Godot Events
        /// <summary>
        /// Called when this object is first initialized.
        /// </summary>
        public override void _Ready()
        {
            _buttons = this.GetAllComponents<MoonButton>(typeof(BaseRootMenu));
            foreach (var button in _buttons)
            {
                button.Connect(nameof(MoonButton.OnFocusEnter), this, nameof(HandleButtonFocus));
                button.Connect("pressed", this, nameof(HandleButtonPress));
            }

            _elements = new Control[PElements.Length];
            for (var index = 0; index < _elements.Length; index++)
            {
                _elements[index] = GetNode<Control>(PElements[index]);
                _elements[index]
                    .CenterPivot();
                _elements[index].Visible = false;
                _elements[index].RectScale = Vector2.Zero;
            }

            this.Wait(0.1f, () => { DisplayElements(true); });
        }
        #endregion

        #region Data Fields
        /// <summary>
        /// Paths to all page elements that will be animated on/off the screen.
        /// </summary>
        [Export] protected NodePath[] PElements { get; private set; }

        /// <summary>
        /// Delay between each element as they are introduced.
        /// </summary>
        [Export] public float ElementIntroDelay { get; private set; } = 0.05f;

        /// <summary>
        /// Reference to all button objects found on this page.
        /// </summary>
        private List<MoonButton> _buttons;

        /// <summary>
        /// Reference to all elements found on this page.
        /// </summary>
        private Control[] _elements;

        /// <summary>
        /// Flag that is true when the page is actively displayed.
        /// </summary>
        public bool IsPageDisplayed { get; protected set; }

        /// <summary>
        /// Stores index of the last requested page to ensure events are only emitted once.
        /// </summary>
        private int _requestedPage = -1;

        /// <summary>
        /// Event emitted when a button object is focused.
        /// </summary>
        [Signal]
        public delegate void OnButtonFocus();

        /// <summary>
        /// Event emitted when a button object is pressed.
        /// </summary>
        [Signal]
        public delegate void OnButtonPress();

        /// <summary>
        /// Event emitted when an element is animated on/off screen.
        /// </summary>
        [Signal]
        public delegate void OnAnimateElement();

        /// <summary>
        /// Event emitted when a request has been made to display a new page.
        /// </summary>
        /// <param name="index_">The page index that was requested.</param>
        [Signal]
        public delegate void OnDisplayPage(int index_);

        /// <summary>
        /// Event emitted when a request has been made to close the menu.
        /// </summary>
        [Signal]
        public delegate void OnExitMenu();
        #endregion

        #region Public Methods
        /// <summary>
        /// Called to emit a request to show a different page.
        /// </summary>
        /// <param name="index_">The page index to request.</param>
        /// <returns>Returns true if the page request is unique, false if the request was already made.</returns>
        public bool DisplayPage(int index_)
        {
            if (_requestedPage == index_)
            {
                return false;
            }

            _requestedPage = index_;
            EmitSignal(nameof(OnDisplayPage), index_);
            return true;
        }

        /// <summary>
        /// Called to emit a request to close the root menu.
        /// </summary>
        public void ExitMenu()
        {
            EmitSignal(nameof(OnExitMenu));
        }

        /// <summary>
        /// Called to hide this page by animating elements away.
        /// </summary>
        /// <param name="onComplete_">Action to be executed when the animation is complete.</param>
        public virtual void HidePage(Action onComplete_)
        {
            DisplayElements(false, onComplete_);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Handles emitting a button focus event.
        /// </summary>
        protected void HandleButtonFocus()
        {
            EmitSignal(nameof(OnButtonFocus));
        }

        /// <summary>
        /// Handles emitting a button press event.
        /// </summary>
        protected void HandleButtonPress()
        {
            EmitSignal(nameof(OnButtonPress));
        }

        /// <summary>
        /// Called to animate elements on or off of the page.
        /// </summary>
        /// <param name="flag_">True when elements should be introduced, false to hide.</param>
        /// <param name="onComplete_">An optional action to be called when the animation is done.</param>
        protected void DisplayElements(bool flag_, Action onComplete_ = null)
        {
            var easing = flag_ ? (EasingFunction)Easing.Back.Out : Easing.Cubic.InOut;
            var duration = flag_ ? 0.75f : 0.25f;
            var size = flag_ ? Vector2.One : Vector2.Zero;

            var delay = 0.5f;
            for (var index = 0; index < _elements.Length; index++)
            {
                var currentIndex = index;
                if (flag_)
                {
                    delay += currentIndex * ElementIntroDelay;
                }

                var animation = _elements[currentIndex]
                    .ScaleTo(size, new MoonTweenParams
                    {
                        Duration = duration, EasingFunction = easing, Delay = delay
                    }, false);
                if (currentIndex == _elements.Length - 1)
                {
                    IsPageDisplayed = flag_;
                    animation.OnComplete(onComplete_);
                }

                animation.OnStart(() =>
                    {
                        if (flag_)
                        {
                            IsPageDisplayed = true;
                        }

                        _elements[currentIndex].Visible = true;
                        EmitSignal(nameof(OnAnimateElement));
                    })
                    .Start();
            }
        }
        #endregion
    }
}