using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public static class Utils
{
    public static (int x, int y) getTerrainDataPos(Terrain terrain, Vector3 world_pos)
    {
        var terrain_position = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;
        var terrain_size = terrainData.size;
        float relative_hitTerX = (world_pos.x - terrain_position.x) / terrain_size.x;
        float relative_hitTerZ = (world_pos.z - terrain_position.z) / terrain_size.z;

        float relativeTerCoordX = terrainData.heightmapResolution * relative_hitTerX;
        float relativeTerCoordZ = terrainData.heightmapResolution * relative_hitTerZ;

        int hitPointTerX = Mathf.RoundToInt(relativeTerCoordX);
        int hitPointTerZ = Mathf.RoundToInt(relativeTerCoordZ);

        return (hitPointTerX, hitPointTerZ);
    }

    public static Vector2 getWorldPos(Terrain terrain, int x, int y)
    {
        var terrain_position = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;
        var terrain_size = terrainData.size;
        float relative_hitTerX =  (float)x / terrainData.heightmapResolution;
        float relative_hitTerZ = (float)y / terrainData.heightmapResolution;
        return new Vector2(relative_hitTerX * terrain_size.x + terrain_position.x, relative_hitTerZ * terrain_size.z + terrain_position.z);
    }

    public static (Vector2 neartest, int index) getNearest(Vector2[] points, Vector2 target)
    {
        float dist_min = float.MaxValue;
        int index = 0;
        for (int i = 0; i < points.Length; i++)
        {
            float dist = Vector2.Distance(points[i], target);
            if (dist < dist_min)
            {
                dist_min = dist;
                index = i;
            }
        }
        return (points[index], index);
    }

    public static Vector2[] toVec2(Vector3[] points)
    {
        return toVec2(points, points.Length);
    }

    public static Vector2[] toVec2(Vector3[] points, int length)
    {
        Vector2[] coords = new Vector2[length];
        for (int i = 0; i < length; i++)
        {
            coords[i] = new Vector2(points[i].x, points[i].z);
        }
        return coords;
    }

    //***********************************************************************
    //
    // * Returns which side of the edge the line (x,y) is on. The return value
    //   is one of the constants defined above (LEFT, RIGHT, ON). See above
    //   for a discussion of which side is left and which is right.
    //=======================================================================
    public static int pointSide(Vector2 side_p1, Vector2 side_p2, Vector2 p)
    {
        // Compute the determinant: | xs ys 1 |
        //                          | xe ye 1 |
        //                          | x  y  1 |
        // Use its sign to get the answer.
        float det = side_p1.x *
            (side_p2.y - p.y) -
            side_p1.y *
            (side_p2.x - p.x) +
            side_p2.x * p.y -
            side_p2.y * p.x;
        if (det > 1e-6)
            return -1;
        else if (det < -1e-6)
            return 1;
        else
            return 0;
    }

    public static bool inSegment(Vector2 side_p1, Vector2 side_p2, Vector2 p)
    {
        Vector2 ab = side_p2 - side_p1;
        Vector2 ac = p - side_p1;
        Vector2 bc = p - side_p2;
        return Vector2.Dot(ab, ac) >= 0 && Vector2.Dot(-ab, bc) >= 0;
    }

    /// <summary>
    /// Determines if the given point is inside the polygon
    /// </summary>
    /// <param name="polygon">the vertices of polygon</param>
    /// <param name="testPoint">the given point</param>
    /// <returns>true if the point is inside the polygon; otherwise, false</returns>
    public static bool isPointInPolygon(Vector2[] polygon, Vector2 p)
    {
        bool result = true;
        int last_clockwise = 2;
        for (int point_index = 0; point_index < polygon.Length - 1; point_index++)
        {
            Vector2 a = new Vector2(polygon[point_index].x, polygon[point_index].y);
            Vector2 b = new Vector2(polygon[point_index + 1].x, polygon[point_index + 1].y);
            Vector2 c = new Vector2(p.x, p.y);
            
            int clockwise = Utils.pointSide(a, b, c);
            if (last_clockwise == 2)
            {
                last_clockwise = clockwise;
            }
            if (clockwise != last_clockwise && clockwise != 0 && inSegment(a, b, c))
            {
                result = false;
                break;
            }
        }

        //int j = polygon.Count() - 1;
        //for (int i = 0; i < polygon.Count(); i++)
        //{
        //    if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y || polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y)
        //    {
        //        if (polygon[i].x + (testPoint.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].y - polygon[i].y) < testPoint.x)
        //        {
        //            result = !result;
        //        }
        //    }
        //    j = i;
        //}
        return result;
    }

    static public bool pointInPolygon(Vector2[] polygon, Vector2 p)
    {

        int i, j = polygon.Length - 1;
        bool oddNodes = false;

        for (i = 0; i < polygon.Length; i++)
        {
            if (polygon[i].y < p.y && polygon[j].y >= p.y
            || polygon[j].y < p.y && polygon[i].y >= p.y)
            {
                if (polygon[i].x + (p.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < p.x)
                {
                    oddNodes = !oddNodes;
                }
            }
            j = i;
        }

        return oddNodes;
    }

    static public float getSFunction(float a, float b, float step)
    {
        float weight = (Mathf.Sin(step * Mathf.PI - Mathf.PI / 2) + 1) / 2;
        return (1 - weight) * a + weight * b;
    }
}