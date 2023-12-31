using System;
using System.Collections.Generic;
using Moonvalk.Accessory;
using Moonvalk.Utilities;

namespace Moonvalk.Animation
{
    /// <summary>
    /// Container representing a singular Wobble instance.
    /// </summary>
    /// <typeparam name="Unit">The type of value that will be affected by Spring forces</typeparam>
    public abstract class BaseMoonWobble<Unit> : IMoonWobble<Unit>
    {
        /// <summary>
        /// The maximum time allowed before a reset occurs.
        /// </summary>
        private const float MaxTimeValue = 100000.0f;

        #region Data Fields
        /// <summary>
        /// A reference to the property value(s) that will be modified.
        /// </summary>
        protected Ref<float>[] _properties;

        /// <summary>
        /// The starting value.
        /// </summary>
        protected Unit[] _startValues;

        /// <summary>
        /// The overall strength of wobble applied to Properties. This is adjusted to
        /// add easing in and out of the animation.
        /// </summary>
        protected float _strength = 1f;

        /// <summary>
        /// The frequency of the sin wave applied to achieve animation.
        /// </summary>
        protected float _frequency = 5f;

        /// <summary>
        /// The amplitude of the sin wave applied to achieve animation.
        /// </summary>
        protected float _amplitude = 10f;

        /// <summary>
        /// The current time since the animation began.
        /// </summary>
        protected float _time;

        /// <summary>
        /// The duration of the wobble animation. Setting this below zero will cause
        /// the animation to loop infinitely.
        /// </summary>
        private float _duration = -1f;

        /// <summary>
        /// The percentage of the property that will be affected. This is useful for
        /// multi-axis values that need to be affected differently.
        /// </summary>
        protected Unit _percentage;

        /// <summary>
        /// Reference to an optional tween used for easing into the animation.
        /// </summary>
        private MoonTween _easeInTween;

        /// <summary>
        /// Reference to an optional tween used for easing out of the animation.
        /// </summary>
        private MoonTween _easeOutTween;

        /// <summary>
        /// The current state of this Wobble object.
        /// </summary>
        private MoonWobbleState _currentState = MoonWobbleState.Idle;

        /// <summary>
        /// A map of Actions that will occur while this Wobble is in an active state.
        /// </summary>
        private readonly MoonActionMap<MoonWobbleState> _events = new MoonActionMap<MoonWobbleState>();

