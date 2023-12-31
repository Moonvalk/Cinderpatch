using Godot;
using Moonvalk.Accessory;

namespace Moonvalk.Animation
{
    /// <summary>
    /// A basic Wobble which handles Vector2 values.
    /// </summary>
    public class MoonWobbleVec2 : BaseMoonWobble<Vector2>
    {
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        public MoonWobbleVec2()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new Wobble.
        /// </summary>
        /// <param name="referenceValues_">Array of references to float values.</param>
        public MoonWobbleVec2(params Ref<float>[] referenceValues_) : base(referenceValues_)
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
            for (var index = 0; index < _properties.Length; index += 2)
            {
                if (_properties[index] == null)
                {
                    Delete();
                    break;
                }

                _properties[index]() = _startValues[index].x + wave * _percentage.x;
                _properties[index + 1]() = _startValues[index].y + wave * _percentage.y;
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