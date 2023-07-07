using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class FrameRateController
{
    public int _frameRate = 60;
}
[System.Serializable]
public class ExportExcel
{
    public bool _isExportCsvfile;
    protected Transform _carTrans;
    private CarController _playerCar;
    private string _path;
    float _time;
    protected string _format = "f4";

    public virtual string[] _startStrArray()
    {
        string[] s = new string[]
        {
            DateTime.Now.ToLongDateString(),
            "Time",
            "ΔTime",
            "Xpos",
            "Zpos",
            "Velocity",
            "RotateY",
            "InputValue",
            "AcceInput",
            "SteeringInput",
            "BrakeInput"
        };

        return s;
    }
    // Start is called before the first frame update
    public void StreamWriterStart()
    {
        _carTrans = GameObject.FindGameObjectWithTag("Player").transform;
        _playerCar = _carTrans.GetComponent<CarController>();

        string datetimeStr = DateTime.Now.ToString("yyyy_MM_dd");
        _path = Application.dataPath + @"\Excel Data\" + datetimeStr + @"\";
        Directory.CreateDirectory(_path);
        _path += @"\" + "SaveData.csv";

        string path1 = _path;
        int fileNum = 0;
        bool isFileFound = true;
        while (isFileFound)
        {
            if (System.IO.File.Exists(path1))
            {
                fileNum++;
                path1 = _path;
                path1 = _path.Replace(".csv", fileNum.ToString() + ".csv");
            }
            else
            {
                isFileFound = false;
                _path = path1;
            }
        }

        using (StreamWriter sw = new StreamWriter(_path, false, Encoding.GetEncoding("Shift_JIS")))
        {
            sw.WriteLine(string.Join(",", _startStrArray()));
        }
    }
    public virtual string[] _updateStrArray()
    {
        var carPos = _carTrans.position;
        var rotateY = _carTrans.rotation.eulerAngles.y;
        var velocity = _carTrans.GetComponent<Rigidbody>().velocity;
        _time += Time.deltaTime;
        rotateY = (rotateY > 180) ? rotateY - 360 : rotateY;

        string[] s = new string[]
        {
            DateTime.Now.ToLongTimeString(),
            _time.ToString(_format),
            Time.deltaTime.ToString(_format),
            carPos.x.ToString(_format),
            carPos.z.ToString(_format),
            (velocity.magnitude*3600/1000).ToString(_format),
            rotateY.ToString(_format),
            String.Empty,
            _playerCar.AccelInput.ToString(_format),
            _playerCar.SteeringInput.ToString(_format),
            _playerCar.BrakeInput.ToString(_format)
        };

        return s;
    }
    public void SaveData(string[] strArray)
    {
        if (_carTrans.position.z < 0.01f) return;
        using (StreamWriter sw = new StreamWriter(_path, true, Encoding.GetEncoding("Shift_JIS")))
        {
            sw.WriteLine(string.Join(",", strArray));
        }
    }
}
[Serializable]
public class ExportDistance : ExportExcel
{
    private enum TargetGroup
    {
        Cars = 0,
        Bikes = 1
    }
    [SerializeField] private Transform[] _movingTargetParents;
    private float _rayPositionY;
    private Collider[] _targetCollider;
    [SerializeField] private Collider _playerCollider;
    private Transform[] _targetTrans;
    private int[] _childIndex;
    private Vector3[] _hitCpuPosition;
    private Vector3[] _hitPlayerPosition;
    public Vector3[] HitCpuPosition { get; private set; }
    public Vector3[] HitPlayerPosition { get; private set; }
    public override string[] _startStrArray()
    {
        _rayPositionY = 0.6f;
        _childIndex = new int[_movingTargetParents.Count()];

        _targetTrans = new Transform[]{
            _movingTargetParents[(int)TargetGroup.Cars].GetChild(_childIndex[(int)TargetGroup.Cars]=0),
            _movingTargetParents[(int)TargetGroup.Bikes].GetChild(_childIndex[(int)TargetGroup.Bikes]=0)
        };
        _targetCollider = new Collider[_movingTargetParents.Count()];
        TargetColliderSetting((int)TargetGroup.Cars);
        TargetColliderSetting((int)TargetGroup.Bikes);

        _hitPlayerPosition = new Vector3[_movingTargetParents.Count()];
        _hitCpuPosition = new Vector3[_movingTargetParents.Count()];

        string[] s = new string[]{
            "Distance",
            "CarPos.x",
            "CarPos.z",
            "CarDistance",
            "Bike.x",
            "Bike.z",
            "BikeDistance"
        };
        s = base._startStrArray().Concat(s).ToArray();

        return s;
    }
    public override string[] _updateStrArray()
    {
        NearestTarget(_movingTargetParents, (int)TargetGroup.Cars);
        ClosestPointing((int)TargetGroup.Cars);
        var carDirection = _hitCpuPosition[(int)TargetGroup.Cars] - _hitPlayerPosition[(int)TargetGroup.Cars];


        NearestTarget(_movingTargetParents, (int)TargetGroup.Bikes);
        ClosestPointing((int)TargetGroup.Bikes);
        var bikeDirection = _hitCpuPosition[(int)TargetGroup.Bikes] - _hitPlayerPosition[(int)TargetGroup.Bikes];

        HitCpuPosition = _hitCpuPosition;
        HitPlayerPosition = _hitPlayerPosition;

        string[] s = new string[]{
            string.Empty,
            carDirection.x.ToString(_format),
            carDirection.z.ToString(_format),
            carDirection.magnitude.ToString(_format),
            bikeDirection.x.ToString(_format),
            bikeDirection.z.ToString(_format),
            bikeDirection.magnitude.ToString(_format)
        };
        s = base._updateStrArray().Concat(s).ToArray();

        return s;
    }
    private void TargetColliderSetting(int parentTransIndex)
    {
        Vector3 direction = _targetTrans[parentTransIndex].position - _carTrans.position;
        RaycastHit hit;
        Vector3 origin = new Vector3(_carTrans.position.x, _rayPositionY, _carTrans.position.z);
        if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity))
        {
            _targetCollider[parentTransIndex] = hit.collider;
        }
    }
    private void NearestTarget(Transform[] parentTrans, int parentTransIndex)
    {
        var oldDirection = _targetTrans[parentTransIndex].position - _carTrans.position;
        if (oldDirection.z > 0) return;
        if (_childIndex[parentTransIndex] == parentTrans[parentTransIndex].childCount - 1) return;//last object
        _childIndex[parentTransIndex]++;
        _targetTrans[parentTransIndex] = parentTrans[parentTransIndex].GetChild(_childIndex[parentTransIndex]);

        TargetColliderSetting(parentTransIndex);
    }
    private void ClosestPointing(int parentTransIndex)
    {
        Vector3 origin = new Vector3(_carTrans.position.x, _rayPositionY, _carTrans.position.z);
        _hitCpuPosition[parentTransIndex] = _targetCollider[parentTransIndex].ClosestPoint(origin);
        _hitCpuPosition[parentTransIndex].y = _rayPositionY;

        _hitPlayerPosition[parentTransIndex] = _playerCollider.ClosestPoint(_hitCpuPosition[parentTransIndex]);
        _hitPlayerPosition[parentTransIndex].y = _rayPositionY;
    }
}
public class MainSceneDirector : MonoBehaviour
{
    [SerializeField] private FrameRateController _frameRateController;
    [SerializeField] private ExportDistance _exportDistance;
    void Start()
    {
        Application.targetFrameRate = _frameRateController._frameRate;

        if (_exportDistance._isExportCsvfile)
        {
            _exportDistance.StreamWriterStart();
        }
    }
    void Update()
    {
        if (_exportDistance._isExportCsvfile)
        {
            _exportDistance.SaveData(_exportDistance._updateStrArray());
        }
    }
    void OnDrawGizmos()
    {
        if (_exportDistance.HitCpuPosition == null || _exportDistance.HitPlayerPosition == null) return;
        float radius = 0.2f;
        Gizmos.color = Color.yellow;
        Vector3[] to = new Vector3[2];
        Vector3[] from = new Vector3[2];
        for (int i = 0; i < _exportDistance.HitCpuPosition.Length; i++)
        {
            Vector3 position = _exportDistance.HitCpuPosition[i];
            Gizmos.DrawSphere(position, radius);
            to[i] = position;
        }
        for (int i = 0; i < _exportDistance.HitPlayerPosition.Length; i++)
        {
            Vector3 position = _exportDistance.HitPlayerPosition[i];
            Gizmos.DrawSphere(position, radius);
            from[i] = position;
        }
        for (int i = 0; i < Mathf.Min(from.Length, to.Length); i++)
        {
            Gizmos.DrawLine(from[i], to[i]);
        }
    }
}

