using System.Collections.Generic;
using Godot;
using Moonvalk.Resources;

namespace Moonvalk.Addons
{
    [Tool, RegisteredType(nameof(CustomNodesList))]
    public class CustomNodesList : Resource
    {
        [Export] public List<CustomNodeResource> Nodes { get; private set; }
    }
}