namespace FishermansSail.Sails.FishermansStaysail.MkA
{
    internal sealed class FishermansStaysailMkAShape : FishermansStaysailShape
    {
        internal override string ObjectPrefix => "FishermansStaysailMkA";

        internal override FishermansStaysailMeshData Create(float width, float headSlope) =>
            FishermansStaysailMkAGeometry.Create(width, headSlope);
    }
}
