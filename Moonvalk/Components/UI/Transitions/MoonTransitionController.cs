using System;
using Godot;
using Moonvalk.Nodes;

namespace Moonvalk.Components.UI
{
    /// <summary>
    /// Controller for handling transition animations between scene swaps.
    /// </summary>
    public class MoonTransitionController : Control
    {
        #region Godot Events
        /// <summary>
        /// Called when this object is first initialized.
        /// </summary>
        public override void _Ready()
        {
            _transition = this.AddInstance<BaseMoonTransition>(PrefabTransition);
            MoveChild(_transition, 0);

            _spinner = GetNode<MoonSceneLoadSpinner>(PSpinner);
            _transition.SnapState(MoonTransitionState.Covered);
        }
        #endregion

        #region Data Fields
        /// <summary>
        /// A prefab to be instantiated as a transition animation.
        /// </summary>
        [Export] protected PackedScene PrefabTransition { get; private set; }

        /// <summary>
        /// Path to the spinner node.
        /// </summary>
        [Export] protected NodePath PSpinner { get; private set; }

        /// <summary>
        /// Stores reference to the spinner node used to play animations during loading.
        /// </summary>
        private MoonSceneLoadSpinner _spinner;

        /// <summary>
        /// Stores reference to the transition object used to play transition animations between scene swaps.
        /// </summary>
        private BaseMoonTransition _transition;
        #endregion

        #region Public Methods
        /// <summary>
        /// Plays the transition animation intro.
        /// </summary>
        /// <param name="onCovered_">An optional action to be executed on completion.</param>
        public void PlayTransitionIntro(Action onCovered_ = null)
        {
            _transition.Events.AddAction(MoonTransitionState.Covered, () =>
            {
                _spinner.Play();
                onCovered_?.Invoke();
            });
            _transition.PlayIntro();
        }

        /// <summary>
        /// Plays the transition animation outro.
        /// </summary>
        /// <param name="onComplete_">An optional action to be executed on completion.</param>
        public void PlayTransitionOutro(Action onComplete_ = null)
        {
            _transition.Events.AddAction(MoonTransitionState.Complete, onComplete_);
            _spinner.Stop();
            _transition.PlayOutro();
        }
        #endregion
    }
}