using System;
using System.Collections.Generic;
using Moonvalk.Accessory;
using Moonvalk.Utilities;

namespace Moonvalk.Animation
{
    /// <summary>
    /// Base class for Spring objects.
    /// </summary>
    /// <typeparam name="Unit">The type of value that will be affected by Spring forces</typeparam>
    public abstract class BaseMoonSpring<Unit> : IMoonSpring<Unit>
    {
        #region Data Fields
        /// <summary>
        /// A reference to the property value(s) that will be modified.
        /// </summary>
        protected Ref<float>[] _properties;

        /// <summary>
        /// The target value that will be reached.
        /// </summary>
        protected Unit[] _targetProperties;

        /// <summary>
        /// The tension value applied to this spring.
        /// </summary>
        protected float _tension = 50f;

        /// <summary>
        /// The dampening value applied to this spring.
        /// </summary>
        protected float _dampening = 10f;

        /// <summary>
        /// The current speed applied to this spring.
        /// </summary>
        protected Unit[] _speed;

        /// <summary>
        /// The amount of force to be applied each frame.
        /// </summary>
        protected Unit[] _currentForce;

        /// <summary>
        /// The minimum force applied to a Spring before it is no longer updated until settings change.
        /// </summary>
        protected Unit[] _minimumForce;

        /// <summary>
        /// The default percentage of total distance springed that will be assigned as a minimum force.
        /// </summary>
        protected const float DefaultMinimumForcePercentage = 0.0001f;

        /// <summary>
        /// The current state of this Spring object.
        /// </summary>
        private MoonSpringState _currentState = MoonSpringState.Stopped;

        /// <summary>
        /// Should this animation begin as soon as a Target value is assigned?
        /// </summary>
        public bool StartOnTargetAssigned { get; set; }

        /// <summary>
        /// A map of Actions that will occur while this Spring is in an active state.
        /// </summary>
        private readonly MoonActionMap<MoonSpringState> _events = new MoonActionMap<MoonSpringState>();

