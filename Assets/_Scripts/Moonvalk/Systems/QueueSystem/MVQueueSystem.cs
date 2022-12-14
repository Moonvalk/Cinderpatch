using System.Collections.Generic;
using UnityEngine;

namespace Moonvalk.Systems
{
    /// <summary>
    /// An abstract representation for a queue System that adds and removes updatable objects.
    /// </summary>
    /// <typeparam name="T">The type of System.</typeparam>
    public abstract class MVQueueSystem<T> : MVSystem<T>
    {
        #region Data Fields
        /// <summary>
        /// A list of all current queued items.
        /// </summary>
        protected List<IQueueItem> _queue;
        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MVQueueSystem()
        {
            this._queue = new List<IQueueItem>();
            base.initialize();
        }

        #region Public Methods
        /// <summary>
        /// Runs this System during each game tick.
        /// </summary>
        /// <param name="deltaTime_">The current delta between last and current frame.</param>
        public override void Execute(float deltaTime_)
        {
            // Cancel System execution when no objects exist to act upon.
            if (this._queue.Count == 0)
            {
                return;
            }
            for (int i = 0; i < this._queue.Count; i++)
            {
                IQueueItem item = this._queue[i];
                bool active = item.Update(deltaTime_);
                if (!active)
                {
                    this.Remove(item);
                    item.HandleTasks();
                }
            }
        }

        /// <summary>
        /// Gets all current queue items.
        /// </summary>
        /// <returns>Returns the full list of IQueueUpdateable items.</returns>
        public List<IQueueItem> GetAll()
        {
            return this._queue;
        }

        /// <summary>
        /// Removes all current Tweens.
        /// </summary>
        public void RemoveAll()
        {
            this._queue.Clear();
        }

        public override void Clear()
        {
            this.RemoveAll();
        }

        /// <summary>
        /// Adds an updatable item to the queue.
        /// </summary>
        /// <param name="itemToAdd_">The item to add.</param>
        public void Add(IQueueItem itemToAdd_)
        {
            if (this._queue.Contains(itemToAdd_))
            {
                return;
            }
            this._queue.Add(itemToAdd_);
        }

        /// <summary>
        /// Removes an updateable item from the queue.
        /// </summary>
        /// <param name="itemToRemove_">The item to remove.</param>
        public void Remove(IQueueItem itemToRemove_)
        {
            this._queue.Remove(itemToRemove_);
        }
        #endregion
    }
}
