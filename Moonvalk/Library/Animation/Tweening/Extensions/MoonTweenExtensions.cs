using System;
using System.Collections.Generic;
using Godot;
using Moonvalk.Nodes;

namespace Moonvalk.Animation
{
    /// <summary>
    /// Contains extension methods made for animating Godot.Node objects with the use of MoonTweens.
    /// </summary>
    public static class MoonTweenExtensions
    {
        /// <summary>
        /// Stores reference to all MoonTweenGroups used to manage MoonTweenHandlers by the manipulated Godot.Node.
        /// Each node being animated is assigned its own group.
        /// </summary>
        private static readonly Dictionary<Node, MoonTweenGroup> TweenGroups = new Dictionary<Node, MoonTweenGroup>();

        /// <summary>
        /// Gets the matching MoonTweenGroup for the given Godot object, when available. If no group
        /// exists a new one will be created.
        /// </summary>
        /// <param name="object_">The object to receive a group for.</param>
        /// <returns>Returns the matching MoonTweenGroup for the input object.</returns>
        public static MoonTweenGroup GetMoonTweenGroup(Node object_)
        {
            MoonTweenGroup group;
            if (TweenGroups.TryGetValue(object_, out var tweenGroup))
            {
                group = tweenGroup;
            }
            else
            {
                group = new MoonTweenGroup();
                TweenGroups.Add(object_, group);
            }

            return group;
        }

        /// <summary>
        /// Gets an Action used to remove the provided object / property pair Tween handler once an
        /// animation is finished.
        /// </summary>
        /// <param name="object_">The object to create an action for.</param>
        /// <param name="property_">The property to create an action for.</param>
        /// <returns>A new Action that will remove properties from the map once complete.</returns>
        public static Action GetRemoveAction(Node object_, MoonTweenProperty property_)
        {
            return () =>
            {
                if (!object_.Validate())
                {
                    Delete(object_);
                    return;
                }

                if (TweenGroups.ContainsKey(object_))
                {
                    var group = TweenGroups[object_];
                    if (group.TweenHandlers.Remove(property_) && group.TweenHandlers.Count == 0)
                    {
                        TweenGroups.Remove(object_);
                    }
                }
            };
        }

        /// <summary>
        /// Clears all Tweens currently being applied to nodes.
        /// </summary>
        public static void ClearAll()
        {
            if (TweenGroups != null)
            {
                foreach (var group in TweenGroups.Values)
                {
                    group?.Clear();
                }

                TweenGroups.Clear();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="node_"></param>
        public static void Delete(Node node_)
        {
            if (TweenGroups.ContainsKey(node_))
            {
                TweenGroups[node_]
                    .Clear();
                TweenGroups.Remove(node_);
            }
        }
    }
}