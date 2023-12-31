using Godot;
using Moonvalk.Accessory;

namespace Moonvalk.Animation
{
    /// <summary>
    /// A basic Wobble which handles Vector3 values.
    /// </summary>
    public class MoonWobbleVec3 : BaseMoonWobble<Vector3>
    {
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        public MoonWobbleVec3()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new Wobble.
        /// </summary>
        /// <param name="referenceValues_">Array of references to float values.</param>
        public MoonWobbleVec3(params Ref<float>[] referenceValues_) : base(referenceValues_)
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
            for (var index = 0; index < _properties.Length; index += 3)
            {
                if (_properties[index] == null)
                {
                    Delete();
                    break;
                }

                _properties[index]() = _startValues[index].x + wave * _percentage.x;
                _properties[index + 1]() = _startValues[index].y + wave * _percentage.y;
                _properties[index + 2]() = _startValues[index].z + wave * _percentage.z;
            }
        }

        /// <summary>
        /// Updates all starting values set the reference property values.
        /// </summary>
        protected override void UpdateStartValues()
        {
            for (var index = 0; index < _properties.Length; index += 3)
            {
                _startValues[index].x = _properties[index]();
                _startValues[index].y = _properties[index + 1]();
                _startValues[index].z = _properties[index + 2]();
            }
        }
    }
}