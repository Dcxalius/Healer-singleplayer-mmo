namespace Project_1.System.Models
{
    internal readonly struct TextureCoordinate2D
    {
        public static TextureCoordinate2D Zero => new TextureCoordinate2D(0f, 0f);

        public float U => u;
        readonly float u;

        public float V => v;
        readonly float v;

        public TextureCoordinate2D(float aU, float aV)
        {
            u = aU;
            v = aV;
        }
    }
}
