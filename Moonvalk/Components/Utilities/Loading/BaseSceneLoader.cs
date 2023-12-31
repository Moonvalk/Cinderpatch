using System;
using Godot;
using Moonvalk.Nodes;

namespace Moonvalk.Components
{
    /// <summary>
    /// </summary>
    /// <typeparam name="DataType">The type of data used to load.</typeparam>
    /// <typeparam name="SceneType">The type of scene element packed scenes should be casted to.</typeparam>
    public abstract class BaseSceneLoader<DataType, SceneType> : Node where SceneType : Node
    {
        #region Godot Events
        /// <summary>
        /// Called when this object is first initialized.
        /// </summary>
        public override void _Ready()
        {
            if (ShowDefaultOnLoad)
            {
                Show(DefaultSceneIndex);
            }

            TransitionAnimator = this.GetComponent<BaseSceneTransitionAnimator>();
            LoadAnimator = this.GetComponent<BaseSceneLoadAnimator>();
        }
        #endregion

        #region Data Fields
        /// <summary>
        /// All scenes available to this object to swap between.
        /// </summary>
        [Export] public DataType Scenes { get; protected set; }

        /// <summary>
        /// Flag that determines if the default scene should be shown on load.
        /// </summary>
        [Export] public bool ShowDefaultOnLoad { get; protected set; } = true;

        /// <summary>
        /// The default scene index to be displayed.
        /// </summary>
        [Export] public int DefaultSceneIndex { get; protected set; }

        /// <summary>
        /// Stores reference to a transition animator if applicable.
        /// </summary>
        public BaseSceneTransitionAnimator TransitionAnimator { get; protected set; }

        /// <summary>
        /// Stores reference to a load animator if applicable.
        /// </summary>
        public BaseSceneLoadAnimator LoadAnimator { get; protected set; }

        /// <summary>
        /// Stores the current scene.
        /// </summary>
        public SceneType CurrentScene { get; protected set; }

        /// <summary>
        /// Stores the current scene index being loaded.
        /// </summary>
        private int _loadedIndex = -1;

        /// <summary>
        /// Event emitted when this element hides itself.
        /// </summary>
        [Signal]
        public delegate void OnHide();

        /// <summary>
        /// Event emitted when this element displays a new scene.
        /// </summary>
        [Signal]
        public delegate void OnDisplay();
        #endregion

        #region Public Methods
        /// <summary>
        /// Hides the current scene with no callback.
        /// </summary>
        public virtual void Hide()
        {
            Hide(null);
        }

        /// <summary>
        /// Called when this object hides its current scene.
        /// </summary>
        /// <param name="onComplete_">An action to be called when the hide animation is complete.</param>
        public virtual void Hide(Action onComplete_)
        {
            HideCurrentScene();
            onComplete_?.Invoke();
        }

        /// <summary>
        /// Called to show the scene at the specified index.
        /// </summary>
        /// <param name="sceneIndex_">The scene index to load.</param>
        /// <returns>Returns true if the scene request is unique, false if the request was already made.</returns>
        public bool Show(int sceneIndex_ = -1)
        {
            var index = sceneIndex_ == -1 ? DefaultSceneIndex : sceneIndex_;
            if (_loadedIndex == index)
            {
                return false;
            }

            _loadedIndex = index;
            DisplayScene(index);
            return true;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Internal execution to remove the current scene and inform subscribers.
        /// </summary>
        protected void HideCurrentScene()
        {
            this.RemoveChildren(CurrentScene);
            EmitSignal(nameof(OnHide));
        }

        /// <summary>
        /// Called to display the scene at the specified index.
        /// </summary>
        /// <param name="sceneIndex_">The scene index to swap to.</param>
        protected virtual void DisplayScene(int sceneIndex_)
        {
            SwapToScene(sceneIndex_);
        }

        /// <summary>
        /// Internal execution to remove the current scene and swap to a new one. When finished the display
        /// event will be invoked to inform subscribers.
        /// </summary>
        /// <param name="sceneIndex_">The scene index to swap to.</param>
        protected abstract void SwapToScene(int sceneIndex_);
        #endregion
    }
}