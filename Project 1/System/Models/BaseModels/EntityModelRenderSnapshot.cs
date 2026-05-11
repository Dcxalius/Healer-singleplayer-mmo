using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;

namespace Project_1.System.Models.BaseModels
{
    internal readonly struct EntityModelRenderSnapshot : IRenderSnapshot
    {
        readonly int renderId;
        readonly WorldSpace3D worldPosition;
        readonly string definitionKey;
        readonly UnitModel.Type shellType;
        readonly int presentationFrameSizePixels;
        readonly GfxType presentationTextureType;
        readonly string presentationTextureName;
        readonly float baseFacingYawRadians;
        readonly float frameFacingYawRadians;
        readonly float presentationFacingYawRadians;
        readonly bool frameFacesCameraInPreview;
        readonly bool presentationFacesCameraInPreview;
        readonly int presentationDirectionIndex;

        public EntityModelRenderSnapshot(
            int aRenderId,
            WorldSpace3D aWorldPosition,
            string aDefinitionKey,
            UnitModel.Type aShellType,
            int aPresentationFrameSizePixels,
            GfxType aPresentationTextureType,
            string aPresentationTextureName,
            float aBaseFacingYawRadians,
            float aFrameFacingYawRadians,
            float aPresentationFacingYawRadians,
            bool aFrameFacesCameraInPreview,
            bool aPresentationFacesCameraInPreview,
            int aPresentationDirectionIndex)
        {
            renderId = aRenderId;
            worldPosition = aWorldPosition;
            definitionKey = aDefinitionKey;
            shellType = aShellType;
            presentationFrameSizePixels = aPresentationFrameSizePixels;
            presentationTextureType = aPresentationTextureType;
            presentationTextureName = aPresentationTextureName;
            baseFacingYawRadians = aBaseFacingYawRadians;
            frameFacingYawRadians = aFrameFacingYawRadians;
            presentationFacingYawRadians = aPresentationFacingYawRadians;
            frameFacesCameraInPreview = aFrameFacesCameraInPreview;
            presentationFacesCameraInPreview = aPresentationFacesCameraInPreview;
            presentationDirectionIndex = aPresentationDirectionIndex;
        }

        public int RenderId => renderId;
        public WorldSpace3D WorldPosition => worldPosition;
        public string DefinitionKey => definitionKey;
        public UnitModel.Type ShellType => shellType;
        public int PresentationFrameSizePixels => presentationFrameSizePixels;
        public GfxType PresentationTextureType => presentationTextureType;
        public string PresentationTextureName => presentationTextureName;
        public bool HasPresentationTexture => !string.IsNullOrWhiteSpace(presentationTextureName);
        public float BaseFacingYawRadians => baseFacingYawRadians;
        public float FrameFacingYawRadians => frameFacingYawRadians;
        public float PresentationFacingYawRadians => presentationFacingYawRadians;
        public bool FrameFacesCameraInPreview => frameFacesCameraInPreview;
        public bool PresentationFacesCameraInPreview => presentationFacesCameraInPreview;
        public int PresentationDirectionIndex => presentationDirectionIndex;
    }
}
