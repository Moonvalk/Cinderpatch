using Godot;

namespace Moonvalk.Animation
{
    /// <summary>
    /// Handler for animating texture sheets on a mesh instance.
    /// </summary>
    public class TextureSheetAnimator : BaseTextureAnimator<TextureSheetAnimation, int>
    {
        #region Private Methods
        /// <summary>
        /// Adjusts the texture on the stored mesh instance.
        /// </summary>
        protected override void AdjustTexture()
        {
            var material = _mesh.GetActiveMaterial(0) as SpatialMaterial;
            if (material != null)
            {
                material.AlbedoTexture = _currentAnimation.Sheet.Texture;
            }
            
            var rect = _currentAnimation.GetFrameData(_currentAnimation.Frames[_currentFrame]);
            material.Uv1Offset = new Vector3(rect.Position.x, rect.Position.y, 0f);
            material.Uv1Scale = new Vector3(rect.Size.x, rect.Size.y, 0f);
        }
        #endregion
    }
}