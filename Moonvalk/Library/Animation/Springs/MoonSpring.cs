using System;
using Godot;
using Moonvalk.Accessory;
using Moonvalk.Utilities.Algorithms;

namespace Moonvalk.Animation
{
    /// <summary>
    /// Object that handles Spring calculations for a singular float value.
    /// </summary>
    public class MoonSpring : BaseMoonSpring<float>
    {
        /// <summary>
        /// Default constructor made without setting up references.
        /// </summary>
        public MoonSpring()
        {
            // ...
        }

        /// <summary>
        /// Constructor for creating a new Spring.
        /// </summary>
        /// <param name="referenceValues_">Array of references to float values.</param>
        public MoonSpring(params Ref<float>[] referenceValues_) : base(referenceValues_)
        {
            // ...
        }

        /// <summary>
        /// Calculates the necessary velocities to be applied to all Spring properties each game tick.
        /// </summary>
        protected override void CalculateForces()
        {
            for (var i = 0; i < _properties.Length; i++)
            {
                if (_properties[i] == null)
                {
                    Delete();
                    break;
                }

                var displacement = _targetProperties[i] - _properties[i]();
                _currentForce[i] = MotionAlgorithms.SimpleHarmonicMotion(_tension, displacement, _dampening, _speed[i]);
            }
        }

        /// <summary>
        /// Applies force to properties each frame.
        /// </summary>
        /// <param name="deltaTime_">The time elapsed between last and current game tick.</param>
        protected override void ApplyForces(float deltaTime_)
        {
            for (var i = 0; i < _properties.Length; i++)
            {
                _speed[i] += _currentForce[i] * deltaTime_;
                _properties[i]() += _speed[i] * deltaTime_;
            }
        }

        /// <summary>
        /// Determines if the minimum forces have been met to continue calculating Spring forces.
        /// </summary>
        /// <returns>Returns true if the minimum forces have been met.</returns>
        protected override bool MinimumForcesMet()
        {
            for (var index = 0; index < _currentForce.Length; index++)
            {
                var metTarget = Mathf.Abs(_targetProperties[index] - _properties[index]()) >= _minimumForce[index];
                var metMinimumForce = Mathf.Abs(_currentForce[index] + _speed[index]) >= _minimumForce[index];
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
            _minimumForce = new float[_properties.Length];
            for (var index = 0; index < _properties.Length; index++)
            {
                _minimumForce[index] = DefaultMinimumForcePercentage * Mathf.Abs(_targetProperties[index] - _properties[index]());
            }
        }

        /// <summary>
        /// Determines if there is a need to apply force to this Spring to meet target values.
        /// </summary>
        /// <returns>Returns true if forces need to be applied</returns>
        protected override bool NeedToApplyForce()
        {
            const float tolerance = 0.001f;
            for (var index = 0; index < _properties.Length; index++)
            {
                if (Math.Abs(_properties[index]() - _targetProperties[index]) > tolerance)
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
            for (var index = 0; index < _properties.Length; index++)
            {
                _properties[index]() = _targetProperties[index];
            }
        }
    }
}