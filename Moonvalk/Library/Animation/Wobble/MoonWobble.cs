using Godot;
using Moonvalk.Accessory;

namespace Moonvalk.Animation
{
    /// <summary>
    /// A basic Wobble which handles float values.
    /// </summary>
    public class MoonWobble : BaseMoonWobble<float>
    {
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        public MoonWobble()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new Wobble.
        /// </summary>
        /// <param name="referenceValues_">Array of references to float values.</param>
        public MoonWobble(params Ref<float>[] referenceValues_) : base(referenceValues_)
        {
            // ...
        }

        /// <summary>
        /// Method used to update all properties available to this object.
        /// </summary>
        protected override void UpdateProperties()
        {
            // Apply easing and set properties.
            var wave = Mathf.Sin(_time * _frequency) * _amplitude * _strength;
            for (var index = 0; index < _properties.Length; index++)
            {
                if (_properties[index] == null)
                {
                    Delete();
                    break;
                }

                _properties[index]() = _startValues[index] + wave * _percentage;
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