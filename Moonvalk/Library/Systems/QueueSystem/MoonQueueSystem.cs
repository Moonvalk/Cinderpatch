using System.Collections.Generic;

namespace Moonvalk.Systems
{
    /// <summary>
    /// An abstract representation for a queue System that adds and removes updatable objects.
    /// </summary>
    /// <typeparam name="Type">The type of System.</typeparam>
    public abstract class MoonQueueSystem<Type> : MoonSystem<Type>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected MoonQueueSystem()
        {
            _removalQueue = new List<IQueueItem>();
            _queue = new List<IQueueItem>();
            Initialize();
        }

        #region Data Fields
        /// <summary>
        /// A list of all current queued items.
        /// </summary>
        private readonly List<IQueueItem> _queue;

        /// <summary>
        /// A queue of all items that will be removed on the following frame.
        /// </summary>
        private readonly List<IQueueItem> _removalQueue;
        #endregion

        #region Public Methods
        /// <summary>
        /// Runs this System during each game tick.
        /// </summary>
        /// <param name="delta_">The current delta between last and current frame.</param>
        public override void Execute(float delta_)
        {
            // Remove elements from the RemovalQueue.
            if (_removalQueue.Count > 0)
            {
                for (var index = 0; index < _removalQueue.Count; index++)
                {
                    _queue.Remove(_removalQueue[index]);
                }

                _removalQueue.Clear();
            }

            // Cancel System execution when no objects exist to act upon.
            if (_queue.Count == 0)
            {
                return;
            }

            for (var index = 0; index < _queue.Count; index++)
            {
                var item = _queue[index];
                if (!item.Update(delta_))
                {
                    item.HandleTasks();
                    if (item.IsComplete())
                    {
                        _removalQueue.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all current queue items.
        /// </summary>
        /// <returns>Returns the full list of IQueueItem items.</returns>
        public List<IQueueItem> GetAll()
        {
            return _queue;
        }

        /// <summary>
        /// Removes all current queued items.
        /// </summary>
        public void RemoveAll()
        {
            _queue.Clear();
        }

        /// <summary>
        /// Clears the queue applied to this system.
        /// </summary>
        public override void Clear()
        {
            RemoveAll();
        }

        /// <summary>
        /// Adds an updatable item to the queue.
        /// </summary>
        /// <param name="itemToAdd_">The item to add.</param>
        public void Add(IQueueItem itemToAdd_)
        {
            if (_queue.Contains(itemToAdd_))
            {
                return;
            }

            _queue.Add(itemToAdd_);
        }

        /// <summary>
        /// Removes an update-able item from the queue.
        /// </summary>
        /// <param name="itemToRemove_">The item to remove.</param>
        public void Remove(IQueueItem itemToRemove_)
        {
            _queue.Remove(itemToRemove_);
        }
        #endregion
    }
}