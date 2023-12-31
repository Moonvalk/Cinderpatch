using Godot;

namespace Moonvalk.Animation
{
    /// <summary>
    /// Handler for animating textures on a mesh instance.
    /// </summary>
    public class TextureAnimator : BaseTextureAnimator<TextureAnimation, Texture>
    {
        #region Private Methods
        /// <summary>
        /// Adjusts the texture on the stored mesh instance.
        /// </summary>
        protected override void AdjustTexture()
        {
            var material = _mesh.GetActiveMaterial(0) as SpatialMaterial;
            if (material == null)
            {
                return;
            }
            
            material.AlbedoTexture = _currentAnimation.Frames[_currentFrame];
        }
        #endregion
    }
}