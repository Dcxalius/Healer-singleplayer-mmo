namespace Project_1.System.Models.BaseModels
{
    internal class BaseModel
    {
        /// <summary>
        /// A model shell that combines a gameplay-facing 3D body with an optional 2D presentation model.
        /// </summary>

        //public Circle Hitbox => hitbox;
        //Circle hitbox;
        //public Shape ClickShape => CalculateClickShape(GameplayModel, FacingYawRadians);

        public float FrameHeight => frameHeight;
        readonly float frameHeight;

        public float PictureLength => pictureLength;
        readonly float pictureLength;

        public Model3D FrameModel => frameModel;
        readonly Model3D frameModel;

        public Model3D GameplayModel => gameplayModel;
        readonly Model3D gameplayModel;

        public Model2D PresentationModel => presentationModel;
        readonly Model2D presentationModel;

        public float FacingYawRadians => facingYawRadians;
        float facingYawRadians;

        protected BaseModel(float aFrameHeight, float aPictureLength, Model3D aFrameModel, Model3D aGameplayModel, Model2D aPresentationModel = null)
        {
            frameHeight = aFrameHeight;
            pictureLength = aPictureLength;
            presentationModel = aPresentationModel;
            float aspectRatio = presentationModel?.AspectRatio ?? 1f;
            frameModel = aFrameModel ?? BaseModelGeometryFactory.CreateFrame(aFrameHeight, aPictureLength, aspectRatio);
            gameplayModel = aGameplayModel ?? BaseModelGeometryFactory.CreateGameplayBase(aFrameHeight, aPictureLength);
        }

        protected BaseModel(float aFrameHeight, float aPictureLength, string aPresentationModelName, Model3D aFrameModel = null, Model3D aGameplayModel = null)
            : this(
                aFrameHeight,
                aPictureLength,
                aFrameModel,
                aGameplayModel,
                ModelFactory.GetModel2D(aPresentationModelName))
        {
        }

        public void SetFacingYaw(float aFacingYawRadians)
        {
            facingYawRadians = aFacingYawRadians;
        }
    }
}
