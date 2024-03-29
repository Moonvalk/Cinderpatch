// #define __DEBUG

using System;
using System.Threading.Tasks;
using Fractural.Tasks;
using Godot;
using Moonvalk.Accessory;
using Moonvalk.Utilities;
using Thread = System.Threading.Thread;

namespace Moonvalk.Resources
{
    /// <summary>
    /// Helper for loading resources. This manages polling for status as Godot loads new items and
    /// provides a callback for utilizing resources when they are complete.
    /// </summary>
    public static class MoonResourceLoader
    {
        /// <summary>
        /// The default poll rate in seconds. This is how often finalization checks will occur when resources are
        /// actively being loaded.
        /// </summary>
        private const float DefaultPollRate = 0.05f;

        /// <summary>
        /// Attempts to load a resource at the provided file path. Once loading is completed the provided callback will be
        /// invoked with the new resource as a parameter.
        /// </summary>
        /// <typeparam name="ResourceType">The type of resource to be loaded.</typeparam>
        /// <param name="path_">The path within the file system where this resource is located.</param>
        /// <param name="onLoad_">A callback to be executed once a successful load is complete.</param>
        /// <param name="pollRate_">The rate at which in seconds polling will be done.</param>
        /// <param name="initialPollDelay_">An initial polling delay in seconds.</param>
        public static void Load<ResourceType>(
            string path_,
            Action<ResourceType> onLoad_ = null,
            float? pollRate_ = null,
            float? initialPollDelay_ = null
        ) where ResourceType : Resource
        {
            void LoadResource()
            {
                var loader = ResourceLoader.LoadInteractive(path_);
#if (__DEBUG)
					GD.Print("Started loading resource at path: " + path_);
#endif
                PollLoader(loader, onLoad_, pollRate_ ?? DefaultPollRate, initialPollDelay_ ?? 0f);
            }

            if (DeviceHelpers.IsDeviceHtml5())
            {
                LoadResource();
                return;
            }

            var thread = new Thread(LoadResource);
            thread.Start();
        }

        /// <summary>
        /// Loads a resource at the specified path asynchronously if available.
        /// </summary>
        /// <param name="path_">The path within the file system where this resource is located.</param>
        /// <param name="onLoad_">A callback to be executed once a successful load is complete.</param>
        /// <typeparam name="ResourceType">The type of resource to be loaded.</typeparam>
        /// <returns>Returns the resource when loaded successfully.</returns>
        public static async GDTask<ResourceType> LoadAsync<ResourceType>(string path_, Action<ResourceType> onLoad_ = null)
            where ResourceType : Resource
        {
            var resource = await GDTask.RunOnThreadPool(() => ResourceLoader.Load<ResourceType>(path_));
            onLoad_?.Invoke(resource);
            return resource;
        }
        
        /// <summary>
        /// Called to poll an active resource loader after a duration of time has passed. This will recursively
        /// call itself until a successful load is complete.
        /// </summary>
        /// <typeparam name="ResourceType">The type of resource to be loaded.</typeparam>
        /// <param name="loader_">The loader being used to pull new resources.</param>
        /// <param name="onLoad_">A callback to be executed once a successful load is complete.</param>
        /// <param name="pollRate_">The adjusted rate at which in seconds polling will be done following an initial delay.</param>
        /// <param name="pollDelay_">A delay in seconds before the next poll will occur.</param>
        private static void PollLoader<ResourceType>(
            ResourceInteractiveLoader loader_,
            Action<ResourceType> onLoad_,
            float pollRate_,
            float pollDelay_
        ) where ResourceType : Resource
        {
            MoonTimer.Wait(pollDelay_, () =>
            {
#if (__DEBUG)
					GD.Print("Polling load for status...");
#endif
                var error = loader_.Poll();
                if (error == Error.FileEof)
                {
                    var resource = (ResourceType)loader_.GetResource();
                    loader_.Dispose();
#if (__DEBUG)
						GD.Print("Resource loaded: " + resource);
#endif
                    onLoad_?.Invoke(resource);
                }
                else if (error == Error.Ok)
                {
                    PollLoader(loader_, onLoad_, pollRate_, pollRate_);
                }
                else
                {
                    GD.Print("Resource load failure: " + error);
                }
            });
        }
    }
}