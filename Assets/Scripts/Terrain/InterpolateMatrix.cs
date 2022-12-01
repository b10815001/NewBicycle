using RoadArchitect;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Search;
#endif
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

[ExecuteInEditMode]
public class InterpolateMatrix : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    GameObject road_architect;
    [SerializeField]
    bool get_road_constraint = false;
    [SerializeField]
    bool do_constraint = true;
    [SerializeField]
    Terrain terrain;
    Road road;
    Vector3[] road_polygon;
    Vector3[] road_rights;

    // Update is called once per frame
    void Update()
    {
        if (get_road_constraint)
        {
            get_road_constraint = false;
            if (road_architect.transform.GetChild(2).gameObject.TryGetComponent<Road>(out road))
            {
                int segment_max = 10;
                float step = 1.0f / (segment_max - 1);
                road_polygon = new Vector3[segment_max * 2];
                road_rights = new Vector3[segment_max];
                GameObject cube_manager = new GameObject("Cube Manager");
                for (int segment_i = 0; segment_i < segment_max; segment_i++)
                {
                    Vector3 road_point_pos, road_tangent;
                    road.getPosAndTangent(16, step * segment_i, out road_point_pos, out road_tangent);
                    road_rights[segment_i] = new Vector3((road.laneWidth + road.shoulderWidth) * road_tangent.normalized.z, 0, (road.laneWidth + road.shoulderWidth) * -road_tangent.normalized.x);

                    // right
                    road_polygon[segment_i] = road_point_pos + road_rights[segment_i];

                    // right right
                    road_polygon[2 * segment_max - segment_i - 1] = road_point_pos + 4 * road_rights[segment_i];

                    //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //cube.transform.position = road_point_pos;
                    //cube.transform.parent = cube_manager.transform;

                    GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube2.transform.position = road_polygon[segment_i];
                    cube2.GetComponent<MeshRenderer>().material.color = UnityEngine.Color.blue;
                    cube2.name = segment_i.ToString();
                    cube2.transform.parent = cube_manager.transform;

                    GameObject cube3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube3.transform.position = road_polygon[2 * segment_max - segment_i - 1];
                    cube3.GetComponent<MeshRenderer>().material.color = UnityEngine.Color.green;
                    cube3.name = (2 * segment_max - segment_i - 1).ToString();
                    cube3.transform.parent = cube_manager.transform;
                }
            }
            else
            {
                Debug.LogError("Road.cs not be loaded");
            }
        }

        if (do_constraint)
        {
            do_constraint = false;

            //Matrix<double> A = DenseMatrix.OfArray(new double[,] {
            //    {1,1,1,1},
            //    {1,2,3,4},
            //    {4,3,2,1}});
            //Vector<double>[] nullspace = A.Kernel();

            //// verify: the following should be approximately (0,0,0)
            //Debug.Log((A * (2 * nullspace[0] - 3 * nullspace[1])).ToString());

            int road_point_count = road_polygon.Length / 2;
            float shoulder_width = road.shoulderWidth;
            float step = 1.0f / (shoulder_width - 1);
            for (int segment_i = 0; segment_i < road_point_count; segment_i++)
            {
                Vector3 road_extend = road_polygon[segment_i] + road_rights[segment_i] * step * segment_i;
                
            }
        }
    }
#endif
}