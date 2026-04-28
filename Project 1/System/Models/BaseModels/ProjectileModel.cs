namespace Project_1.System.Models.BaseModels
{
    internal class ProjectileModel : BaseModel
    {
        public ProjectileModel(float aFrameHeight, float aPictureLength, string aPresentationModelName = null, Model3D aFrameModel = null, Model3D aGameplayModel = null, Model2D aPresentationModel = null)
            : base(
                aFrameHeight,
                aPictureLength,
                aFrameModel,
                aGameplayModel,
                aPresentationModel ?? ModelFactory.GetModel2D(aPresentationModelName))
        {
        }

        //TODO: Calculate the max height of the projectile to create the frame
        //TODO: Glass/Transparent material for the 3d texture
        //TODO: Slide the Model to it's target and then destroy it
        //TODO: Effects on hit (particles, sound, etc.)
        //TODO: Different projectile types (arrows, fireballs, etc.) with different base shapes and effects
    }
}
