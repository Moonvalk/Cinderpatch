using Godot;
using Moonvalk.UI;

namespace Moonvalk.Components.UI
{
    /// <summary>
    /// Base controller for a slider element that takes input to move up / down.
    /// </summary>
    public class MoonSliderController : Control
    {
        #region Godot Events
        /// <summary>
        /// Called when this object is first initialized.
        /// </summary>
        public override void _Ready()
        {
            ProgressBar = this.GetComponent<MoonProgressBar>();
            _buttonDown = GetNode<MoonButton>(PButtonDown);
            _buttonUp = GetNode<MoonButton>(PButtonUp);

            _buttonDown.Connect("pressed", this, nameof(HandleButtonDownPress));
            _buttonUp.Connect("pressed", this, nameof(HandleButtonUpPress));
            ProgressBar.Connect(nameof(MoonProgressBar.OnProgressChange), this, nameof(HandleProgressBarValueChange));
        }
        #endregion

        #region Data Fields
        /// <summary>
        /// Path to the button element for decrementing.
        /// </summary>
        [Export] protected NodePath PButtonDown { get; private set; }

        /// <summary>
        /// Path to the button element for incrementing.
        /// </summary>
        [Export] protected NodePath PButtonUp { get; private set; }

        /// <summary>
        /// Stores reference to the progress bar element.
        /// </summary>
        public MoonProgressBar ProgressBar { get; private set; }

        /// <summary>
        /// Stores reference to the button for decrementing.
        /// </summary>
        private MoonButton _buttonDown;

        /// <summary>
        /// Stores reference to the button for incrementing.
        /// </summary>
        private MoonButton _buttonUp;

        /// <summary>
        /// Event which emits when the slider value has changed.
        /// </summary>
        /// <param name="slider_">Reference to the slider object.</param>
        /// <param name="value_">The new value stored on this slider.</param>
        [Signal]
        public delegate void OnSliderValueChange(MoonSliderController slider_, float value_);
        #endregion

        #region Private Methods
        /// <summary>
        /// Handles a press on the down button.
        /// </summary>
        protected void HandleButtonDownPress()
        {
            UpdateProgressBar(ProgressBar.Progress - 0.1f);
        }

        /// <summary>
        /// Handles a press on the up button.
        /// </summary>
        protected void HandleButtonUpPress()
        {
            UpdateProgressBar(ProgressBar.Progress + 0.1f);
        }

        /// <summary>
        /// Called to update the progress bar with animation.
        /// </summary>
        /// <param name="percentage_">The new percentage to animate to.</param>
        private void UpdateProgressBar(float percentage_)
        {
            ProgressBar.SetProgress(percentage_);
        }

        /// <summary>
        /// Handles emitting an event when the progress bar value has changed.
        /// </summary>
        /// <param name="value_">The value to share.</param>
        protected void HandleProgressBarValueChange(float value_)
        {
            EmitSignal(nameof(OnSliderValueChange), this, value_);
        }
        #endregion
    }
}