#if (TOOLS)
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Moonvalk.Addons
{
    [Tool]
    public class CustomNodesPlugin : EditorPlugin
    {
        #region Data Fields
        /// <summary>
        /// Stores reference to the refresh button within the Godot editor to load custom nodes.
        /// </summary>
        private Button _registerNodesButton;
        
        /// <summary>
        /// Path to the custom nodes list.
        /// </summary>
        private const string CustomNodesContainerLocation = "res://Addons/CustomNodes/CustomNodes.tres";
        
        /// <summary>
        /// Text displayed on the refresh nodes button.
        /// </summary>
        private const string RegisterNodesText = "Register Nodes";
        #endregion

        private static bool GetCustomNodes(out List<CustomNodeResource> customNodes_)
        {
            var customNodeHolder = GD.Load<CustomNodesContainer>(CustomNodesContainerLocation);
            if (customNodeHolder == null || customNodeHolder.Lists.Count == 0)
            {
                customNodes_ = null;
                return false;
            }

            customNodes_ = customNodeHolder.Lists.SelectMany(list => list.Nodes).ToList();
            return customNodes_.Count != 0;
        }
        
        /// <summary>
        /// Called to refresh registered custom nodes on user request.
        /// </summary>
        private void RefreshCustomNodes()
        {
            GD.Print("\nRefreshing Registered Custom Nodes...");
            UnregisterCustomNodes();
            RegisterCustomNodes();
        }
        
        private void RegisterCustomNodes()
        {
            if (!GetCustomNodes(out var customNodes))
            {
                return;
            }
            
            foreach (var node in customNodes)
            {
                AddCustomType(node.Name, node.InheritedNode, GD.Load<Script>(node.ScriptLocation), node.Icon);
                GD.Print($"Added new custom node: {node.Name}");
            }
        }
        
        private void UnregisterCustomNodes()
        {
            if (!GetCustomNodes(out var customNodes))
            {
                return;
            }
            
            foreach (var node in customNodes)
            {
                RemoveCustomType(node.Name);
                GD.Print($"Removed custom node: {node.Name}");
            }
        }

        /// <summary>
        /// Called when the refresh button is pressed.
        /// </summary>
        protected void OnRefreshPressed()
        {
            RefreshCustomNodes();
        }

        #region Godot Events
        /// <summary>
        /// Called when this plugin enters the main tree (on tool load).
        /// </summary>
        public override void _EnterTree()
        {
            if (!Engine.EditorHint)
            {
                return;
            }
            
            // Initialize a new refresh button and slot it in the toolbar container.
            _registerNodesButton = new Button();
            _registerNodesButton.Text = RegisterNodesText;

            AddControlToContainer(CustomControlContainer.Toolbar, _registerNodesButton);
            _registerNodesButton.Icon = _registerNodesButton.GetIcon("Reload", "EditorIcons");
            _registerNodesButton.Connect("pressed", this, nameof(OnRefreshPressed));

            RefreshCustomNodes();
        }

        /// <summary>
        /// Called when this plugin exits the main tree (on tool unload).
        /// </summary>
        public override void _ExitTree()
        {
            if (!Engine.EditorHint)
            {
                return;
            }
            
            UnregisterCustomNodes();
            RemoveControlFromContainer(CustomControlContainer.Toolbar, _registerNodesButton);
            _registerNodesButton.QueueFree();
        }
        #endregion
    }
}
#endif