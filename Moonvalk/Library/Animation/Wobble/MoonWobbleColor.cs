using Godot;
using Moonvalk.Accessory;

namespace Moonvalk.Animation
{
    /// <summary>
    /// A basic Wobble which handles Color values.
    /// </summary>
    public class MoonWobbleColor : BaseMoonWobble<Color>
    {
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        public MoonWobbleColor()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new Wobble.
        /// </summary>
        /// <param name="referenceValues_">Array of references to float values.</param>
        public MoonWobbleColor(params Ref<float>[] referenceValues_) : base(referenceValues_)
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
            for (var index = 0; index < _properties.Length; index += 4)
            {
                if (_properties[index] == null)
                {
                    Delete();
                    break;
                }

                _properties[index]() = _startValues[index].r + wave * _percentage.r;
                _properties[index + 1]() = _startValues[index].g + wave * _percentage.g;
                _properties[index + 2]() = _startValues[index].b + wave * _percentage.b;
                _properties[index + 3]() = _startValues[index].a + wave * _percentage.a;
            }
        }

        /// <summary>
        /// Updates all starting values set the reference property values.
        /// </summary>
        protected override void UpdateStartValues()
        {
            for (var index = 0; index < _properties.Length; index += 4)
            {
                _startValues[index].r = _properties[index]();
                _startValues[index].g = _properties[index + 1]();
                _startValues[index].b = _properties[index + 2]();
                _startValues[index].a = _properties[index + 3]();
            }
        }
    }
}