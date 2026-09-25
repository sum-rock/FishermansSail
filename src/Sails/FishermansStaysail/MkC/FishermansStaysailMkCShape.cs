namespace MoreSailwindSails.Sails.FishermansStaysail.MkC
{
    internal sealed class FishermansStaysailMkCShape : FishermansStaysailShape
    {
        internal override string ObjectPrefix => "FishermansStaysailMkC";
        internal override float FixedUpperHeadAngle =>
            FishermansStaysailMkCGeometry.FixedUpperHeadAngle;

        internal override FishermansStaysailMeshData Create(float width, float headSlope) =>
            FishermansStaysailMkCGeometry.Create(width, headSlope);
    }
}
