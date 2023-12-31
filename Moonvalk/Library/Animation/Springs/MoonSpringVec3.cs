using Godot;
using Moonvalk.Accessory;
using Moonvalk.Utilities.Algorithms;

namespace Moonvalk.Animation
{
    /// <summary>
    /// Object that handles Spring calculations for a Vector3 value.
    /// </summary>
    public class MoonSpringVec3 : BaseMoonSpring<Vector3>
    {
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        public MoonSpringVec3()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new Spring.
        /// </summary>
        /// <param name="referenceValues_">Array of references to Vector3 values.</param>
        public MoonSpringVec3(params Ref<float>[] referenceValues_) : base(referenceValues_)
        {
            // ...
        }

        /// <summary>
        /// Calculates the necessary velocities to be applied to all Spring properties each game tick.
        /// </summary>
        protected override void CalculateForces()
        {
            for (var index = 0; index < _properties.Length; index += 3)
            {
                if (_properties[index] == null)
                {
                    Delete();
                    break;
                }

                var displacement = _targetProperties[index].x - _properties[index]();
                _currentForce[index].x = MotionAlgorithms.SimpleHarmonicMotion(_tension, displacement, _dampening, _speed[index].x);
                displacement = _targetProperties[index].y - _properties[index + 1]();
                _currentForce[index].y = MotionAlgorithms.SimpleHarmonicMotion(_tension, displacement, _dampening, _speed[index].y);
                displacement = _targetProperties[index].z - _properties[index + 2]();
                _currentForce[index].z = MotionAlgorithms.SimpleHarmonicMotion(_tension, displacement, _dampening, _speed[index].z);
            }
        }

        /// <summary>
        /// Applies force to properties each frame.
        /// </summary>
        /// <param name="deltaTime_">The time elapsed between last and current game tick.</param>
        protected override void ApplyForces(float deltaTime_)
        {
            for (var index = 0; index < _properties.Length; index += 3)
            {
                _speed[index].x += _currentForce[index].x * deltaTime_;
                _speed[index].y += _currentForce[index].y * deltaTime_;
                _speed[index].z += _currentForce[index].z * deltaTime_;
                _properties[index]() += _speed[index].x * deltaTime_;
                _properties[index + 1]() += _speed[index].y * deltaTime_;
                _properties[index + 2]() += _speed[index].z * deltaTime_;
            }
        }

        /// <summary>
        /// Determines if the minimum forces have been met to continue calculating Spring forces.
        /// </summary>
        /// <returns>Returns true if the minimum forces have been met.</returns>
        protected override bool MinimumForcesMet()
        {
            for (var index = 0; index < _currentForce.Length; index += 3)
            {
                var current = new Vector3(_properties[index](), _properties[index + 1](), _properties[index + 2]());
                var metTarget = ConversionHelpers.Abs(_targetProperties[index] - current) >= _minimumForce[index];
                var metMinimumForce = ConversionHelpers.Abs(_currentForce[index] + _speed[index]) >= _minimumForce[index];
                if (metTarget && metMinimumForce)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Assigns the minimum force required until the Spring is completed based on inputs.
        /// </summary>
        protected override void SetMinimumForce()
        {
            _minimumForce = new Vector3[_properties.Length];
            for (var index = 0; index < _properties.Length; index += 3)
            {
                var current = new Vector3(_properties[index](), _properties[index + 1](), _properties[index + 2]());
                _minimumForce[index] = MoonSpring.DefaultMinimumForcePercentage *
                                           ConversionHelpers.Abs(_targetProperties[index] - current);
            }
        }

        /// <summary>
        /// Determines if there is a need to apply force to this Spring to meet target values.
        /// </summary>
        /// <returns>Returns true if forces need to be applied</returns>
        protected override bool NeedToApplyForce()
        {
            for (var index = 0; index < _properties.Length; index += 3)
            {
                var current = new Vector3(_properties[index](), _properties[index + 1](), _properties[index + 2]());
                if (current != _targetProperties[index])
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Snaps all Spring properties directly to their target values.
        /// </summary>
        protected override void SnapSpringToTarget()
        {
            for (var index = 0; index < _properties.Length; index += 3)
            {
                _properties[index]() = _targetProperties[index].x;
                _properties[index + 1]() = _targetProperties[index].y;
                _properties[index + 2]() = _targetProperties[index].z;
            }
        }
    }
}