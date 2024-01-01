using System.Collections.Generic;
using Godot;
using Moonvalk.Resources;

namespace Moonvalk.Addons
{
    [Tool, RegisteredType(nameof(CustomNodesContainer))]
    public class CustomNodesContainer : Resource
    {
        [Export] public List<CustomNodesList> Lists { get; private set; }
    }
}