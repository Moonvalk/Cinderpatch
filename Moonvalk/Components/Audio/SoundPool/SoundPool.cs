using System.Collections.Generic;
using System.Linq;
using Godot;
using Moonvalk.Components;

namespace Moonvalk.Audio
{
    /// <summary>
    /// Container for holding a pool of SoundQueue objects. This pool can be
    /// cycled through for randomized playback.
    /// </summary>
    [Tool]
    public class SoundPool : Node
    {
        /// <summary>
        /// List of all sound queues that are stored as child nodes.
        /// </summary>
        private List<SoundQueue> _sounds = new List<SoundQueue>();

        /// <summary>
        /// Generator for getting random sound effects.
        /// </summary>
        private readonly RandomNumberGenerator _random = new RandomNumberGenerator();

        /// <summary>
        /// Stores reference to the previously played index.
        /// </summary>
        private int _previousIndex = -1;

        /// <summary>
        /// Called when this object is initialized.
        /// </summary>
        public override void _Ready()
        {
            _sounds = this.GetAllComponents<SoundQueue>();
        }

        /// <summary>
        /// Called by the Godot editor to get potential warnings with setup.
        /// </summary>
        /// <returns>A string representing a warning with current configuration.</returns>
        public override string _GetConfigurationWarning()
        {
            var numberOfSoundQueues = GetChildren()
                .OfType<SoundQueue>()
                .Count();

            return numberOfSoundQueues < 2 ? "Expected two or more children of type SoundQueue." : "";
        }

        /// <summary>
        /// Called to play a new random sound effect within this pool.
        /// </summary>
        public void PlayRandomSound()
        {
            int index;
            do
            {
                index = _random.RandiRange(0, _sounds.Count - 1);
            } while (index == _previousIndex);

            _previousIndex = index;
            _sounds[index].PlaySound();
        }
    }
}