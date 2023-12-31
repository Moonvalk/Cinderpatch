using System;
using System.Collections.Generic;
using Moonvalk.Accessory;
using Moonvalk.Utilities;

namespace Moonvalk.Animation
{
    /// <summary>
    /// Container representing a singular Tween object.
    /// </summary>
    /// <typeparam name="Unit">The type of value that will be affected by Spring forces</typeparam>
    public abstract class BaseMoonTween<Unit> : IMoonTween<Unit>
    {
        #region Data Fields
        /// <summary>
        /// A reference to the property value(s) that will be modified.
        /// </summary>
        protected Ref<float>[] _properties;

        /// <summary>
        /// The target value that will be reached.
        /// </summary>
        protected Unit[] _targetValues;

        /// <summary>
        /// The starting value.
        /// </summary>
        protected Unit[] _startValues;

        /// <summary>
        /// A timer used to delay Tweens before playing.
        /// </summary>
        private MoonTimer _delayTimer;

        /// <summary>
        /// A duration in seconds that it will take for this Tween to elapse.
        /// </summary>
        private float _duration = 1f;

        /// <summary>
        /// The percentage currently elapsed from 0f to 1f.
        /// </summary>
        protected float _percentage;

        /// <summary>
        /// An EasingFunction to be applied to this Tween.
        /// </summary>
        protected EasingFunction[] _easingFunctions;

        /// <summary>
        /// The current state of this Tween object.
        /// </summary>
        private MoonTweenState _currentState = MoonTweenState.Idle;

        /// <summary>
        /// Should this animation begin as soon as a Target value is assigned?
        /// </summary>
        public bool StartOnTargetAssigned { get; set; }

        /// <summary>
        /// A map of Actions that will occur while this Tween is in an active state.
        /// </summary>
        private readonly MoonActionMap<MoonTweenState> _events = new MoonActionMap<MoonTweenState>();

