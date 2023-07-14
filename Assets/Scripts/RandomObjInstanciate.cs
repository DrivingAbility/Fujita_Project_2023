using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjInstanciate : MonoBehaviour
{
    [SerializeField] GameObject dotObj;
    [SerializeField] int _numPerSquare = 10;
    [SerializeField, Range(0f, 0.1f)] float _minDotSize = 0.01f;
    [SerializeField, Range(0f, 0.1f)] float _maxDotSize = 0.03f;
    [SerializeField] float _meshSquareLength = 12.0f;
    [SerializeField] Vector2 _tilling = Vector2.one;
    [SerializeField, Range(0f, 2f)] float height;
    string parentPlaneName = "ParentDotPlane";
    [ContextMenu("GenerateDots")]
    public void GenerateDots()
    {
        GenerateDotsMesh();
    }
    public void GenerateDotsMesh()
    {
        if (GameObject.Find(parentPlaneName)) DestroyImmediate(GameObject.Find(parentPlaneName));
        var parentObj = new GameObject(parentPlaneName);

        int dotsCount = (int)Mathf.Pow(_meshSquareLength, 2) * _numPerSquare;
        CombineInstance[] combine = new CombineInstance[dotsCount];
        for (int i = 0; i < dotsCount; i++)
        {
            Vector3 position = new Vector3(0, 0, 0);
            position.x = Random.Range(-_meshSquareLength / 2, _meshSquareLength / 2);
            position.y = Random.Range(0, height);
            position.z = Random.Range(-_meshSquareLength / 2, _meshSquareLength / 2);

            dotObj.transform.position = position;
            dotObj.transform.localScale = Vector3.one * Random.Range(_minDotSize, _maxDotSize);
            combine[i].mesh = dotObj.GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = dotObj.GetComponent<MeshFilter>().transform.localToWorldMatrix;
        }

        var combineObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        combineObj.name = "SphereDots";
        GameObject.DestroyImmediate(combineObj.GetComponent<SphereCollider>());
        var parentMeshFilter = combineObj.GetComponent<MeshFilter>();
        var parentMeshRenderer = combineObj.GetComponent<MeshRenderer>();
        parentMeshFilter.sharedMesh = new Mesh();
        parentMeshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        parentMeshFilter.sharedMesh.name = "CombinedMesh";
        parentMeshFilter.sharedMesh.CombineMeshes(combine);
        parentMeshRenderer.sharedMaterial = dotObj.GetComponent<MeshRenderer>().sharedMaterial;
        parentMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        for (int i = 0; i < _tilling.x; i++)
        {
            for (int j = 0; j < _tilling.y; j++)
            {
                GameObject unitObj = Instantiate(combineObj, new Vector3(-(_tilling.x - 1) * _meshSquareLength / 2 + _meshSquareLength * i, 0, _meshSquareLength * j + _meshSquareLength / 2), Quaternion.identity, parentObj.transform);
                unitObj.isStatic = true;
            }
        }
        GameObject.DestroyImmediate(combineObj);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
