using Godot;
using Moonvalk.Accessory;

namespace Moonvalk.Animation
{
    /// <summary>
    /// A Tween object which handles Vector2 values.
    /// </summary>
    public class MoonTweenVec2 : BaseMoonTween<Vector2>
    {
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        public MoonTweenVec2()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new Tween.
        /// </summary>
        /// <param name="referenceValues_">Array of references to float values.</param>
        public MoonTweenVec2(params Ref<float>[] referenceValues_) : base(referenceValues_)
        {
            // ...
        }

        /// <summary>
        /// Method used to update all properties available to this object.
        /// </summary>
        protected override void UpdateProperties()
        {
            // Apply easing and set properties.
            for (var index = 0; index < _properties.Length; index += 2)
            {
                if (_properties[index] == null)
                {
                    Delete();
                    break;
                }

                _properties[index]() = _easingFunctions[index](_percentage, _startValues[index].x, _targetValues[index].x);
                _properties[index + 1]() = _easingFunctions[index](_percentage, _startValues[index].y, _targetValues[index].y);
            }
        }

        /// <summary>
        /// Updates all starting values set the reference property values.
        /// </summary>
        protected override void UpdateStartValues()
        {
            for (var index = 0; index < _properties.Length; index += 2)
            {
                _startValues[index].x = _properties[index]();
                _startValues[index].y = _properties[index + 1]();
            }
        }
    }
}