using UnityEngine;

namespace FishermansSail
{
    // Visual running line: the existing sheet winches continue to control the
    // clew. The upper corner moves freely beneath the aft masthead pulley.
    internal sealed class FishermanSupportLine : MonoBehaviour
    {
        public LineRenderer[] UpperSheets;
        public RopeEffect[] NativeSheets;
        private readonly Vector3[] upperPoints = new Vector3[33];

        internal static FishermanSupportLine Create(
            Transform parent,
            RopeEffect left,
            RopeEffect right
        )
        {
            var root = new GameObject("Fisherman upper sheets");
            root.transform.SetParent(parent, false);
            var route = root.AddComponent<FishermanSupportLine>();
            route.NativeSheets = new[] { left, right };
            route.UpperSheets = new[]
            {
                CreateRenderer(root.transform, "Port upper sheet", left, route.upperPoints.Length),
                CreateRenderer(
                    root.transform,
                    "Starboard upper sheet",
                    right,
                    route.upperPoints.Length
                ),
            };
            return route;
        }

        private static LineRenderer CreateRenderer(
            Transform parent,
            string name,
            RopeEffect source,
            int count
        )
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rope = root.AddComponent<LineRenderer>();
            var original = source.GetComponent<LineRenderer>();
            rope.sharedMaterials = original.sharedMaterials;
            rope.startColor = original.startColor;
            rope.endColor = original.endColor;
            rope.startWidth = source.ropeWidth;
            rope.endWidth = source.ropeWidth;
            rope.textureMode = LineTextureMode.Tile;
            rope.useWorldSpace = true;
            rope.numCornerVertices = 2;
            rope.numCapVertices = 2;
            rope.shadowCastingMode = original.shadowCastingMode;
            rope.receiveShadows = original.receiveShadows;
            rope.positionCount = count;
            rope.enabled = false;
            return rope;
        }

        internal void Draw(Vector3 head, Vector3 aftGuide)
        {
            for (int side = 0; side < UpperSheets.Length; side++)
            {
                var source = NativeSheets[side];
                var rope = UpperSheets[side];
                if (!source || !source.gameObject.activeInHierarchy)
                {
                    rope.enabled = false;
                    continue;
                }
                float slack = FlyingSailGeometry.SheetSlack(
                    source.currentRopeLength,
                    source.totalRopeLength
                );
                for (int i = 0; i < upperPoints.Length; i++)
                    upperPoints[i] = FlyingSailGeometry.UpperSheetPoint(
                        head,
                        aftGuide,
                        source.transform.position,
                        slack,
                        (float)i / (upperPoints.Length - 1)
                    );
                rope.SetPositions(upperPoints);
                rope.enabled = true;
            }
        }

        internal void Hide()
        {
            foreach (var sheet in UpperSheets)
                sheet.enabled = false;
        }
    }
}
