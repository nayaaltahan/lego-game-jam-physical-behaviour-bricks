// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LEGOModelImporter
{

    public class MultipleMeshTool
    {
        private Vector3[] vertices;
        private Vector3[] normals;
        private Vector4[] tangents;
        private Vector2[] uvs;
        private int vertexCount;
        private int[] tris;
        private int triangleCount;

        private int[] vertexMap;
        private int[] vertexCollapse;

        private List<Edge> edges;

        public MultipleMeshTool(Mesh source, Transform sourceTransform, List<Mesh> connectedSources, List<Transform> connectedSourceTransforms)
        {
            var vertexList = new List<Vector3>();
            var normalList = new List<Vector3>();
            var tangentList = new List<Vector4>();
            var uvList = new List<Vector2>();
            var triList = new List<int>();
            int triOffset = source.vertices.Length;

            foreach(var vertex in source.vertices)
            {
                vertexList.Add(sourceTransform.TransformPoint(vertex));
            }
            normalList.AddRange(source.normals);
            if (source.tangents != null)
            {
                tangentList.AddRange(source.tangents);
            }
            if (source.uv != null)
            {
                uvList.AddRange(source.uv);
            }
            triList.AddRange(source.triangles);

            vertexCount = source.vertices.Length;
            triangleCount = source.triangles.Length;

            for (var i = 0; i < connectedSources.Count; ++i)
            {
                foreach (var vertex in connectedSources[i].vertices)
                {
                    vertexList.Add(connectedSourceTransforms[i].TransformPoint(vertex));
                }
                normalList.AddRange(connectedSources[i].normals);
                if (tangentList != null)
                {
                    tangentList.AddRange(connectedSources[i].tangents);
                }
                if (uvList != null)
                {
                    uvList.AddRange(connectedSources[i].uv);
                }

                for (var j = 0; j < connectedSources[i].triangles.Length; ++j)
                {
                    triList.Add(connectedSources[i].triangles[j] + triOffset);
                }
                triOffset += connectedSources[i].vertices.Length;
            }

            vertices = vertexList.ToArray();
            normals = normalList.ToArray();
            if (tangentList.Count > 0)
            {
                tangents = tangentList.ToArray();
            }
            if (uvList.Count > 0)
            {
                uvs = uvList.ToArray();
            }
            tris = triList.ToArray();

            ComputeVertexMap();
        }

        public void ApplyTo(Mesh target, bool recalculateTangents, bool recalculateLightMapUVs)
        {
            target.Clear(false);

            target.SetVertices(vertices, 0, vertexCount);
            target.SetNormals(normals, 0, vertexCount);
            if (tangents != null)
                target.SetTangents(tangents, 0, vertexCount);
            if (uvs != null)
                target.SetUVs(0, uvs, 0, vertexCount);

            target.SetTriangles(tris, 0, triangleCount, 0);

            if (recalculateTangents)
                target.RecalculateTangents();

            UnwrapParam param;
            UnwrapParam.SetDefaults(out param);
            param.packMargin = 0.02f;

            if (recalculateLightMapUVs)
                Unwrapping.GenerateSecondaryUVSet(target, param);
        }


        public class Edge
        {
            // These are all indices into buffers.
            public int v0, v1; // The vertices of the edge.
            public int mapV0, mapV1; // Other vertices that have the same position as v0 and v1
            public int neighbour; // The coinciding edge of the triangle on the "other side" of this edge.
            public int mapNeighbour; // The coinciding edge of the triangle on the "other side" of this edge belonging to mapV0 and mapV1.
            public int tri; // The triangle this edge belongs to. 
        }

        static void AddEdge(ref List<Edge> edges, ref List<int>[] openEdges, ref List<int>[] openMapEdges, int[] vertexMap, int v0, int v1, int tri)
        {
            int minV = Mathf.Min(v0, v1);

            int neighbour = -1;
            for (int e = 0; e < openEdges[minV].Count; ++e)
            {
                int i = openEdges[minV][e];
                Edge ee = edges[i];

                if ((ee.v0 == v1 && ee.v1 == v0) || (ee.v1 == v0 && ee.v0 == v1))
                {
                    neighbour = i;
                    break;
                }
            }

            int mapNeighbour = -1;
            int mapV0 = vertexMap[v0];
            int mapV1 = vertexMap[v1];

            int minMapV = Mathf.Min(mapV0, mapV1);
            for (int e = 0; e < openMapEdges[minMapV].Count; ++e)
            {
                int i = openMapEdges[minMapV][e];
                Edge ee = edges[i];

                if ((ee.mapV0 == mapV1 && ee.mapV1 == mapV0) || (ee.mapV1 == mapV0 && ee.mapV0 == mapV1))
                {
                    mapNeighbour = i;
                    break;
                }
            }

            Edge edge = new Edge();
            edge.v0 = v0;
            edge.v1 = v1;
            edge.mapV0 = mapV0;
            edge.mapV1 = mapV1;
            edge.neighbour = neighbour;
            edge.mapNeighbour = mapNeighbour;
            edge.tri = tri;

            int edgeIndex = edges.Count;

            if (neighbour >= 0)
            {
                edges[neighbour].neighbour = edgeIndex;
                openEdges[minV].Remove(neighbour);
            }
            else
            {
                openEdges[minV].Add(edgeIndex);
            }

            if (mapNeighbour >= 0)
            {
                edges[mapNeighbour].mapNeighbour = edgeIndex;
                openMapEdges[minMapV].Remove(mapNeighbour);
            }
            else
            {
                openMapEdges[minMapV].Add(edgeIndex);
            }
            edges.Add(edge);
        }

        static public void GenerateEdgeList(int[] vertexMap, int[] tris, out List<Edge> edges)
        {
            List<int>[] openEdges = new List<int>[vertexMap.Length];
            List<int>[] openMapEdges = new List<int>[vertexMap.Length];
            for (int i = 0; i < openEdges.Length; ++i)
            {
                openEdges[i] = new List<int>();
                openMapEdges[i] = new List<int>();
            }

            edges = new List<Edge>();
            for (int i = 0; i < tris.Length; i += 3)
            {
                AddEdge(ref edges, ref openEdges, ref openMapEdges, vertexMap, tris[i], tris[i + 1], i);
                AddEdge(ref edges, ref openEdges, ref openMapEdges, vertexMap, tris[i + 1], tris[i + 2], i);
                AddEdge(ref edges, ref openEdges, ref openMapEdges, vertexMap, tris[i + 2], tris[i], i);
            }
        }

        public void GenerateChamfer(float scale, float chamferSize = 0.02f, bool addChamferGeometry = false, bool lockEdges = false)
        {
            Vector3[] push = new Vector3[vertices.Length];
            float[] pushScale = new float[vertices.Length];

            if (edges == null)
            {
                GenerateEdgeList(vertexMap, tris, out edges);
            }

            // Classify edges!
            for (int i = 0; i < edges.Count; ++i)
            {
                Edge e0 = edges[i];
                if (e0.neighbour < 0 && e0.mapNeighbour < 0 && lockEdges)
                {
                    // This edge is non-manifold and does not have a neighbour edge, so it must be on the edge of the mesh.
                    // Lock down the verties by setting pushScale to a large negative value.
                    pushScale[e0.v0] = -1000;
                    pushScale[e0.v1] = -1000;
                }
                else if (e0.neighbour < 0 && e0.mapNeighbour >= 0)
                {
                    Edge e1 = edges[e0.mapNeighbour];

                    int t0 = e0.tri;
                    int t1 = e1.tri;

                    // Classify edge
                    // If center of t1 is in front of plane from t0, then it's a crease and we need to push in the opposite direction
                    Vector3 center0 = (vertices[tris[t0]] + vertices[tris[t0 + 1]] + vertices[tris[t0 + 2]]) / 3.0f;
                    Vector3 normal0 = MathUtils.TriangleNormal(vertices[tris[t0]], vertices[tris[t0 + 1]], vertices[tris[t0 + 2]]);

                    Vector3 center1 = (vertices[tris[t1]] + vertices[tris[t1 + 1]] + vertices[tris[t1 + 2]]) / 3.0f;

                    Plane pl = new Plane(normal0, center0);
                    float dir = pl.GetDistanceToPoint(center1) > 0 ? 1.0f : -1.0f;

                    // Add push to each vertex.
                    push[e0.v0] += Vector3.ProjectOnPlane(normals[e1.v1], normals[e0.v0]) * dir;
                    push[e0.v1] += Vector3.ProjectOnPlane(normals[e1.v0], normals[e0.v1]) * dir;

                    push[e1.v0] += Vector3.ProjectOnPlane(normals[e0.v1], normals[e1.v0]) * dir;
                    push[e1.v1] += Vector3.ProjectOnPlane(normals[e0.v0], normals[e1.v1]) * dir;

                    pushScale[e0.v0]++;
                    pushScale[e0.v1]++;
                    pushScale[e1.v0]++;
                    pushScale[e1.v1]++;
                }
            }

            // Push vertices on plane
            for (int i = 0; i < vertices.Length; ++i)
            {
                if (pushScale[i] > 0 && push[i].magnitude > 0.0001f)
                {
                    push[i].Normalize();
                }
                else
                {
                    push[i] = Vector3.zero;
                }
            }

            // Connect open edges
            List<int> chamferTris = new List<int>();
            for (int i = 0; i < edges.Count; ++i)
            {
                if (edges[i].neighbour < 0 && edges[i].mapNeighbour >= 0)
                {
                    Edge e0 = edges[i];
                    Edge e1 = edges[e0.mapNeighbour];
                    int v0 = e0.v0;
                    int v1 = e0.v1;
                    int v2 = e1.v0;
                    int v3 = e1.v1;

                    if (ContainsTriangle(chamferTris, v1, v0, v2) || ContainsTriangle(chamferTris, v2, v0, v3))
                    {
                        // Don't add triangles more than once.
                        continue;
                    }

                    // Check if any vertices are not part of the original mesh.
                    if (v0 >= vertexCount || v1 >= vertexCount || v2 >= vertexCount || v3 >= vertexCount)
                    {
                        if (!addChamferGeometry)
                        {
                            // Don't add the chamfer geometry on the boundary of the  original mesh.
                            continue;
                        }
                        else if (v0 >= vertexCount && v1 >= vertexCount && v2 >= vertexCount && v3 >= vertexCount)
                        {
                            // Don't add chamfer geometry completely outside the original mesh.
                            continue;
                        }
                    }

                    chamferTris.Add(v1); chamferTris.Add(v0); chamferTris.Add(v2);
                    chamferTris.Add(v2); chamferTris.Add(v0); chamferTris.Add(v3);
                }
            }

            // Connect open vertices
            for (int i = 0; i < vertices.Length; ++i)
            {
                if (vertexCollapse[i] >= 3)
                {
                    List<int> poly = new List<int>();
                    for (int j = i; j < vertices.Length; ++j)
                    {
                        if (vertexMap[j] == i)
                        {
                            poly.Add(j);
                        }
                    }

                    if (poly.Count > 3)
                    {
                        // Order vertices in poly counter-clockwise.
                        Vector3 center = Vector3.zero;
                        Vector3 normal = Vector3.zero;
                        foreach (var v in poly)
                        {
                            center += vertices[v] + push[v];
                            normal += normals[v];
                        }
                        center /= poly.Count;
                        normal.Normalize();

                        Vector3 reference = (vertices[poly[0]] + push[poly[0]]) - center;

                        poly.Sort((a, b) =>
                        {
                            var angleA = Vector3.SignedAngle(reference, (vertices[a] + push[a]) - center, normal);
                            var angleB = Vector3.SignedAngle(reference, (vertices[b] + push[b]) - center, normal);
                            return angleA.CompareTo(angleB);
                        });
                    }

                    // TODO: proper triangulation for polygons with more than three vertices (add a center vertex).
                    for (int j = 1; j < poly.Count - 1; ++j)
                    {
                        int v0 = poly[0];
                        int v1 = poly[j];
                        int v2 = poly[j + 1];

                        // Check if any vertices are not part of the original mesh.
                        if (v0 >= vertexCount || v1 >= vertexCount || v2 >= vertexCount)
                        {
                            if (!addChamferGeometry)
                            {
                                // Don't add the chamfer geometry on the boundary of the  original mesh.
                                continue;
                            }
                            else if (v0 >= vertexCount && v1 >= vertexCount && v2 >= vertexCount)
                            {
                                // Don't add chamfer geometry completely outside the original mesh.
                                continue;
                            }
                        }

                        Vector3 n = MathUtils.TriangleNormal(vertices[v0] + push[v0] * 0.01f, vertices[v1] + push[v1] * 0.01f, vertices[v2] + push[v2] * 0.01f);
                        if (Vector3.Dot(n, (normals[v0] + normals[v1] + normals[v2]) / 3f) < 0)
                        {
                            // Swap
                            int t = v1;
                            v1 = v2;
                            v2 = t;
                        }
                        chamferTris.Add(v0);
                        chamferTris.Add(v1);
                        chamferTris.Add(v2);
                    }
                }
            }

            // Move chamfer vertices.
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] += push[i] * chamferSize * scale;
            }

            // Merge original triangles and chamfer triangles.
            var newTris = new int[triangleCount + chamferTris.Count];
            for (var i = 0; i < triangleCount; ++i)
            {
                newTris[i] = tris[i];
            }
            for(var i = 0; i < chamferTris.Count; ++i)
            {
                newTris[i + triangleCount] = chamferTris[i];
            }
            tris = newTris;
            triangleCount += chamferTris.Count;

            // Remove vertices not in use anymore.
            RemoveUnusedVertices();
        }

        private bool ContainsTriangle(List<int> triangles, int v0, int v1, int v2)
        {
            for (int i = 0; i < triangles.Count; i += 3)
            {
                int tv0 = triangles[i];
                int tv1 = triangles[i + 1];
                int tv2 = triangles[i + 2];

                if ((v0 == tv0 && v1 == tv1 && v2 == tv2) ||
                    (v0 == tv0 && v1 == tv2 && v2 == tv1) ||
                    (v0 == tv1 && v1 == tv0 && v2 == tv2) ||
                    (v0 == tv1 && v1 == tv2 && v2 == tv0) ||
                    (v0 == tv2 && v1 == tv0 && v2 == tv1) ||
                    (v0 == tv2 && v1 == tv1 && v2 == tv0))
                {
                    return true;
                }
            }

            return false;
        }

        private void RemoveUnusedVertices()
        {
            // Remove unused vertices
            int[] remapper;
            int usedVertexCount = ComputeUsedVertices(ref tris, vertices.Length, out remapper);

            vertices = Remap(vertices, usedVertexCount, remapper);
            normals = Remap(normals, usedVertexCount, remapper);

            if (tangents != null)
                tangents = Remap(tangents, usedVertexCount, remapper);

            if (uvs != null)
                uvs = Remap(uvs, usedVertexCount, remapper);

            vertexCount = vertices.Length;
        }

        private int ComputeUsedVertices(ref int[] tris, int vertexCount, out int[] vertexMap)
        {
            vertexMap = new int[vertexCount];
            for (int i = 0; i < vertexCount; ++i)
            {
                vertexMap[i] = -1;
            }

            int usedVertexCount = 0;
            for (int i = 0; i < triangleCount; ++i)
            {
                int v = tris[i];
                if (vertexMap[v] < 0)
                {
                    vertexMap[v] = usedVertexCount;
                    usedVertexCount++;
                }
                tris[i] = vertexMap[v];
            }

            return usedVertexCount;
        }

        private T[] Remap<T>(T[] ts, int usedCount, int[] vertexMap)
        {
            if (ts == null || ts.Length == 0)
                return ts;

            T[] result = new T[usedCount];
            for (int i = 0; i < vertexMap.Length; ++i)
            {
                if (vertexMap[i] >= 0)
                {
                    result[vertexMap[i]] = ts[i];
                }
            }
            return result;
        }

        void ComputeVertexMap()
        {
            vertexMap = new int[vertices.Length];
            vertexCollapse = new int[vertices.Length];

            for (int i = 0; i < vertices.Length; ++i)
            {
                vertexMap[i] = i;
                vertexCollapse[i] = 1;
                for (int j = 0; j < i; ++j)
                {
                    if (Vector3.Distance(vertices[i], vertices[j]) < 0.0001f)
                    {
                        vertexMap[i] = j;
                        vertexCollapse[j]++; // Increase number of vertices mapped to this vertex
                        break;
                    }
                }
            }
        }

        public void ClearNormalMapUVs()
        {
            uvs = null;
        }

    }

}