        /// <summary>
        /// Stores reference to custom Springs applied to user generated values.
        /// </summary>
        private static Dictionary<Ref<float>[], BaseMoonSpring<Unit>> _customSprings;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        protected BaseMoonSpring()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new BaseSpring.
        /// </summary>
        /// <param name="referenceValues_">Array of references to values.</param>
        protected BaseMoonSpring(params Ref<float>[] referenceValues_)
        {
            SetReferences(referenceValues_);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets all reference values that this Spring will manipulate.
        /// </summary>
        /// <param name="referenceValues_">Array of references to values.</param>
        public BaseMoonSpring<Unit> SetReferences(params Ref<float>[] referenceValues_)
        {
            // Store reference to properties and build function maps.
            _properties = referenceValues_;

            // Create new array for storing property targets.
            _targetProperties = new Unit[referenceValues_.Length];
            _currentForce = new Unit[referenceValues_.Length];
            _speed = new Unit[referenceValues_.Length];
            return this;
        }

        /// <summary>
        /// Updates this Spring.
        /// </summary>
        /// <param name="deltaTime_">The duration of time between last and current game tick.</param>
        /// <returns>Returns true when this Spring is active and false when it is complete.</returns>
        public bool Update(float deltaTime_)
        {
            if (_currentState == MoonSpringState.Complete)
            {
                return false;
            }

            if (_currentState == MoonSpringState.Stopped)
            {
                return true;
            }

            _currentState = MoonSpringState.Update;
            _events.Run(_currentState);

            // Update springs each frame until settled.
            CalculateForces();
            ApplyForces(deltaTime_);
            if (!MinimumForcesMet())
            {
                SnapSpringToTarget();
                _currentState = MoonSpringState.Complete;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts this Spring with the current settings if there is a need to apply forces.
        /// </summary>
        /// <returns>Returns reference to this Spring.</returns>
        public BaseMoonSpring<Unit> Start()
        {
            if (NeedToApplyForce())
            {
                _currentState = MoonSpringState.Start;
                _events.Run(_currentState);
                (Global.GetSystem<MoonSpringSystem>() as MoonSpringSystem)?.Add(this);
            }

            return this;
        }

        /// <summary>
        /// Stops this Spring.
        /// </summary>
        /// <returns>Returns reference to this Spring.</returns>
        public BaseMoonSpring<Unit> Stop()
        {
            _currentState = MoonSpringState.Stopped;
            return this;
        }

        /// <summary>
        /// Sets the dampening factor applied to this spring.
        /// </summary>
        /// <param name="dampening_">New dampening factor.</param>
        /// <returns>Returns reference to this spring.</returns>
        public BaseMoonSpring<Unit> SetDampening(float dampening_)
        {
            _dampening = dampening_;
            return this;
        }

        /// <summary>
        /// Sets the tension factor applied to this spring.
        /// </summary>
        /// <param name="tension_">New tension factor.</param>
        /// <returns>Returns reference to this spring.</returns>
        public BaseMoonSpring<Unit> SetTension(float tension_)
        {
            _tension = tension_;
            return this;
        }

        /// <summary>
        /// Called to set all parameters from a reference object.
        /// </summary>
        /// <param name="parameters_">All properties that will be assigned.</param>
        /// <returns>Returns this Spring object.</returns>
        public BaseMoonSpring<Unit> SetParameters(MoonSpringParams parameters_)
        {
            SetDampening(parameters_.Dampening)
                .SetTension(parameters_.Tension);
            return this;
        }

        /// <summary>
        /// Applies a new target spring height and begins animating towards reaching that value.
        /// </summary>
        /// <param name="targetProperties_">Target spring heights for each property.</param>
        /// <returns>Returns reference to this spring.</returns>
        public BaseMoonSpring<Unit> To(params Unit[] targetProperties_)
        {
            _targetProperties = targetProperties_;
            SetMinimumForce();
            if (StartOnTargetAssigned)
            {
                Start();
            }

            return this;
        }

        /// <summary>
        /// Snaps each spring property to the provided target values.
        /// </summary>
        /// <param name="targetProperties_">Target spring heights for each property.</param>
        /// <returns>Returns reference to this spring.</returns>
        public BaseMoonSpring<Unit> Snap(params Unit[] targetProperties_)
        {
            _targetProperties = targetProperties_;
            SnapSpringToTarget();
            return this;
        }

        /// <summary>
        /// Removes this Spring on the following game tick by forcing completion.
        /// </summary>
        /// <returns>Returns reference to this Spring.</returns>
        public BaseMoonSpring<Unit> Delete()
        {
            Reset();
            _currentState = MoonSpringState.Complete;
            return this;
        }

        /// <summary>
        /// Defines Actions that will occur when this Spring begins.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add.</param>
        /// <returns>Returns this Spring object.</returns>
        public BaseMoonSpring<Unit> OnStart(params Action[] tasksToAdd_)
        {
            _events.AddAction(MoonSpringState.Start, true, tasksToAdd_);
            return this;
        }

        /// <summary>
        /// Defines Actions that will occur when this Spring updates.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add.</param>
        /// <returns>Returns this Spring object.</returns>
        public BaseMoonSpring<Unit> OnUpdate(params Action[] tasksToAdd_)
        {
            _events.AddAction(MoonSpringState.Update, true, tasksToAdd_);
            return this;
        }

        /// <summary>
        /// Defines Actions that will occur once this Spring has completed.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add.</param>
        /// <returns>Returns this Spring object.</returns>
        public BaseMoonSpring<Unit> OnComplete(params Action[] tasksToAdd_)
        {
            _events.AddAction(MoonSpringState.Complete, true, tasksToAdd_);
            return this;
        }

        /// <summary>
        /// Defines Actions that will occur once this Spring has completed.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add.</param>
        /// <returns>Returns this Spring object.</returns>
        public BaseMoonSpring<Unit> Then(params Action[] tasksToAdd_)
        {
            return OnComplete(tasksToAdd_);
        }

        /// <summary>
        /// Clears all Actions that have been assigned to this Spring.
        /// </summary>
        /// <returns>Returns this Spring object.</returns>
        private BaseMoonSpring<Unit> Reset()
        {
            _events.ClearAll();
            return this;
        }

        /// <summary>
        /// Clears all Actions that have been assigned to this Spring for the given state.
        /// </summary>
        /// <param name="state_">The state to reset actions for.</param>
        /// <returns>Returns this Spring object.</returns>
        public BaseMoonSpring<Unit> Reset(MoonSpringState state_)
        {
            _events.Clear(state_);
            return this;
        }

        /// <summary>
        /// Handles all tasks for the current state of this base spring.
        /// </summary>
        public void HandleTasks()
        {
            _events.Run(_currentState);
        }

        /// <summary>
        /// Gets the current state of this Spring.
        /// </summary>
        /// <returns>Returns the current state.</returns>
        public MoonSpringState GetCurrentState()
        {
            return _currentState;
        }

        /// <summary>
        /// Initializes a custom Spring based on a reference value as a property.
        /// </summary>
        /// <typeparam name="SpringType">The type of Spring that will be used.</typeparam>
        /// <param name="referenceValue_">The property to be animated.</param>
        /// <param name="target_">The target value.</param>
        /// <param name="parameters_">Properties that adjust how this animation will look.</param>
        /// <param name="start_">Flag that determines if this animation should begin immediately.</param>
        /// <returns>Returns reference to the new Spring object.</returns>
        public static BaseMoonSpring<Unit> CustomSpringTo<SpringType>(
            Ref<float> referenceValue_,
            Unit target_,
            MoonSpringParams parameters_ = null,
            bool start_ = true
        ) where SpringType : BaseMoonSpring<Unit>, new()
        {
            var refs = new[] { referenceValue_ };
            return CustomSpringTo<SpringType>(refs, target_, parameters_, start_);
        }

        /// <summary>
        /// Initializes a custom Spring based on a reference value as a property.
        /// </summary>
        /// <typeparam name="SpringType">The type of Spring that will be used.</typeparam>
        /// <param name="referenceValues_">The property to be animated.</param>
        /// <param name="target_">The target value.</param>
        /// <param name="parameters_">Properties that adjust how this animation will look.</param>
        /// <param name="start_">Flag that determines if this animation should begin immediately.</param>
        /// <returns>Returns reference to the new Spring object.</returns>
        public static BaseMoonSpring<Unit> CustomSpringTo<SpringType>(
            Ref<float>[] referenceValues_,
            Unit target_,
            MoonSpringParams parameters_ = null,
            bool start_ = true
        ) where SpringType : BaseMoonSpring<Unit>, new()
        {
            _customSprings = _customSprings ?? new Dictionary<Ref<float>[], BaseMoonSpring<Unit>>();
            if (_customSprings.ContainsKey(referenceValues_))
            {
                _customSprings[referenceValues_]
                    .Delete();
                _customSprings.Remove(referenceValues_);
            }

            BaseMoonSpring<Unit> spring = new SpringType
                { StartOnTargetAssigned = start_ };
            spring.SetReferences(referenceValues_)
                .SetParameters(parameters_ ?? new MoonSpringParams())
                .OnComplete(() => { _customSprings.Remove(referenceValues_); })
                .To(target_);

            _customSprings.Add(referenceValues_, spring);
            return spring;
        }

        /// <summary>
        /// Gets a custom Spring object for the provided reference value, if it exists.
        /// </summary>
        /// <typeparam name="Unit">The type of used for this reference value.</typeparam>
        /// <param name="referenceValue_">The reference value a Spring object is applied to.</param>
        /// <returns>Returns the requested Spring object if it exists or null if it cannot be found.</returns>
        public static BaseMoonSpring<Unit> GetCustomSpring(Ref<float> referenceValue_)
        {
            var refs = new[] { referenceValue_ };
            return GetCustomSpring(refs);
        }

        /// <summary>
        /// Gets a custom Spring object for the provided reference value, if it exists.
        /// </summary>
        /// <typeparam name="Unit">The type of used for this reference value.</typeparam>
        /// <param name="referenceValues_">The reference value a Spring object is applied to.</param>
        /// <returns>Returns the requested Spring object if it exists or null if it cannot be found.</returns>
        public static BaseMoonSpring<Unit> GetCustomSpring(Ref<float>[] referenceValues_)
        {
            if (_customSprings.TryGetValue(referenceValues_, out var spring))
            {
                return spring;
            }

            return null;
        }

        /// <summary>
        /// Returns true when this object is complete.
        /// </summary>
        /// <returns>True when state is complete.</returns>
        public bool IsComplete()
        {
            return _currentState == MoonSpringState.Complete;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Calculates the necessary velocities to be applied to all Spring properties each game tick.
        /// </summary>
        protected abstract void CalculateForces();

        /// <summary>
        /// Applies force to properties each frame.
        /// </summary>
        /// <param name="deltaTime_">The time elapsed between last and current game tick.</param>
        protected abstract void ApplyForces(float deltaTime_);

        /// <summary>
        /// Determines if the minimum forces have been met to continue calculating Spring forces.
        /// </summary>
        /// <returns>Returns true if the minimum forces have been met.</returns>
        protected abstract bool MinimumForcesMet();

        /// <summary>
        /// Determines if there is a need to apply force to this Spring to meet target values.
        /// </summary>
        /// <returns>Returns true if forces need to be applied</returns>
        protected abstract bool NeedToApplyForce();

        /// <summary>
        /// Assigns the minimum force required until the Spring is completed based on inputs.
        /// </summary>
        protected abstract void SetMinimumForce();

        /// <summary>
        /// Snaps all Spring properties directly to their target values.
        /// </summary>
        protected abstract void SnapSpringToTarget();
        #endregion
    }
}