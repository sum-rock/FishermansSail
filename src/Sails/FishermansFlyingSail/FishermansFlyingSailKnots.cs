using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MoreSailwindSails.Sails.FishermansFlyingSail
{
    internal sealed class FishermansFlyingSailKnots : MonoBehaviour
    {
        public MeshRenderer[] Renderers;

        // Called only while constructing the inactive template. Instances share
        // this generated mesh; the template asset owner handles its lifetime.
        internal static FishermansFlyingSailKnots TryCreate(
            Transform parent,
            float ropeWidth,
            out Mesh mesh
        )
        {
            mesh = null;
            GameObject donor = null,
                root = null;
            Mesh baked = null;
            try
            {
                if (parent.gameObject.activeInHierarchy)
                    throw new InvalidOperationException(
                        "Knot construction requires an inactive template."
                    );
                var directory = RefsDirectory.instance;
                if (!directory || !directory.clothRopeJibSheetPrefab)
                    throw new InvalidOperationException(
                        "Native jib-sheet rope prefab is unavailable."
                    );
                donor = Object.Instantiate(directory.clothRopeJibSheetPrefab, parent, false);
                var native = donor.GetComponent<ClothRope>();
                var renderer = native ? native.skinned : null;
                if (
                    !renderer
                    || !renderer.sharedMesh
                    || renderer.sharedMesh.subMeshCount != 1
                    || renderer.bones.Length < 2
                    || !renderer.sharedMaterial
                )
                    throw new InvalidOperationException("Native jib-sheet renderer is incomplete.");

                // The shipped mesh is not readable. Bake a private inactive copy;
                // never request vertices from or modify the original shared mesh.
                baked = new Mesh();
                renderer.BakeMesh(baked);
                var vertices = baked.vertices;
                var anchor = renderer.transform.InverseTransformPoint(renderer.bones[0].position);
                var direction = renderer.transform.InverseTransformVector(
                    renderer.bones[1].position - renderer.bones[0].position
                );
                if (direction.sqrMagnitude < 1e-8f)
                    throw new InvalidOperationException(
                        "Native jib-sheet attachment has no direction."
                    );
                var rotation = Quaternion.FromToRotation(direction, Vector3.forward);
                var selected = FishermansFlyingSailKnotGeometry.Select(
                    vertices,
                    baked.triangles,
                    anchor,
                    out var triangles
                );
                var normals = baked.normals;
                var uv = baked.uv;
                if (normals.Length != vertices.Length || uv.Length != vertices.Length)
                    throw new InvalidOperationException("Native knot normals or UVs are missing.");
                var knotVertices = new Vector3[selected.Length];
                var knotNormals = new Vector3[selected.Length];
                var knotUV = new Vector2[selected.Length];
                for (int i = 0; i < selected.Length; i++)
                {
                    int source = selected[i];
                    knotVertices[i] = rotation * (vertices[source] - anchor);
                    knotNormals[i] = rotation * normals[source];
                    knotUV[i] = uv[source];
                }
                mesh = new Mesh { name = "FishermansFlyingSail native jib knot" };
                mesh.vertices = knotVertices;
                mesh.normals = knotNormals;
                mesh.uv = knotUV;
                mesh.triangles = triangles;
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();

                root = new GameObject("FishermansFlyingSail corner knots");
                root.transform.SetParent(parent, false);
                var knots = root.AddComponent<FishermansFlyingSailKnots>();
                knots.Renderers = new MeshRenderer[4];
                for (int i = 0; i < knots.Renderers.Length; i++)
                {
                    var knot = new GameObject("Corner knot " + i);
                    knot.transform.SetParent(root.transform, false);
                    // This parent is outside SE's fabric scaling hierarchy, just
                    // like the world-space ropes: resizing does not enlarge knots.
                    knot.transform.localScale = Vector3.one * (ropeWidth / 0.05f);
                    knot.AddComponent<MeshFilter>().sharedMesh = mesh;
                    var visual = knot.AddComponent<MeshRenderer>();
                    visual.sharedMaterial = renderer.sharedMaterial;
                    visual.shadowCastingMode = renderer.shadowCastingMode;
                    visual.receiveShadows = renderer.receiveShadows;
                    visual.enabled = false;
                    knots.Renderers[i] = visual;
                }
                return knots;
            }
            catch (Exception exception)
            {
                if (root)
                    Object.DestroyImmediate(root);
                if (mesh)
                    Object.DestroyImmediate(mesh);
                mesh = null;
                Plugin.Log.LogWarning($"Flying Sail corner knots unavailable: {exception.Message}");
                return null;
            }
            finally
            {
                if (donor)
                    Object.DestroyImmediate(donor);
                if (baked)
                    Object.DestroyImmediate(baked);
            }
        }

        internal void Draw(Transform[] bones, Vector3 guide, Vector3 controls, Vector3 aftDirection)
        {
            for (int i = 0; i < Renderers.Length; i++)
            {
                var position = bones[i].position;
                var direction =
                    i == 1 ? guide - position
                    : i == 3 ? controls - position
                    : -aftDirection;
                if (direction.sqrMagnitude < 1e-8f)
                    direction = aftDirection;
                var up = bones[0].position - bones[2].position;
                if (Vector3.Cross(direction, up).sqrMagnitude < 1e-8f)
                    up = aftDirection;
                if (Vector3.Cross(direction, up).sqrMagnitude < 1e-8f)
                    up = Vector3.right;
                Renderers[i]
                    .transform.SetPositionAndRotation(
                        position,
                        Quaternion.LookRotation(direction, up)
                    );
                Renderers[i].enabled = true;
            }
        }

        internal void Hide()
        {
            if (Renderers == null)
                return;
            foreach (var renderer in Renderers)
                if (renderer)
                    renderer.enabled = false;
        }

        private void OnDisable() => Hide();
    }
}