        /// <summary>
        /// Stores reference to custom Wobbles applied to user generated values.
        /// </summary>
        private static Dictionary<Ref<float>[], BaseMoonWobble<Unit>> _customWobbles;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        protected BaseMoonWobble()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new BaseWobble.
        /// </summary>
        /// <param name="referenceValues_">Array of references to values.</param>
        protected BaseMoonWobble(params Ref<float>[] referenceValues_)
        {
            SetReferences(referenceValues_);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets all reference values that this Wobble will manipulate.
        /// </summary>
        /// <param name="referenceValues_">Array of references to values.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> SetReferences(params Ref<float>[] referenceValues_)
        {
            // Store reference to properties.
            _properties = referenceValues_;

            // Create new arrays for storing property start, end, and easing functions.
            _startValues = new Unit[referenceValues_.Length];
            return this;
        }

        /// <summary>
        /// Starts this Wobble with the current settings.
        /// </summary>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> Start()
        {
            UpdateStartValues();
            if (_easeInTween != null)
            {
                _easeInTween.Start();
                _currentState = MoonWobbleState.Idle;
            }
            else
            {
                HandleDuration();
                _currentState = MoonWobbleState.Start;
            }

            _events.Run(_currentState);
            (Global.GetSystem<MoonWobbleSystem>() as MoonWobbleSystem)?.Add(this);
            return this;
        }

        /// <summary>
        /// Stops this Wobble.
        /// </summary>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> Stop()
        {
            if (_easeOutTween != null)
            {
                _easeOutTween.Start();
            }
            else
            {
                _currentState = MoonWobbleState.Stopped;
            }

            return this;
        }

        /// <summary>
        /// Updates this Wobble each game tick.
        /// </summary>
        /// <param name="deltaTime_">The duration of time between last and current game tick.</param>
        /// <returns>Returns true when this object is active and false when it is complete.</returns>
        public bool Update(float deltaTime_)
        {
            Animate(deltaTime_);
            if (_currentState == MoonWobbleState.Complete)
            {
                return false;
            }

            if (_currentState == MoonWobbleState.Stopped || _currentState == MoonWobbleState.Idle)
            {
                return true;
            }

            _currentState = MoonWobbleState.Update;
            _events.Run(_currentState);
            return true;
        }

        /// <summary>
        /// Called to add an ease in to the wobble animation.
        /// </summary>
        /// <param name="parameters_">Properties that adjust the ease in Tween.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> EaseIn(MoonTweenParams parameters_ = null)
        {
            _strength = 0f;
            _easeInTween?.Delete();
            _easeInTween = null;
            _easeInTween = new MoonTween(() => ref _strength);
            _easeInTween.SetParameters(parameters_ ?? new MoonTweenParams())
                .To(1f);
            _easeInTween.OnStart(() =>
                {
                    _currentState = MoonWobbleState.Start;
                    _events.Run(_currentState);
                })
                .OnComplete(HandleDuration);
            return this;
        }

        /// <summary>
        /// Called to add an ease out to the wobble animation.
        /// </summary>
        /// <param name="parameters_">Properties that adjust the ease in Tween.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> EaseOut(MoonTweenParams parameters_ = null)
        {
            _easeOutTween?.Delete();
            _easeOutTween = null;
            _easeOutTween = new MoonTween(() => ref _strength);
            _easeOutTween.SetParameters(parameters_ ?? new MoonTweenParams())
                .To(0f);
            _easeOutTween.OnComplete(() => { _currentState = MoonWobbleState.Complete; });
            return this;
        }

        /// <summary>
        /// Called to add an ease in and out to the wobble animation.
        /// </summary>
        /// <param name="parameters_">Properties that adjust the ease in Tween.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> EaseInOut(MoonTweenParams parameters_ = null)
        {
            EaseIn(parameters_)
                .EaseOut(parameters_);
            return this;
        }

        /// <summary>
        /// Sets the frequency of the sin wave used for animation.
        /// </summary>
        /// <param name="frequency_">The new frequency value.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> SetFrequency(float frequency_)
        {
            _frequency = frequency_;
            return this;
        }

        /// <summary>
        /// Sets the amplitude of the sin wave used for animation.
        /// </summary>
        /// <param name="amplitude_">The new amplitude value.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> SetAmplitude(float amplitude_)
        {
            _amplitude = amplitude_;
            return this;
        }

        /// <summary>
        /// Sets the duration of this animation when expected to run for a finite amount of time.
        /// </summary>
        /// <param name="duration_">The duration in seconds.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> SetDuration(float duration_)
        {
            _duration = duration_;
            return this;
        }

        /// <summary>
        /// Sets the percentage of the property that will be affected. This is useful for
        /// multi-axis values that need to be affected differently.
        /// </summary>
        /// <param name="percentage_">The percentage value per axis, when applicable.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> SetPercentage(Unit percentage_)
        {
            _percentage = percentage_;
            return this;
        }

        /// <summary>
        /// Called to set all parameters from a reference object.
        /// </summary>
        /// <param name="parameters_">All properties that will be assigned.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> SetParameters(MoonWobbleParams parameters_)
        {
            SetFrequency(parameters_.Frequency)
                .SetAmplitude(parameters_.Amplitude)
                .SetDuration(parameters_.Duration);
            if (parameters_.EaseIn != null)
            {
                EaseIn(parameters_.EaseIn);
            }

            if (parameters_.EaseOut != null)
            {
                EaseOut(parameters_.EaseOut);
            }

            return this;
        }

        /// <summary>
        /// Removes this Wobble on the following game tick.
        /// </summary>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> Delete()
        {
            Reset();
            _currentState = MoonWobbleState.Complete;
            return this;
        }

        /// <summary>
        /// Defines Actions that will occur when this Wobble begins.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> OnStart(params Action[] tasksToAdd_)
        {
            _events.AddAction(MoonWobbleState.Start, tasksToAdd_);
            return this;
        }

        /// <summary>
        /// Defines Actions that will occur when this Wobble updates.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> OnUpdate(params Action[] tasksToAdd_)
        {
            _events.AddAction(MoonWobbleState.Update, tasksToAdd_);
            return this;
        }

        /// <summary>
        /// Defines Actions that will occur once this Wobble has completed.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> OnComplete(params Action[] tasksToAdd_)
        {
            _events.AddAction(MoonWobbleState.Complete, tasksToAdd_);
            return this;
        }

        /// <summary>
        /// Defines Actions that will occur once this Wobble has completed.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> Then(params Action[] tasksToAdd_)
        {
            return OnComplete(tasksToAdd_);
        }

        /// <summary>
        /// Clears all Actions that have been assigned to this Wobble.
        /// </summary>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> Reset()
        {
            _events.ClearAll();
            return this;
        }

        /// <summary>
        /// Clears all Actions that have been assigned to this Wobble for the given state.
        /// </summary>
        /// <param name="state_">The state to reset actions for.</param>
        /// <returns>Returns this Wobble object.</returns>
        public BaseMoonWobble<Unit> Reset(MoonWobbleState state_)
        {
            _events.Clear(state_);
            return this;
        }

        /// <summary>
        /// Called to force handle tasks for the current state.
        /// </summary>
        public void HandleTasks()
        {
            _events.Run(_currentState);
        }

        /// <summary>
        /// Gets the current state of this Wobble.
        /// </summary>
        /// <returns>Returns the current state.</returns>
        public MoonWobbleState GetCurrentState()
        {
            return _currentState;
        }

        /// <summary>
        /// Initializes a custom Wobble based on a reference value as a property.
        /// </summary>
        /// <param name="referenceValue_">The property to be animated.</param>
        /// <param name="percentage_">
        /// the percentage of the property that will be affected. This is useful for
        /// multi-axis values that need to be affected differently.
        /// </param>
        /// <param name="parameters_">Properties that adjust how this animation will look.</param>
        /// <param name="start_">Flag that determines if this animation should begin immediately.</param>
        /// <returns>Returns the new Wobble instance.</returns>
        public static BaseMoonWobble<Unit> CustomWobbleTo<WobbleType>(
            Ref<float> referenceValue_,
            Unit percentage_,
            MoonWobbleParams parameters_ = null,
            bool start_ = true
        ) where WobbleType : BaseMoonWobble<Unit>, new()
        {
            var refs = new[] { referenceValue_ };
            return CustomWobbleTo<WobbleType>(refs, percentage_, parameters_, start_);
        }

        /// <summary>
        /// Initializes a custom Wobble based on a reference value as a property.
        /// </summary>
        /// <param name="referenceValues_">The property to be animated.</param>
        /// <param name="percentage_">
        /// the percentage of the property that will be affected. This is useful for
        /// multi-axis values that need to be affected differently.
        /// </param>
        /// <param name="parameters_">Properties that adjust how this animation will look.</param>
        /// <param name="start_">Flag that determines if this animation should begin immediately.</param>
        /// <returns>Returns the new Wobble instance.</returns>
        public static BaseMoonWobble<Unit> CustomWobbleTo<WobbleType>(
            Ref<float>[] referenceValues_,
            Unit percentage_,
            MoonWobbleParams parameters_ = null,
            bool start_ = true
        ) where WobbleType : BaseMoonWobble<Unit>, new()
        {
            _customWobbles = _customWobbles ?? new Dictionary<Ref<float>[], BaseMoonWobble<Unit>>();
            if (_customWobbles.ContainsKey(referenceValues_))
            {
                _customWobbles[referenceValues_]
                    .Delete();
                _customWobbles.Remove(referenceValues_);
            }

            BaseMoonWobble<Unit> wobble = new WobbleType();
            wobble.SetReferences(referenceValues_)
                .SetParameters(parameters_ ?? new MoonWobbleParams())
                .SetPercentage(percentage_)
                .OnComplete(() => { _customWobbles.Remove(referenceValues_); });
            if (start_)
            {
                wobble.Start();
            }

            _customWobbles.Add(referenceValues_, wobble);
            return wobble;
        }

        /// <summary>
        /// Gets a custom Wobble object for the provided reference value, if it exists.
        /// </summary>
        /// <typeparam name="Unit">The type of used for this reference value.</typeparam>
        /// <param name="referenceValue_">The reference value a Wobble object is applied to.</param>
        /// <returns>Returns the requested Wobble object if it exists or null if it cannot be found.</returns>
        public static BaseMoonWobble<Unit> GetCustomWobble(Ref<float> referenceValue_)
        {
            var refs = new[] { referenceValue_ };
            return GetCustomWobble(refs);
        }

        /// <summary>
        /// Gets a custom Wobble object for the provided reference value, if it exists.
        /// </summary>
        /// <typeparam name="Unit">The type of used for this reference value.</typeparam>
        /// <param name="referenceValues_">The reference value a Wobble object is applied to.</param>
        /// <returns>Returns the requested Wobble object if it exists or null if it cannot be found.</returns>
        public static BaseMoonWobble<Unit> GetCustomWobble(Ref<float>[] referenceValues_)
        {
            return _customWobbles.TryGetValue(referenceValues_, out var wobble) ? wobble : null;
        }

        /// <summary>
        /// Returns true when this object is complete.
        /// </summary>
        /// <returns>True when state is complete.</returns>
        public bool IsComplete()
        {
            return _currentState == MoonWobbleState.Complete;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Method used to update all properties available to this object.
        /// </summary>
        protected abstract void UpdateProperties();

        /// <summary>
        /// Called to continue animating this wobble object.
        /// </summary>
        /// <param name="deltaTime_">Time elapsed between last and current frame.</param>
        private void Animate(float deltaTime_)
        {
            _time = (_time + deltaTime_) % MaxTimeValue;
            UpdateProperties();
        }

        /// <summary>
        /// Updates all starting values set the reference property values.
        /// </summary>
        protected abstract void UpdateStartValues();

        /// <summary>
        /// Called to handle adding a timer for stopping this animation when a duration has been defined.
        /// </summary>
        private void HandleDuration()
        {
            if (_duration > 0f)
            {
                MoonTimer.Wait(_duration, () => { Stop(); });
            }
        }
        #endregion
    }
}