#if (TOOLS)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace Moonvalk.Resources
{
    /// <summary>
    /// This is a plugin for registering custom C# Resources within the Godot Registry.
    /// </summary>
    [Tool]
    public class ResourceRegistryPlugin : EditorPlugin
    {
        /// <summary>
        /// Called to refresh registered classes on user request.
        /// </summary>
        private void RefreshCustomClasses()
        {
            GD.Print("\nRefreshing Registered Resources...");
            UnregisterCustomClasses();
            RegisterCustomClasses();
        }

        /// <summary>
        /// Called to register all custom types.
        /// </summary>
        private void RegisterCustomClasses()
        {
            _customTypes.Clear();

            var file = new File();
            foreach (var type in GetCustomRegisteredTypes())
            {
                if (type.IsSubclassOf(typeof(Resource)))
                {
                    AddRegisteredType(type, nameof(Resource), file);
                }
                else
                {
                    AddRegisteredType(type, nameof(Node), file);
                }
            }
        }

        /// <summary>
        /// Called to add a new registered type to the Godot registry.
        /// </summary>
        /// <param name="type_">The type being added.</param>
        /// <param name="defaultBaseTypeName_">The default inherited type in the case none is provided.</param>
        /// <param name="file_">The file being written.</param>
        private void AddRegisteredType(Type type_, string defaultBaseTypeName_, File file_)
        {
            var attribute = (RegisteredTypeAttribute)Attribute.GetCustomAttribute(type_, typeof(RegisteredTypeAttribute));
            var path = FindClassPath(type_);
            if (path == null || !file_.FileExists(path))
            {
                return;
            }

            var script = GD.Load<Script>(path);
            if (script == null)
            {
                return;
            }

            var baseTypeName = attribute.BaseType == "" ? defaultBaseTypeName_ : attribute.BaseType;
            ImageTexture icon = null;
            var iconPath = attribute.IconPath;
            if (iconPath == "")
            {
                var baseType = type_.BaseType;
                while (baseType != null)
                {
                    var baseTypeAttribute = (RegisteredTypeAttribute)Attribute.GetCustomAttribute(baseType, typeof(RegisteredTypeAttribute));
                    if (baseTypeAttribute != null && baseTypeAttribute.IconPath != "")
                    {
                        iconPath = baseTypeAttribute.IconPath;
                        break;
                    }

                    baseType = baseType.BaseType;
                }
            }

            if (iconPath != "")
            {
                if (file_.FileExists(iconPath))
                {
                    var rawIcon = ResourceLoader.Load<Texture>(iconPath);
                    if (rawIcon != null)
                    {
                        var image = rawIcon.GetData();
                        var length = (int)Mathf.Round(16 * GetEditorInterface()
                            .GetEditorScale());
                        image.Resize(length, length);
                        icon = new ImageTexture();
                        icon.CreateFromImage(image);
                    }
                    else
                    {
                        GD.PushError($"Could not load the icon for the registered type \"{type_.FullName}\" at path \"{path}\".");
                    }
                }
                else
                {
                    GD.PushError($"The icon path of \"{path}\" for the registered type \"{type_.FullName}\" does not exist.");
                }
            }

            AddCustomType($"{ResourceRegistrySettings.ClassPrefix}{type_.Name}", baseTypeName, script, icon);
            _customTypes.Add($"{ResourceRegistrySettings.ClassPrefix}{type_.Name}");
            GD.Print($"Registered custom type: {type_.Name} -> {path}");
        }

        /// <summary>
        /// Finds a matching path for the requested type.
        /// </summary>
        /// <param name="type_">The type to find.</param>
        /// <returns>Returns a string matching the path of the requested type.</returns>
        private static string FindClassPath(Type type_)
        {
            switch (ResourceRegistrySettings.SearchType)
            {
                case ResourceRegistrySettings.ResourceSearchType.Recursive:
                    return FindClassPathRecursive(type_);
                case ResourceRegistrySettings.ResourceSearchType.Namespace:
                    return FindClassPathNamespace(type_);
                default:
                    GD.PushError($"ResourceSearchType {ResourceRegistrySettings.SearchType} not implemented!");
                    return "";
            }
        }

        /// <summary>
        /// Finds the matching class path by searching namespaces.
        /// </summary>
        /// <param name="type_">The type to find a path for.</param>
        /// <returns>Returns the matching path for the type.</returns>
        private static string FindClassPathNamespace(Type type_)
        {
            foreach (var dir in ResourceRegistrySettings.ResourceScriptDirectories)
            {
                var filePath = $"{dir}/{type_.Namespace?.Replace(".", "/") ?? ""}/{type_.Name}.cs";
                var file = new File();
                if (file.FileExists(filePath))
                {
                    return filePath;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the matching class path by searching recursively for type name.
        /// </summary>
        /// <param name="type_">The type to find a path for.</param>
        /// <returns>Returns the matching path for the type.</returns>
        private static string FindClassPathRecursive(Type type_)
        {
            foreach (var directory in ResourceRegistrySettings.ResourceScriptDirectories)
            {
                var fileFound = FindClassPathRecursiveHelper(type_, directory);
                if (fileFound != null)
                {
                    return fileFound;
                }
            }

            return null;
        }

        /// <summary>
        /// Helper method called to recursively search for type paths.
        /// </summary>
        /// <param name="type_">The type to find a path for.</param>
        /// <param name="directory_">The directory to search.</param>
        /// <returns>Returns the matching path, when found.</returns>
        private static string FindClassPathRecursiveHelper(Type type_, string directory_)
        {
            var dir = new Directory();

            if (dir.Open(directory_) != Error.Ok)
            {
                return null;
            }

            dir.ListDirBegin();
            while (true)
            {
                var fileOrDirName = dir.GetNext();

                // Skips hidden files like .
                if (fileOrDirName == "")
                {
                    break;
                }

                if (fileOrDirName.BeginsWith("."))
                {
                    continue;
                }

                if (dir.CurrentIsDir())
                {
                    var foundFilePath = FindClassPathRecursiveHelper(type_, dir.GetCurrentDir() + "/" + fileOrDirName);
                    if (foundFilePath == null)
                    {
                        continue;
                    }

                    dir.ListDirEnd();
                    return foundFilePath;
                }
                
                if (fileOrDirName == $"{type_.Name}.cs")
                {
                    return dir.GetCurrentDir() + "/" + fileOrDirName;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all valid custom registered types.
        /// </summary>
        /// <returns>All valid custom registered types.</returns>
        private static IEnumerable<Type> GetCustomRegisteredTypes()
        {
            var assembly = Assembly.GetAssembly(typeof(ResourceRegistryPlugin));
            return assembly.GetTypes()
                .Where(t => !t.IsAbstract
                            && Attribute.IsDefined(t, typeof(RegisteredTypeAttribute))
                            && (t.IsSubclassOf(typeof(Node)) || t.IsSubclassOf(typeof(Resource)))
                );
        }

        /// <summary>
        /// Called to unregister all custom Resource types from the Godot registry.
        /// </summary>
        private void UnregisterCustomClasses()
        {
            foreach (var script in _customTypes)
            {
                RemoveCustomType(script);
                GD.Print($"Unregister custom resource: {script}");
            }

            _customTypes.Clear();
        }

        /// <summary>
        /// Called when the refresh button is pressed.
        /// </summary>
        protected void OnRefreshPressed()
        {
            RefreshCustomClasses();
        }

        #region Data Fields
        /// <summary>
        /// A list of all custom types.
        /// </summary>
        private readonly List<string> _customTypes = new List<string>();

        /// <summary>
        /// Stores reference to the refresh button within the Godot editor to load registered types.
        /// </summary>
        private Button _refreshButton;
        
        /// <summary>
        /// Text displayed on the refresh resources button.
        /// </summary>
        private const string RefreshButtonText = "Build Resources";
        #endregion

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
            _refreshButton = new Button();
            _refreshButton.Text = RefreshButtonText;

            AddControlToContainer(CustomControlContainer.Toolbar, _refreshButton);
            _refreshButton.Icon = _refreshButton.GetIcon("Reload", "EditorIcons");
            _refreshButton.Connect("pressed", this, nameof(OnRefreshPressed));

            ResourceRegistrySettings.Init();
            RefreshCustomClasses();
            GD.PushWarning(
                "You may change any setting for the C# Registry Plugin in Project -> ProjectSettings -> General -> ResourceRegistryPlugin");
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
            
            UnregisterCustomClasses();
            RemoveControlFromContainer(CustomControlContainer.Toolbar, _refreshButton);
            _refreshButton.QueueFree();
        }
        #endregion
    }
}
#endif