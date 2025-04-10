using UnityEngine;

namespace PathCreation.Examples
{
    public class RoadMeshCreator : PathSceneTool
    {
        [Header("Road settings")]
        public float roadWidth = .4f;
        [Range(0, .5f)]
        public float thickness = .15f;
        public bool flattenSurface;
        public bool addEndCaps = true;

        [Header("Material settings")]
        public Material roadMaterial;
        public Material undersideMaterial;
        public Material endCapMaterial;
        public float textureTiling = 1;

        [SerializeField, HideInInspector]
        GameObject meshHolder;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;
        Mesh mesh;

        protected override void PathUpdated()
        {
            if (pathCreator != null)
            {
                AssignMeshComponents();
                AssignMaterials();
                CreateRoadMesh();
            }
        }

        public void ForcePathUpdate()
        {
            PathUpdated();
        }

        void CreateRoadMesh()
        {
            // Calculate vertex count
            int baseVertCount = path.NumPoints * 8;
            // Add extra vertices for start and end caps (if not a closed loop)
            int capVertCount = (!path.isClosedLoop && addEndCaps) ? 8 : 0;

            Vector3[] verts = new Vector3[baseVertCount + capVertCount];
            Vector2[] uvs = new Vector2[verts.Length];
            Vector3[] normals = new Vector3[verts.Length];

            int numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
            int[] roadTriangles = new int[numTris * 3];
            int[] underRoadTriangles = new int[numTris * 3];
            int[] sideOfRoadTriangles = new int[numTris * 2 * 3];

            // Add triangles for caps (if not a closed loop)
            int[] startCapTriangles = (!path.isClosedLoop && addEndCaps) ? new int[6] : new int[0];
            int[] endCapTriangles = (!path.isClosedLoop && addEndCaps) ? new int[6] : new int[0];

            int vertIndex = 0;
            int triIndex = 0;

            // Vertices for the top of the road are layed out:
            // 0  1
            // 8  9
            // and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right.
            int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
            int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };

            bool usePathNormals = !(path.space == PathSpace.xyz && flattenSurface);

            for (int i = 0; i < path.NumPoints; i++)
            {
                Vector3 localUp = (usePathNormals) ? Vector3.Cross(path.GetTangent(i), path.GetNormal(i)) : path.up;
                Vector3 localRight = (usePathNormals) ? path.GetNormal(i) : Vector3.Cross(localUp, path.GetTangent(i));

                // Find position to left and right of current path vertex
                Vector3 vertSideA = path.GetPoint(i) - localRight * Mathf.Abs(roadWidth);
                Vector3 vertSideB = path.GetPoint(i) + localRight * Mathf.Abs(roadWidth);

                // Add top of road vertices
                verts[vertIndex + 0] = vertSideA;
                verts[vertIndex + 1] = vertSideB;
                // Add bottom of road vertices
                verts[vertIndex + 2] = vertSideA - localUp * thickness;
                verts[vertIndex + 3] = vertSideB - localUp * thickness;

                // Duplicate vertices to get flat shading for sides of road
                verts[vertIndex + 4] = verts[vertIndex + 0];
                verts[vertIndex + 5] = verts[vertIndex + 1];
                verts[vertIndex + 6] = verts[vertIndex + 2];
                verts[vertIndex + 7] = verts[vertIndex + 3];

                // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
                uvs[vertIndex + 0] = new Vector2(0, path.times[i]);
                uvs[vertIndex + 1] = new Vector2(1, path.times[i]);

                // Top of road normals
                normals[vertIndex + 0] = localUp;
                normals[vertIndex + 1] = localUp;
                // Bottom of road normals
                normals[vertIndex + 2] = -localUp;
                normals[vertIndex + 3] = -localUp;
                // Sides of road normals
                normals[vertIndex + 4] = -localRight;
                normals[vertIndex + 5] = localRight;
                normals[vertIndex + 6] = -localRight;
                normals[vertIndex + 7] = localRight;

                // Set triangle indices
                if (i < path.NumPoints - 1 || path.isClosedLoop)
                {
                    for (int j = 0; j < triangleMap.Length; j++)
                    {
                        roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % baseVertCount;
                        // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                        underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % baseVertCount;
                    }
                    for (int j = 0; j < sidesTriangleMap.Length; j++)
                    {
                        sideOfRoadTriangles[triIndex * 2 + j] = (vertIndex + sidesTriangleMap[j]) % baseVertCount;
                    }
                }

                vertIndex += 8;
                triIndex += 6;
            }

