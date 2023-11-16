using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[ExecuteAlways]
public class LinesData : MonoBehaviour
{
    [SerializeField] LineRenderer _lineFwdRU;
    [SerializeField] LineRenderer _lineFwdRD;
    [SerializeField] LineRenderer _lineFwdLU;
    [SerializeField] LineRenderer _lineFwdLD;
    [SerializeField] LineRenderer _lineRightU;
    [SerializeField] LineRenderer _lineRightD;
    [SerializeField] LineRenderer _lineUpRF;
    [SerializeField] LineRenderer _lineUpRB;
    [SerializeField] LineRenderer _lineUpLF;
    [SerializeField] LineRenderer _lineUpLB;
    public LineRenderer LineFwdRU => _lineFwdRU;
    public LineRenderer LineFwdRD => _lineFwdRD;
    public LineRenderer LineFwdLU => _lineFwdLU;
    public LineRenderer LineFwdLD => _lineFwdLD;
    public LineRenderer LineRightU => _lineRightU;
    public LineRenderer LineRightD => _lineRightD;
    public LineRenderer LineUpRF => _lineUpRF;
    public LineRenderer LineUpRB => _lineUpRB;
    public LineRenderer LineUpLF => _lineUpLF;
    public LineRenderer LineUpLB => _lineUpLB;
    // Start is called before the first frame update

    // Update is called once per frame

}
