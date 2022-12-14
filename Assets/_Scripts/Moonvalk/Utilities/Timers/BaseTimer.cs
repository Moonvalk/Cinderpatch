using System;
using System.Collections.Generic;
using Moonvalk.Accessory;
using Moonvalk.Systems;
using UnityEngine;

namespace Moonvalk.Utilities
{
    /// <summary>
    /// A configurable timer object with additional functionality for referencing tasks.
    /// </summary>
    public abstract class BaseTimer : ITimer
    {
        #region Data Fields
        /// <summary>
        /// The duration in seconds this timer takes to complete.
        /// </summary>
        protected float _duration;

        /// <summary>
        /// The time remaining in seconds.
        /// </summary>
        protected float _timeRemaining;

        /// <summary>
        /// A multiplier to be applied to delta time.
        /// </summary>
        protected float _timeScale = 1f;

        /// <summary>
        /// The current timer state.
        /// </summary>
        protected BaseTimerState _currentState = BaseTimerState.Idle;

        /// <summary>
        /// A map of all tasks that need to be completed by each available timer state.
        /// </summary>
        protected Dictionary<BaseTimerState, InitValue<List<Action>>> _functions;
        #endregion

        #region Public Getters/Setters
        /// <summary>
        /// Returns true when this Timer is complete.
        /// </summary>
        /// <value>Returns a boolean value representing whether this Timer has completed.</value>
        public bool IsComplete
        {
            get
            {
                return this._currentState == BaseTimerState.Complete;
            }
        }

