using System;

namespace Moonvalk.Animation
{
    /// <summary>
    /// Base class for MoonTweenHandlers. These are containers that automate updating
    /// Node data each game tick with the use of extension methods.
    /// </summary>
    /// <typeparam name="Unit">The type of unit that will be animated.</typeparam>
    /// <typeparam name="TweenType">The type of MoonTween object that will be used.</typeparam>
    /// <typeparam name="ParentType">The type of Node which is being animated.</typeparam>
    public abstract class BaseMoonTweenHandler<Unit, TweenType, ParentType> : IMoonTweenHandler
        where TweenType : BaseMoonTween<Unit>, new()
    {
        /// <summary>
        /// The values used for animating properties.
        /// </summary>
        protected float[] _values;

        /// <summary>
        /// Constructor for a new handler.
        /// </summary>
        /// <param name="object_">The object that will be animated.</param>
        /// <param name="property_">The property found on object_ that will be manipulated.</param>
        /// <param name="target_">The target value to be animated to.</param>
        /// <param name="parameters_">Properties that determine how the animation will look.</param>
        /// <param name="start_">Flag that determines if this animation should begin immediately.</param>
        /// <param name="onComplete_">
        /// An action to be run when this Tween is complete. This is primarily used
        /// to remove a Tween reference once finished.
        /// </param>
        protected BaseMoonTweenHandler(
            ParentType object_,
            MoonTweenProperty property_,
            Unit target_,
            MoonTweenParams parameters_,
            bool start_,
            Action onComplete_
        )
        {
            BindData(object_, property_);
            TrySetupTween(target_, parameters_, start_, onComplete_);
        }

        /// <summary>
        /// The property this container will animate upon.
        /// </summary>
        public MoonTweenProperty Property { get; protected set; }

        /// <summary>
        /// Any actions that will occur during Tween updates managed by this container.
        /// </summary>
        protected Action _onUpdate;

        /// <summary>
        /// Stores the Tween object being managed.
        /// </summary>
        protected TweenType _tween;

        /// <summary>
        /// Gets the Tween found within this container casted to the requested type if applicable.
        /// </summary>
        /// <typeparam name="TweenValue">The type of value being animated.</typeparam>
        /// <returns>Returns the Tween casted to the requested type if available.</returns>
        public IMoonTween<TweenValue> GetTween<TweenValue>()
        {
            return (IMoonTween<TweenValue>)_tween;
        }

        /// <summary>
        /// Deletes the Tween found within this container.
        /// </summary>
        public void Delete()
        {
            _tween?.Delete();
            _tween = null;
            _onUpdate = null;
        }

        /// <summary>
        /// Starts the Tween found within this container.
        /// </summary>
        public void Start()
        {
            _tween.Start();
        }

        /// <summary>
        /// Called to bind required data to this container on initialization.
        /// </summary>
        /// <param name="object_">The object that this handler will manipulate.</param>
        /// <param name="property_">The property that will be adjusted.</param>
        private void BindData(ParentType object_, MoonTweenProperty property_)
        {
            _values = GetInitialPropertyValues(object_, property_);
            _onUpdate = AssignUpdateAction(object_, property_);
        }

        /// <summary>
        /// Called to set up new Tween objects managed by this container.
        /// </summary>
        /// <param name="target_">The target value to be animated to.</param>
        /// <param name="parameters_">Parameters used to manipulate how the animation will play.</param>
        /// <param name="start_">Flag that determines if this animation should begin immediately.</param>
        /// <param name="onComplete_">
        /// An action to be run when this Tween is complete. This is primarily used
        /// to remove a Tween reference once finished.
        /// </param>
        private void TrySetupTween(Unit target_, MoonTweenParams parameters_, bool start_, Action onComplete_)
        {
            SetupTween(target_, parameters_, start_, onComplete_);
        }

        /// <summary>
        /// Called to set up new Tween objects managed by this container.
        /// </summary>
        /// <param name="target_">The target value to be animated to.</param>
        /// <param name="parameters_">Parameters used to manipulate how the animation will play.</param>
        /// <param name="start_">Flag that determines if this animation should begin immediately.</param>
        /// <param name="onComplete_">
        /// An action to be run when this Tween is complete. This is primarily used
        /// to remove a Tween reference once finished.
        /// </param>
        protected abstract void SetupTween(Unit target_, MoonTweenParams parameters_, bool start_, Action onComplete_);

        /// <summary>
        /// Called to assign a new Action called each game tick during animations that will
        /// manipulate the Node property.
        /// </summary>
        /// <param name="object_">The object that this handler will manipulate.</param>
        /// <param name="property_">The property to be adjusted.</param>
        /// <returns>Returns a new Action.</returns>
        protected abstract Action AssignUpdateAction(ParentType object_, MoonTweenProperty property_);

        /// <summary>
        /// Gets the starting value for the Tween object to begin at.
        /// </summary>
        /// <param name="object_">The object that this handler will manipulate.</param>
        /// <param name="property_">The property to be adjusted.</param>
        /// <returns>Returns the initial value.</returns>
        protected abstract float[] GetInitialPropertyValues(ParentType object_, MoonTweenProperty property_);
    }
}