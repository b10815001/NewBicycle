using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent (typeof (PolygonCollider2D))]
/// <summary>
/// Builds a Mesh for a gameObject using the PolygonCollider2D's path</summary>
public class bgPolygonCreator : MonoBehaviour {
	public string savePath = "Assets";
	public string saveFileName = "polygon";
	public bool editing = false;
	public Vector2[] backupPolygon;
	public Vector2[] editingPolygon;
	public float polygonCoordTranslateX = 0.0f;
	public float polygonCoordTranslateY = 0.0f;
	public float polygonCoordScale = 1.0f;
	public float polygonCoordRotate = 0.0f;
	protected PolygonCollider2D polygon;

	///<summary>The index of the polygon path to use for the mesh</summary>
	int pathIndex = 0;
	///<summary>The Z position for the generated mesh</summary>
	public float height = 0f;
	#if UNITY_EDITOR
	void Start() 
	{
		polygon = GetComponent<PolygonCollider2D>();
	}

    private void OnEnable()
    {
		polygon = GetComponent<PolygonCollider2D>();
	}


	///<summary>
	///(Re)builds the Mesh using the path of the PolygonCollider2D</summary>
	public void OnColliderUpdate() {
		Vector2[] path = polygon.GetPath(pathIndex);

		//Mesh msh = new Mesh();

		//msh.vertices = path.Select(v => new Vector3(v.x, v.y, zPosition)).ToArray();
		//msh.triangles = new Triangulator(path).Triangulate();

		//msh.RecalculateNormals();
		//msh.RecalculateBounds();
		//meshFilter.mesh = msh;

		////recalculate UV
		//Bounds bounds = msh.bounds;

		//msh.uv = path.Select(v => new Vector2(v.x / bounds.size.x, v.y / bounds.size.y)).ToArray();
	}

    void Update() {
		if (!Application.isPlaying) OnColliderUpdate();	
	}


