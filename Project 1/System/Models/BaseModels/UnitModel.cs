using System;

namespace Project_1.System.Models.BaseModels
{
    internal class UnitModel : BaseModel
    {
        public enum Type
        {
            Small,
            Medium,
            Large,
            Elite,
            Giant,
            Boss,
            GiantBoss
        }

        public Type UnitType => unitType;
        readonly Type unitType;

        static float GetPictureLength(Type type)
        {
            return type switch
            {
                Type.Small => 0.3f,
                Type.Medium => 0.5f,
                Type.Large => 0.7f,
                Type.Elite => 0.9f,
                Type.Giant => 1.2f,
                Type.Boss => 2f,
                Type.GiantBoss => 4f,
                _ => throw new NotImplementedException()
            };
        }

        static float GetFrameHeight(Type type)
        {
            return GetPictureLength(type);
        }

        public UnitModel(Type aType, string aPresentationModelName, Model3D aFrameModel = null, Model3D aGameplayModel = null, Model2D aPresentationModel = null)
            : base(
                GetFrameHeight(aType),
                GetPictureLength(aType),
                aFrameModel,
                aGameplayModel,
                aPresentationModel ?? ModelFactory.GetModel2D(aPresentationModelName))
        {
            unitType = aType;
        }

        //TODO: Calculate the angles of the pieces for rotation of the 2d model

        //TODO: Calculate the shape of the 3d base, aswell as a scaled frame for the 2d art
        //Multiple shapes should be available, but lets start basic for now

        //TODO: Create art design doc
        //The 2d art has a front, always facing the X, Z direction.
        //The 3d model faces the 2d art along the facing direction.

        //TODO: Animations
        //Simple jumping animation for the 3D model, where the pieces move up and down in a wave pattern, with the head moving the most, and the feet moving the least.
        //Idle animation for the 2D model, where the pieces move up and down in a wave pattern, with the head moving the most, and the feet moving the least.
        //Bobbing Walking animation for the 3D model, where the pieces move in a wave pattern, with the head moving the least, and the feet moving the most.
        //Walking animation for the 2d model
    }
}