        /// <summary>
        /// Returns true when this Timer is actively running.
        /// </summary>
        /// <value>Returns a boolean value representing whether this Timer is running.</value>
        public bool IsRunning
        {
            get
            {
                return (this._currentState == BaseTimerState.Start || this._currentState == BaseTimerState.Update);
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor taking no additional properties.
        /// </summary>
        public BaseTimer()
        {
            this.initialize();
        }

        /// <summary>
        /// Constructor that allows the user to set a duration.
        /// </summary>
        /// <param name="duration_">The duration in seconds that this timer will run for.</param>
        public BaseTimer(float duration_)
        {
            this.initialize();
            this.Duration(duration_);
        }

        /// <summary>
        /// Constructor that allows the user to set completion tasks.
        /// </summary>
        /// <param name="onCompleteTasks_">Tasks to run on completion.</param>
        public BaseTimer(params Action[] onCompleteTasks_)
        {
            this.initialize();
            this.OnComplete(onCompleteTasks_);
        }

        /// <summary>
        /// Constructor that allows the user to set a duration and completion tasks.
        /// </summary>
        /// <param name="duration_">The duration in seconds that this timer will run for.</param>
        /// <param name="onCompleteTasks_">Tasks to run on completion.</param>
        public BaseTimer(float duration_, params Action[] onCompleteTasks_)
        {
            this.initialize();
            this.Duration(duration_);
            this.OnComplete(onCompleteTasks_);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets the duration of this Timer.
        /// </summary>
        /// <param name="duration_">The duration in seconds that need to elapse while this Timer runs.</param>
        /// <returns>Returns this MVTimer object.</returns>
        public BaseTimer Duration(float duration_)
        {
            this._duration = duration_;
            return this;
        }

        /// <summary>
        /// Adds Actions that will be run when this MVTimer is complete.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add for this state.</param>
        /// <returns>Returns this MVTimer object.</returns>
        public BaseTimer OnComplete(params Action[] tasksToAdd_)
        {
            this.addTasks(BaseTimerState.Complete, tasksToAdd_);
            return this;
        }

        /// <summary>
        /// Adds Actions that will be run when this MVTimer is started.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add for this state.</param>
        /// <returns>Returns this MVTimer object.</returns>
        public BaseTimer OnStart(params Action[] tasksToAdd_)
        {
            this.addTasks(BaseTimerState.Start, tasksToAdd_);
            return this;
        }

        /// <summary>
        /// Adds Actions that will be run when this MVTimer is updated.
        /// </summary>
        /// <param name="tasksToAdd_">Array of Actions to add for this state.</param>
        /// <returns>Returns this MVTimer object.</returns>
        public BaseTimer OnUpdate(params Action[] tasksToAdd_)
        {
            this.addTasks(BaseTimerState.Update, tasksToAdd_);
            return this;
        }

        /// <summary>
        /// Starts this Timer with the latest configured settings.
        /// </summary>
        /// <returns>This MVTimer object.</returns>
        public BaseTimer Start()
        {
            this._timeRemaining = this._duration;
            this._currentState = BaseTimerState.Start;
            this.handleTasks(_currentState);
            (Global.GetSystem<TimerSystem>() as TimerSystem).Add(this);
            return this;
        }

        public BaseTimer Start(float duration_)
        {
            this.Duration(duration_);
            return this.Start();
        }

        /// <summary>
        /// Stops this Timer.
        /// </summary>
        public void Stop()
        {
            this._currentState = BaseTimerState.Stopped;
        }

        /// <summary>
        /// Pauses this Timer.
        /// </summary>
        public void Paused()
        {
            this._currentState = BaseTimerState.Idle;
        }

        /// <summary>
        /// Resumes this Timer from wherever last left off.
        /// </summary>
        public void Resume()
        {
            this._currentState = BaseTimerState.Update;
        }

        public void SetTimeScale(float timeScale_)
        {
            if (timeScale_ < 0f)
            {
                return;
            }
            this._timeScale = timeScale_;
        }

        /// <summary>
        /// Updates this Timer.
        /// </summary>
        /// <param name="deltaTime_">Duration of time taken since last and current game tick.</param>
        /// <returns>Returns true when actively running and false once complete.</returns>
        public bool Update(float deltaTime_)
        {
            if (this._currentState == BaseTimerState.Complete || this._currentState == BaseTimerState.Stopped)
            {
                return false;
            }
            if (this._currentState == BaseTimerState.Idle)
            {
                return true;
            }

            this._currentState = BaseTimerState.Update;
            this.handleTasks(this._currentState);
            bool complete = this.runTimer(deltaTime_);
            if (complete)
            {
                this._currentState = BaseTimerState.Complete;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Handles tasks for the current state.
        /// </summary>
        public void HandleTasks()
        {
            this.handleTasks(this._currentState);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes this object.
        /// </summary>
        protected void initialize()
        {
            this._functions = new Dictionary<BaseTimerState, InitValue<List<Action>>>();
            foreach (BaseTimerState state in Enum.GetValues(typeof(BaseTimerState)))
            {
                this._functions.Add(state, new InitValue<List<Action>>(() => { return new List<Action>(); }));
            }
        }

        /// <summary>
        /// Adds an array of new Actions to a TweenState.
        /// </summary>
        /// <param name="state_">The TweenState to add tasks for.</param>
        /// <param name="tasksToAdd_">The tasks to add.</param>
        protected void addTasks(BaseTimerState state_, params Action[] tasksToAdd_)
        {
            foreach (Action task in tasksToAdd_)
            {
                _functions[state_].Value.Add(task);
            }
        }

        /// <summary>
        /// Runs this MVTimer object.
        /// </summary>
        /// <param name="deltaTime_">Duration of time between last and current game tick.</param>
        /// <returns>Returns true when complete or false when actively running.</returns>
        protected bool runTimer(float deltaTime_)
        {
            this._timeRemaining -= (deltaTime_ * this._timeScale);
            if (this._timeRemaining <= 0f)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles all tasks for the specified state.
        /// </summary>
        /// <param name="state_">The state to run tasks for.</param>
        protected void handleTasks(BaseTimerState state_)
        {
            foreach (Action action in _functions[state_].Value)
            {
                action();
            }
        }
        #endregion
    }
}
