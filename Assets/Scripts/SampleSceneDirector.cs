using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SampleSceneDirector : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] _tmpro;
    [SerializeField] Transform[] _bikeTf;
    [SerializeField] Vector3[] _startPos;
    // Start is called before the first frame update
    void Start()
    {
        _startPos = new Vector3[2];
        _startPos[0] = _bikeTf[0].position;
        _startPos[1] = _bikeTf[1].position;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _bikeTf.Length; i++)
        {
            TMPControll(_bikeTf[i], _tmpro[i], i);
        }
    }
    void TMPControll(Transform transform, TextMeshProUGUI tmpro, int i)
    {
        tmpro.SetText((transform.position.x - _startPos[i].x).ToString("f2") + " m");
    }
}
