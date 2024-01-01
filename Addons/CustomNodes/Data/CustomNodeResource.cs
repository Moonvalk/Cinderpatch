using Godot;
using Moonvalk.Resources;

namespace Moonvalk.Addons
{
    [Tool, RegisteredType(nameof(CustomNodeResource))]
    public class CustomNodeResource : Resource
    {
        [Export] public string Name { get; private set; }
        [Export] public string InheritedNode { get; private set; }
        [Export] public string ScriptLocation { get; private set; }
        [Export] public Texture Icon { get; private set; }
    }
}