            // Add end caps if path is not closed and caps are enabled
            if (!path.isClosedLoop && addEndCaps)
            {
                // Create start cap
                int startIndex = baseVertCount;
                Vector3 startForward = path.GetTangent(0);
                Vector3 startUp = (usePathNormals) ? Vector3.Cross(startForward, path.GetNormal(0)) : path.up;
                Vector3 startRight = (usePathNormals) ? path.GetNormal(0) : Vector3.Cross(startUp, startForward);

                // Get the first four vertices of the road
                Vector3 topLeft = verts[0];
                Vector3 topRight = verts[1];
                Vector3 bottomLeft = verts[2];
                Vector3 bottomRight = verts[3];

                // Copy these vertices for the cap
                verts[startIndex] = topLeft;
                verts[startIndex + 1] = topRight;
                verts[startIndex + 2] = bottomLeft;
                verts[startIndex + 3] = bottomRight;

                // Set start cap normals (facing back)
                for (int i = 0; i < 4; i++)
                {
                    normals[startIndex + i] = -startForward;
                }

                // Set start cap UVs
                uvs[startIndex] = new Vector2(0, 0);
                uvs[startIndex + 1] = new Vector2(1, 0);
                uvs[startIndex + 2] = new Vector2(0, 1);
                uvs[startIndex + 3] = new Vector2(1, 1);

                startCapTriangles[0] = startIndex + 1;
                startCapTriangles[1] = startIndex + 2;
                startCapTriangles[2] = startIndex;
                startCapTriangles[3] = startIndex + 3;
                startCapTriangles[4] = startIndex + 2;
                startCapTriangles[5] = startIndex + 1;

                // Create end cap
                int endIndex = baseVertCount + 4;
                int lastPointIndex = path.NumPoints - 1;
                Vector3 endForward = path.GetTangent(lastPointIndex);
                Vector3 endUp = (usePathNormals) ? Vector3.Cross(endForward, path.GetNormal(lastPointIndex)) : path.up;
                Vector3 endRight = (usePathNormals) ? path.GetNormal(lastPointIndex) : Vector3.Cross(endUp, endForward);

                // Get the last four vertices of the road
                int lastVertIndex = (path.NumPoints - 1) * 8;
                topLeft = verts[lastVertIndex];
                topRight = verts[lastVertIndex + 1];
                bottomLeft = verts[lastVertIndex + 2];
                bottomRight = verts[lastVertIndex + 3];

                // Copy these vertices for the cap
                verts[endIndex] = topLeft;
                verts[endIndex + 1] = topRight;
                verts[endIndex + 2] = bottomLeft;
                verts[endIndex + 3] = bottomRight;

                // Set end cap normals (facing forward)
                for (int i = 0; i < 4; i++)
                {
                    normals[endIndex + i] = endForward;
                }

                // Set end cap UVs
                uvs[endIndex] = new Vector2(0, 0);
                uvs[endIndex + 1] = new Vector2(1, 0);
                uvs[endIndex + 2] = new Vector2(0, 1);
                uvs[endIndex + 3] = new Vector2(1, 1);

                endCapTriangles[0] = endIndex;
                endCapTriangles[1] = endIndex + 2;
                endCapTriangles[2] = endIndex + 1;
                endCapTriangles[3] = endIndex + 1;
                endCapTriangles[4] = endIndex + 2;
                endCapTriangles[5] = endIndex + 3;
            }

            mesh.Clear();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.normals = normals;

            // Set submesh count based on whether we have caps
            mesh.subMeshCount = (!path.isClosedLoop && addEndCaps) ? 5 : 3;

            mesh.SetTriangles(roadTriangles, 0);
            mesh.SetTriangles(underRoadTriangles, 1);
            mesh.SetTriangles(sideOfRoadTriangles, 2);

            // Add cap triangles to separate submeshes
            if (!path.isClosedLoop && addEndCaps)
            {
                mesh.SetTriangles(startCapTriangles, 3);
                mesh.SetTriangles(endCapTriangles, 4);
            }

            mesh.RecalculateBounds();
        }

        // Add MeshRenderer and MeshFilter components to this gameobject if not already attached
        void AssignMeshComponents()
        {
            if (meshHolder == null)
            {
                meshHolder = new GameObject("Road Mesh Holder");
            }

            meshHolder.transform.rotation = Quaternion.identity;
            meshHolder.transform.position = Vector3.zero;
            meshHolder.transform.localScale = Vector3.one;

            // Ensure mesh renderer and filter components are assigned
            if (!meshHolder.gameObject.GetComponent<MeshFilter>())
            {
                meshHolder.gameObject.AddComponent<MeshFilter>();
            }
            if (!meshHolder.GetComponent<MeshRenderer>())
            {
                meshHolder.gameObject.AddComponent<MeshRenderer>();
            }

            meshRenderer = meshHolder.GetComponent<MeshRenderer>();
            meshFilter = meshHolder.GetComponent<MeshFilter>();
            meshHolder.TryGetComponent(out meshCollider);
            if (mesh == null)
            {
                mesh = new Mesh();
            }
            meshFilter.sharedMesh = mesh;
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = mesh;
            }
        }

        void AssignMaterials()
        {
            // Determine number of materials needed based on cap settings
            int materialCount = (!path.isClosedLoop && addEndCaps) ? 5 : 3;
            Material[] materials = new Material[materialCount];

            // Assign materials if they exist
            if (roadMaterial != null && undersideMaterial != null)
            {
                materials[0] = roadMaterial;
                materials[1] = undersideMaterial;
                materials[2] = undersideMaterial;

                if (!path.isClosedLoop && addEndCaps)
                {
                    // Use endCapMaterial if assigned, otherwise use road material
                    Material capMaterial = (endCapMaterial != null) ? endCapMaterial : roadMaterial;
                    materials[3] = capMaterial;
                    materials[4] = capMaterial;
                }

                meshRenderer.sharedMaterials = materials;
                meshRenderer.sharedMaterials[0].mainTextureScale = new Vector3(1, textureTiling);
            }
        }
    }
}