        /// <summary>
        /// Stores reference to custom tweens applied to user generated values.
        /// </summary>
        private static Dictionary<Ref<float>[], BaseMoonTween<Unit>> _customTweens;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        protected BaseMoonTween()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new BaseTween.
        /// </summary>
        /// <param name="referenceValues_">Array of references to values.</param>
        protected BaseMoonTween(params Ref<float>[] referenceValues_)
        {
            SetReferences(referenceValues_);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets all reference values that this Tween will manipulate.
        /// </summary>
        /// <param name="referenceValues_">Array of references to values.</param>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> SetReferences(params Ref<float>[] referenceValues_)
        {
            // Store reference to properties.
            _properties = referenceValues_;

            // Create new arrays for storing property start, end, and easing functions.
            _startValues = new Unit[referenceValues_.Length];
            _targetValues = new Unit[referenceValues_.Length];
            _easingFunctions = new EasingFunction[referenceValues_.Length];
            for (var i = 0; i < _easingFunctions.Length; i++)
            {
                _easingFunctions[i] = Easing.Linear.None;
            }

            return this;
        }

        /// <summary>
        /// Starts this Tween with the current settings.
        /// </summary>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> Start()
        {
            UpdateStartValues();
            _percentage = 0f;
            if (_delayTimer != null)
            {
                _delayTimer.Start();
                _currentState = MoonTweenState.Idle;
            }
            else
            {
                _currentState = MoonTweenState.Start;
            }

            _events.Run(_currentState);
            (Global.GetSystem<MoonTweenSystem>() as MoonTweenSystem)?.Add(this);
            return this;
        }

        /// <summary>
        /// Stops this Tween.
        /// </summary>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> Stop()
        {
            _currentState = MoonTweenState.Stopped;
            return this;
        }

        /// <summary>
        /// Updates this Tween.
        /// </summary>
        /// <param name="deltaTime_">The duration of time between last and current game tick.</param>
        /// <returns>Returns true when this Tween is active and false when it is complete.</returns>
        public bool Update(float deltaTime_)
        {
            if (_currentState == MoonTweenState.Complete)
            {
                return false;
            }

            if (_currentState == MoonTweenState.Stopped)
            {
                return true;
            }

            if (_currentState == MoonTweenState.Idle)
            {
                if (_delayTimer != null && _delayTimer.IsComplete())
                {
                    _currentState = MoonTweenState.Start;
                    _events.Run(_currentState);
                }
                else
                {
                    return true;
                }
            }

            var targetReached = Animate(deltaTime_);
            _currentState = MoonTweenState.Update;
            _events.Run(_currentState);
            if (targetReached)
            {
                _currentState = MoonTweenState.Complete;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the delay in seconds applied to this Tween.
        /// </summary>
        /// <param name="delaySeconds_">The delay duration in seconds.</param>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> SetDelay(float delaySeconds_)
        {
            if (_delayTimer == null)
            {
                _delayTimer = new MoonTimer();
            }

            _delayTimer.SetDuration(delaySeconds_);
            return this;
        }

        /// <summary>
        /// Sets the duration in seconds that must elapse for this Tween to resolve.
        /// </summary>
        /// <param name="durationSeconds_">The duration of this Tween in seconds.</param>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> SetDuration(float durationSeconds_)
        {
            _duration = durationSeconds_;
            return this;
        }

        /// <summary>
        /// Sets the target value(s) that this Tween will reach once complete.
        /// </summary>
        /// <param name="targetValues_">An array of target values for each property.</param>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> To(params Unit[] targetValues_)
        {
            for (var i = 0; i < targetValues_.Length; i++)
            {
                _targetValues[i] = targetValues_[i];
            }

            if (StartOnTargetAssigned)
            {
                Start();
            }

            return this;
        }

        /// <summary>
        /// Sets an Easing Function for each available property.
        /// </summary>
        /// <param name="functions_">An array of Easing Functions per property.</param>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> SetEase(params EasingFunction[] functions_)
        {
            for (var i = 0; i < _easingFunctions.Length; i++)
            {
                var nextFunc = functions_.Length > i ? functions_[i] : functions_[0];
                _easingFunctions[i] = nextFunc;
            }

            return this;
        }

        /// <summary>
        /// Called to set all parameters from a reference object.
        /// </summary>
        /// <param name="parameters_">All properties that will be assigned.</param>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> SetParameters(MoonTweenParams parameters_)
        {
            SetDuration(parameters_.Duration)
                .SetDelay(parameters_.Delay)
                .SetEase(parameters_.EasingType != Easing.Types.None ? Easing.Functions[parameters_.EasingType] : parameters_.EasingFunction);
            return this;
        }

        /// <summary>
        /// Removes this Tween object on the following game tick.
        /// </summary>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> Delete()
        {
            Reset();
            _currentState = MoonTweenState.Complete;
            return this;
        }

        /// <summary>
        /// Defines Actions that will occur when this Tween begins.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add.</param>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> OnStart(params Action[] tasksToAdd_)
        {
            _events.AddAction(MoonTweenState.Start, tasksToAdd_);
            return this;
        }

        /// <summary>
        /// Defines Actions that will occur when this Tween updates.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add.</param>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> OnUpdate(params Action[] tasksToAdd_)
        {
            _events.AddAction(MoonTweenState.Update, tasksToAdd_);
            return this;
        }

        /// <summary>
        /// Defines Actions that will occur once this Tween has completed.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add.</param>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> OnComplete(params Action[] tasksToAdd_)
        {
            _events.AddAction(MoonTweenState.Complete, tasksToAdd_);
            return this;
        }

        /// <summary>
        /// Defines Actions that will occur once this Tween has completed.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add.</param>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> Then(params Action[] tasksToAdd_)
        {
            return OnComplete(tasksToAdd_);
        }

        /// <summary>
        /// Clears all Actions that have been assigned to this Tween.
        /// </summary>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> Reset()
        {
            _events.ClearAll();
            return this;
        }

        /// <summary>
        /// Clears all Actions that have been assigned to this Tween for the given state.
        /// </summary>
        /// <param name="state_">The state to reset actions for.</param>
        /// <returns>Returns this Tween object.</returns>
        public BaseMoonTween<Unit> Reset(MoonTweenState state_)
        {
            _events.Clear(state_);
            return this;
        }

        /// <summary>
        /// Adds a Tween that will begin following the original Tween's completion.
        /// </summary>
        /// <param name="triggeringTween_">The original Tween.</param>
        /// <param name="tweenToFollow_">The Tween to start on completion.</param>
        /// <returns>Returns this Tween object.</returns>
        public static BaseMoonTween<Unit> operator +(BaseMoonTween<Unit> triggeringTween_, BaseMoonTween<Unit> tweenToFollow_)
        {
            return triggeringTween_.OnComplete(() => { tweenToFollow_.Start(); });
        }

        /// <summary>
        /// Adds an Action that will begin following this Tween's completion.
        /// </summary>
        /// <param name="triggeringTween_">The original MVTween.</param>
        /// <param name="taskForCompletion_">The Action to run on completion.</param>
        /// <returns>Returns this Tween object.</returns>
        public static BaseMoonTween<Unit> operator +(BaseMoonTween<Unit> triggeringTween_, Action taskForCompletion_)
        {
            return triggeringTween_.OnComplete(taskForCompletion_);
        }

        /// <summary>
        /// Called to force handle tasks for the current state.
        /// </summary>
        public void HandleTasks()
        {
            _events.Run(_currentState);
        }

        /// <summary>
        /// Gets the current state of this Tween.
        /// </summary>
        /// <returns>Returns the current state.</returns>
        public MoonTweenState GetCurrentState()
        {
            return _currentState;
        }

        /// <summary>
        /// Initializes a custom tween based on a reference value as a property.
        /// </summary>
        /// <typeparam name="TweenType">The type of Tween that will be used.</typeparam>
        /// <param name="referenceValue_">The property to be animated.</param>
        /// <param name="target_">The target value.</param>
        /// <param name="parameters_">Properties that adjust how this animation will look.</param>
        /// <param name="start_">Flag that determines if this animation should begin immediately.</param>
        /// <returns>Returns the new Tween object.</returns>
        public static BaseMoonTween<Unit> CustomTweenTo<TweenType>(
            Ref<float> referenceValue_,
            Unit target_,
            MoonTweenParams parameters_ = null,
            bool start_ = true
        ) where TweenType : BaseMoonTween<Unit>, new()
        {
            var refs = new[] { referenceValue_ };
            return CustomTweenTo<TweenType>(refs, target_, parameters_, start_);
        }

        /// <summary>
        /// Initializes a custom tween based on a reference value as a property.
        /// </summary>
        /// <typeparam name="TweenType">The type of Tween that will be used.</typeparam>
        /// <param name="referenceValues_">The properties to be animated.</param>
        /// <param name="target_">The target value.</param>
        /// <param name="parameters_">Properties that adjust how this animation will look.</param>
        /// <param name="start_">Flag that determines if this animation should begin immediately.</param>
        /// <returns>Returns the new Tween object.</returns>
        public static BaseMoonTween<Unit> CustomTweenTo<TweenType>(
            Ref<float>[] referenceValues_,
            Unit target_,
            MoonTweenParams parameters_ = null,
            bool start_ = true
        ) where TweenType : BaseMoonTween<Unit>, new()
        {
            _customTweens = _customTweens ?? new Dictionary<Ref<float>[], BaseMoonTween<Unit>>();
            if (_customTweens.ContainsKey(referenceValues_))
            {
                _customTweens[referenceValues_]
                    .Delete();
                _customTweens.Remove(referenceValues_);
            }

            BaseMoonTween<Unit> tween = new TweenType
                { StartOnTargetAssigned = start_ };
            tween.SetReferences(referenceValues_)
                .SetParameters(parameters_ ?? new MoonTweenParams())
                .OnComplete(() => { _customTweens.Remove(referenceValues_); })
                .To(target_);

            _customTweens.Add(referenceValues_, tween);
            return tween;
        }

        /// <summary>
        /// Gets a custom Tween object for the provided reference value, if it exists.
        /// </summary>
        /// <typeparam name="Unit">The type of used for this reference value.</typeparam>
        /// <param name="referenceValue_">The reference value a Tween object is applied to.</param>
        /// <returns>Returns the requested Tween object if it exists or null if it cannot be found.</returns>
        public static BaseMoonTween<Unit> GetCustomTween(Ref<float> referenceValue_)
        {
            var refs = new[] { referenceValue_ };
            return GetCustomTween(refs);
        }

        /// <summary>
        /// Gets a custom Tween object for the provided reference value, if it exists.
        /// </summary>
        /// <typeparam name="Unit">The type of used for this reference value.</typeparam>
        /// <param name="referenceValues_">The reference values a Tween object is applied to.</param>
        /// <returns>Returns the requested Tween object if it exists or null if it cannot be found.</returns>
        public static BaseMoonTween<Unit> GetCustomTween(Ref<float>[] referenceValues_)
        {
            if (_customTweens.TryGetValue(referenceValues_, out var tween))
            {
                return tween;
            }

            return null;
        }

        /// <summary>
        /// Returns true when this object is complete.
        /// </summary>
        /// <returns>True when state is complete.</returns>
        public bool IsComplete()
        {
            return _currentState == MoonTweenState.Complete;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Method used to update all properties available to this object.
        /// </summary>
        protected abstract void UpdateProperties();

        /// <summary>
        /// Animates this Tween from start to target.
        /// </summary>
        /// <param name="deltaTime_">The delta between last and current game tick.</param>
        /// <returns>Returns true when complete or false when actively animating.</returns>
        private bool Animate(float deltaTime_)
        {
            var isComplete = false;

            // Complete delay before animating.
            if (_delayTimer != null && !_delayTimer.IsComplete())
            {
                return false;
            }

            // Begin animating by progressing percentage.
            var newPercentage = _percentage + deltaTime_ / _duration;
            if (newPercentage >= 1f)
            {
                _percentage = 1f;
                isComplete = true;
            }
            else
            {
                _percentage = newPercentage;
            }

            UpdateProperties();
            return isComplete;
        }

        /// <summary>
        /// Updates all starting values set the reference property values.
        /// </summary>
        protected abstract void UpdateStartValues();
        #endregion
    }
}