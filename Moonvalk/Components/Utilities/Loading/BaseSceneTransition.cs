using System;
using Godot;
using Moonvalk.Animation;
using Moonvalk.Audio;
using Moonvalk.Utilities;

namespace Moonvalk.Components
{
    public class BaseSceneTransition : Control
    {
        /// <summary>
        /// All states available to a screen transition.
        /// </summary>
        public enum TransitionState
        {
            Idle,
            Intro,
            Covered,
            Outro,
            Complete
        }

        #region Data Fields
        /// <summary>
        /// Properties that change how the transition will animate.
        /// </summary>
        [Export] public MoonTweenParams TransitionParams { get; private set; }

        /// <summary>
        /// Path to the sound queue for the transition intro.
        /// </summary>
        [Export] protected NodePath PAudioEnter { get; private set; }

        /// <summary>
        /// Path to the sound queue for the transition outro.
        /// </summary>
        [Export] protected NodePath PAudioExit { get; private set; }

        /// <summary>
        /// Sound queue to be played when the transition intro is played.
        /// </summary>
        private readonly SoundQueue _audioEnter;

        /// <summary>
        /// Sound queue to be played when the transition outro is played.
        /// </summary>
        private readonly SoundQueue _audioExit;

        /// <summary>
        /// Stores the current state of this transition element.
        /// </summary>
        private TransitionState _currentState = TransitionState.Idle;

        /// <summary>
        /// A map of events based on the state of this object which other objects can add actions to.
        /// </summary>
        private readonly MoonActionMap<TransitionState> _events = new MoonActionMap<TransitionState>();

        /// <summary>
        /// The current progress of this transition animation.
        /// </summary>
        private float _progress;

        /// <summary>
        /// Gets the current progress of this transition.
        /// </summary>
        public float Progress => _progress;
        #endregion

        #region Public Methods
        public BaseSceneTransition()
        {
            _audioEnter = GetNode<SoundQueue>(PAudioEnter);
            _audioExit = GetNode<SoundQueue>(PAudioExit);
        }
        
        /// <summary>
        /// </summary>
        /// <param name="onComplete_"></param>
        public void PlayIntro(Action onComplete_)
        {
            if (_currentState == TransitionState.Covered)
            {
                return;
            }

            _events.AddAction(TransitionState.Covered, onComplete_);
            _audioEnter.PlaySound();
            SetState(TransitionState.Intro);
            AnimateProgress(1f, () => { SetState(TransitionState.Covered); });
        }

        /// <summary>
        /// </summary>
        /// <param name="onComplete_"></param>
        public void PlayOutro(Action onComplete_)
        {
            if (_currentState == TransitionState.Complete)
            {
                return;
            }

            _events.AddAction(TransitionState.Complete, onComplete_);
            _audioEnter.PlaySound();
            SetState(TransitionState.Outro);
            AnimateProgress(-1f, () => { SetState(TransitionState.Complete); });
        }

        /// <summary>
        /// Called to snap this transition to the specified state.
        /// </summary>
        /// <param name="state_">The state to snap to.</param>
        public void SnapStateTo(TransitionState state_)
        {
            switch (state_)
            {
                case TransitionState.Covered:
                    _progress = 0f;
                    Material.Set("shader_param/direction", -1f);
                    break;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Called to set the progress value on the transition shader.
        /// </summary>
        protected void SetProgress()
        {
            Material.Set("shader_param/progress", _progress);
        }

        /// <summary>
        /// Handles playing the animation on progress and updating the shader material.
        /// </summary>
        /// <param name="direction_">The direction of the transition shader animation.</param>
        /// <param name="onComplete_">An action that will be invoked on completion.</param>
        protected void AnimateProgress(float direction_, Action onComplete_ = null)
        {
            _progress = 0f;
            MoonTween.CustomTweenTo<MoonTween>(() => ref _progress, 1f, TransitionParams, false)
                .OnComplete(onComplete_)
                .OnStart(() => { Material.Set("shader_param/direction", direction_); })
                .OnUpdate(SetProgress)
                .Start();
        }

        /// <summary>
        /// Sets the state of this object and emits it to listeners.
        /// </summary>
        /// <param name="state_">The new state to be set.</param>
        protected void SetState(TransitionState state_)
        {
            _currentState = state_;
            _events.Run(_currentState, true);
            if (_currentState == TransitionState.Complete)
            {
                SetState(TransitionState.Idle);
            }
        }
        #endregion
    }
}