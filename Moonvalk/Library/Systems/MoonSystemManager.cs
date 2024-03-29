using System.Collections.Generic;

namespace Moonvalk.Systems
{
    /// <summary>
    /// A manager for handling all MoonSystems.
    /// </summary>
    public class MoonSystemManager
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MoonSystemManager()
        {
            Global.Systems = this;
            Global.RegisterSystems();
        }

        #region Data Fields
        /// <summary>
        /// A map that stores reference to all MoonSystems.
        /// </summary>
        private readonly List<IMoonSystem> _systemMap = new List<IMoonSystem>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Update method that runs each MoonSystem in order stored within _systemMap.
        /// </summary>
        /// <param name="delta_">The duration of time between last and current frame.</param>
        public void Update(float delta_)
        {
            foreach (var system in _systemMap)
            {
                system.Execute(delta_);
            }
        }

        /// <summary>
        /// Registers a new MoonSystem here.
        /// </summary>
        /// <param name="system_">The MoonSystem object to be registered.</param>
        public void RegisterSystem(IMoonSystem system_)
        {
            _systemMap.Add(system_);
        }

        /// <summary>
        /// Gets an MoonSystem stored within this manager by type.
        /// </summary>
        /// <typeparam name="Type">The type of the MoonSystem to find.</typeparam>
        /// <returns>Returns the matching MoonSystem of the type T, if possible.</returns>
        public IMoonSystem Get<Type>()
        {
            foreach (var system in _systemMap)
            {
                if (system.GetType() == typeof(Type))
                {
                    return system;
                }
            }

            return null;
        }

        /// <summary>
        /// Clears all System queues at once.
        /// </summary>
        public void ClearAllSystems()
        {
            foreach (var system in _systemMap)
            {
                system.Clear();
            }
        }
        #endregion
    }
}