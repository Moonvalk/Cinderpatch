using Godot;
using Moonvalk.Resources;

namespace Moonvalk.Addons
{
    [Tool]
    public class SceneLoader : Node
    {
        public override async void _Ready()
        {
            await MoonResourceLoader.LoadAsync<Resource>("");
        }
    }
}