	public void SavePolygonAsset()
	{
		bgPolygonAsset polygonAsset = ScriptableObject.CreateInstance<bgPolygonAsset>();
		Vector2[] point2DArray = polygon.GetPath(pathIndex);
		Vector3[] point3DArray = new Vector3[point2DArray.Length];
		for (int i = 0; i < point3DArray.Length; i++)
		{
			point3DArray[i] = new Vector3(point2DArray[i].x, height, point2DArray[i].y);
		}
		polygonAsset.points = point3DArray;
		string filename = savePath + "/" + saveFileName + ".asset";
		int postfixNumber = 1;
		while (File.Exists(filename))
		{
			filename = savePath + "/" + saveFileName + postfixNumber.ToString() + ".asset";
			postfixNumber++;
		}
		AssetDatabase.CreateAsset(polygonAsset, filename);
		EditorUtility.SetDirty(polygonAsset);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	public void StartEditing()
	{
		Vector2[] path = polygon.GetPath(pathIndex);

		backupPolygon = new Vector2[path.Length];
		editingPolygon = new Vector2[path.Length];
		for (int i = 0; i < path.Length; i++)
		{
			backupPolygon[i] = path[i];
			editingPolygon[i] = path[i];
		}
		polygon.SetPath(pathIndex, editingPolygon);

		editing = true;
	}

	public void PolygonEditing()
	{
		Vector2[] path = backupPolygon;
		for (int i = 0; i < path.Length; i++)
		{

			//scale
			editingPolygon[i] = path[i] * polygonCoordScale;
			//rotate
			float sin_t = Mathf.Sin(polygonCoordRotate * Mathf.Deg2Rad);
			float cos_t = Mathf.Cos(polygonCoordRotate * Mathf.Deg2Rad);
			float rotateX = editingPolygon[i].y * sin_t + editingPolygon[i].x * cos_t;
			float rotateY = editingPolygon[i].y * cos_t - editingPolygon[i].x * sin_t;
			editingPolygon[i] = new Vector2(rotateX, rotateY);

			//translate
			editingPolygon[i] = new Vector2(editingPolygon[i].x + polygonCoordTranslateX, editingPolygon[i].y + polygonCoordTranslateY);
		}

		polygon.SetPath(pathIndex, editingPolygon);
	}

	public void ApplyEditing()
	{
		polygonCoordScale = 1.0f;
		polygonCoordRotate = 0.0f;
		editing = false;
	}

	public void UndoEditing()
	{
		polygonCoordTranslateX = 0.0f;
		polygonCoordTranslateY = 0.0f;
		polygonCoordScale = 1.0f;
		polygonCoordRotate = 0.0f;
		editing = false;
		polygon.SetPath(pathIndex, backupPolygon);
	}
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(bgPolygonCreator))]
public class bgPolygonCreatorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		bgPolygonCreator polygonCreator = (bgPolygonCreator)target;
		if (polygonCreator.editing == false)
		{
			if (GUILayout.Button("Start Editing"))
			{
				polygonCreator.StartEditing();
			}
		}
		else
		{
			polygonCreator.polygonCoordTranslateX = EditorGUILayout.Slider("Translate X", polygonCreator.polygonCoordTranslateX, -5.0f, 5.0f);
			polygonCreator.polygonCoordTranslateY = EditorGUILayout.Slider("Translate Y", polygonCreator.polygonCoordTranslateY, -5.0f, 5.0f);
			polygonCreator.polygonCoordScale = EditorGUILayout.Slider("Polygon Scale", polygonCreator.polygonCoordScale, 0.001f, 5.0f);
			polygonCreator.polygonCoordRotate = EditorGUILayout.Slider("Polygon Rotate", polygonCreator.polygonCoordRotate, -180.0f, 180.0f);
			polygonCreator.PolygonEditing();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply"))
			{
				polygonCreator.ApplyEditing();
			}
			if (GUILayout.Button("Undo"))
			{
				polygonCreator.UndoEditing();
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		polygonCreator.savePath = EditorGUILayout.TextField(polygonCreator.savePath);
		if (GUILayout.Button("Browse Path"))
		{
			string getPath = EditorUtility.OpenFolderPanel("Select Folder", polygonCreator.savePath, "");
			getPath = FileUtil.GetProjectRelativePath(getPath);
			if (getPath.Length >= 6) //Assets(six word)
			{
				if (getPath.Substring(0, 6) == "Assets")
				{
					polygonCreator.savePath = getPath;
				}
				else
				{
					Debug.LogWarning("Path must be inside unity project (under Assets)");
				}
			}
			else
			{
				Debug.LogWarning("Path must be inside unity project (under Assets)");
			}
			GUIUtility.ExitGUI();//solve for "EndLayoutGroup: BeginLayoutGroup must be called first."
		}
		EditorGUILayout.EndHorizontal();
		polygonCreator.saveFileName = EditorGUILayout.TextField("File Name:",polygonCreator.saveFileName);
		if (GUILayout.Button("Save Polygon Asset"))
		{
			polygonCreator.SavePolygonAsset();
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif



//class Triangulator {
//	private List<Vector2> mPoints = new List<Vector2>();

//	public Triangulator (Vector2[] points) {
//		mPoints = new List<Vector2>(points);
//	}

//	public int[] Triangulate() {
//		List<int> indices = new List<int>();

//		int n = mPoints.Count;
//		if (n < 3) return indices.ToArray();

//		int[] V = new int[n];
//		if (Area() > 0) {
//			for (int v = 0; v < n; v++)
//				V[v] = v;
//		}
//		else {
//			for (int v = 0; v < n; v++)
//				V[v] = (n - 1) - v;
//		}

//		int nv = n;
//		int count = 2 * nv;
//		for (int m = 0, v = nv - 1; nv > 2; ) {
//			if ((count--) <= 0)
//				return indices.ToArray();

//			int u = v;
//			if (nv <= u)
//				u = 0;
//			v = u + 1;
//			if (nv <= v)
//				v = 0;
//			int w = v + 1;
//			if (nv <= w)
//				w = 0;

//			if (Snip(u, v, w, nv, V)) {
//				int a, b, c, s, t;
//				a = V[u];
//				b = V[v];
//				c = V[w];
//				indices.Add(a);
//				indices.Add(b);
//				indices.Add(c);
//				m++;
//				for (s = v, t = v + 1; t < nv; s++, t++)
//					V[s] = V[t];
//				nv--;
//				count = 2 * nv;
//			}
//		}

//		indices.Reverse();
//		return indices.ToArray();
//	}

//	private float Area () {
//		int n = mPoints.Count;
//		float A = 0.0f;
//		for (int p = n - 1, q = 0; q < n; p = q++) {
//			Vector2 pval = mPoints[p];
//			Vector2 qval = mPoints[q];
//			A += pval.x * qval.y - qval.x * pval.y;
//		}
//		return (A * 0.5f);
//	}

//	private bool Snip (int u, int v, int w, int n, int[] V) {
//		int p;
//		Vector2 A = mPoints[V[u]];
//		Vector2 B = mPoints[V[v]];
//		Vector2 C = mPoints[V[w]];
//		if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
//			return false;
//		for (p = 0; p < n; p++) {
//			if ((p == u) || (p == v) || (p == w))
//				continue;
//			Vector2 P = mPoints[V[p]];
//			if (InsideTriangle(A, B, C, P))
//				return false;
//		}
//		return true;
//	}

//	private bool InsideTriangle (Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
//		float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
//		float cCROSSap, bCROSScp, aCROSSbp;

//		ax = C.x - B.x; ay = C.y - B.y;
//		bx = A.x - C.x; by = A.y - C.y;
//		cx = B.x - A.x; cy = B.y - A.y;
//		apx = P.x - A.x; apy = P.y - A.y;
//		bpx = P.x - B.x; bpy = P.y - B.y;
//		cpx = P.x - C.x; cpy = P.y - C.y;

//		aCROSSbp = ax * bpy - ay * bpx;
//		cCROSSap = cx * apy - cy * apx;
//		bCROSScp = bx * cpy - by * cpx;

//		return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
//	}
//}
