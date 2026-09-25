namespace MoreSailwindSails.Sails.FishermansStaysail.MkB
{
    internal sealed class FishermansStaysailMkBShape : FishermansStaysailShape
    {
        internal override string ObjectPrefix => "FishermansStaysailMkB";
        internal override float FixedUpperHeadAngle =>
            FishermansStaysailMkBGeometry.FixedUpperHeadAngle;

        internal override FishermansStaysailMeshData Create(float width, float headSlope) =>
            FishermansStaysailMkBGeometry.Create(width, headSlope);
    }
}
