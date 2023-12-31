using Godot;
using Moonvalk.Accessory;

namespace Moonvalk.Animation
{
    /// <summary>
    /// A basic Tween which handles Color value properties.
    /// </summary>
    public class MoonTweenColor : BaseMoonTween<Color>
    {
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        public MoonTweenColor()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new Tween.
        /// </summary>
        /// <param name="referenceValues_">Array of references to float values.</param>
        public MoonTweenColor(params Ref<float>[] referenceValues_) : base(referenceValues_)
        {
            // ...
        }

        /// <summary>
        /// Method used to update all properties available to this object.
        /// </summary>
        protected override void UpdateProperties()
        {
            // Apply easing and set properties.
            for (var index = 0; index < _properties.Length; index += 4)
            {
                if (_properties[index] == null)
                {
                    Stop();
                    break;
                }

                _properties[index]() = _easingFunctions[index](_percentage, _startValues[index].r, _targetValues[index].r);
                _properties[index + 1]() = _easingFunctions[index](_percentage, _startValues[index].g, _targetValues[index].g);
                _properties[index + 2]() = _easingFunctions[index](_percentage, _startValues[index].b, _targetValues[index].b);
                _properties[index + 3]() = _easingFunctions[index](_percentage, _startValues[index].a, _targetValues[index].a);
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