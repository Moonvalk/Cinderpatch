using Godot;
using Moonvalk.Accessory;
using Moonvalk.Utilities.Algorithms;

namespace Moonvalk.Animation
{
    /// <summary>
    /// Object that handles Spring calculations for a Vector2 value.
    /// </summary>
    public class MoonSpringVec2 : BaseMoonSpring<Vector2>
    {
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        public MoonSpringVec2()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new Spring.
        /// </summary>
        /// <param name="referenceValues_">Array of references to Vector2 values.</param>
        public MoonSpringVec2(params Ref<float>[] referenceValues_) : base(referenceValues_)
        {
            // ...
        }

        /// <summary>
        /// Calculates the necessary velocities to be applied to all Spring properties each game tick.
        /// </summary>
        protected override void CalculateForces()
        {
            for (var index = 0; index < _properties.Length; index += 2)
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
            }
        }

        /// <summary>
        /// Applies force to properties each frame.
        /// </summary>
        /// <param name="deltaTime_">The time elapsed between last and current game tick.</param>
        protected override void ApplyForces(float deltaTime_)
        {
            for (var index = 0; index < _properties.Length; index += 2)
            {
                _speed[index].x += _currentForce[index].x * deltaTime_;
                _speed[index].y += _currentForce[index].y * deltaTime_;
                _properties[index]() += _speed[index].x * deltaTime_;
                _properties[index + 1]() += _speed[index].y * deltaTime_;
            }
        }

        /// <summary>
        /// Determines if the minimum forces have been met to continue calculating Spring forces.
        /// </summary>
        /// <returns>Returns true if the minimum forces have been met.</returns>
        protected override bool MinimumForcesMet()
        {
            for (var index = 0; index < _currentForce.Length; index += 2)
            {
                var current = new Vector2(_properties[index](), _properties[index + 1]());
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
            _minimumForce = new Vector2[_properties.Length];
            for (var index = 0; index < _properties.Length; index += 2)
            {
                var current = new Vector2(_properties[index](), _properties[index + 1]());
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
            for (var index = 0; index < _properties.Length; index += 2)
            {
                var current = new Vector2(_properties[index](), _properties[index + 1]());
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
            for (var index = 0; index < _properties.Length; index += 2)
            {
                _properties[index]() = _targetProperties[index].x;
                _properties[index + 1]() = _targetProperties[index].y;
            }
        }
    }
}