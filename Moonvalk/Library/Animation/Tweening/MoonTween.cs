using Moonvalk.Accessory;

namespace Moonvalk.Animation
{
    /// <summary>
    /// A basic Tween which handles singular float value properties.
    /// </summary>
    public class MoonTween : BaseMoonTween<float>
    {
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        public MoonTween()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new Tween.
        /// </summary>
        /// <param name="referenceValues_">Array of references to float values.</param>
        public MoonTween(params Ref<float>[] referenceValues_) : base(referenceValues_)
        {
            // ...
        }

        /// <summary>
        /// Method used to update all properties available to this object.
        /// </summary>
        protected override void UpdateProperties()
        {
            // Apply easing and set properties.
            for (var index = 0; index < _properties.Length; index++)
            {
                if (_properties[index] == null)
                {
                    Delete();
                    break;
                }

                _properties[index]() = _easingFunctions[index](_percentage, _startValues[index], _targetValues[index]);
            }
        }

        /// <summary>
        /// Updates all starting values set the reference property values.
        /// </summary>
        protected override void UpdateStartValues()
        {
            for (var index = 0; index < _properties.Length; index++)
            {
                _startValues[index] = _properties[index]();
            }
        }
    }
}