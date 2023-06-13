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

    [SerializeField] private bool _isTargetVisualize;
    [SerializeField] private Transform[] _movingTargetParents;
    [SerializeField] private GameObject _targetBoxObject;
    [SerializeField] private GameObject _targetSphereObject;
    private Transform[] _targetTrans;
    private int[] _childIndex;
    private Transform[] _hitCpuPoint;
    private Transform[] _hitPlayerPoint;
    public override string[] _startStrArray()
    {
        _childIndex = new int[_movingTargetParents.Count()];

        _targetTrans = new Transform[]{
            _movingTargetParents[(int)TargetGroup.Cars].GetChild(_childIndex[(int)TargetGroup.Cars]=0),
            _movingTargetParents[(int)TargetGroup.Bikes].GetChild(_childIndex[(int)TargetGroup.Bikes]=0)
        };

        _hitPlayerPoint = new Transform[_movingTargetParents.Count()];
        _hitCpuPoint = new Transform[_movingTargetParents.Count()];

        RayControll((int)TargetGroup.Cars);
        RayControll((int)TargetGroup.Bikes);
        if (_isTargetVisualize)
        {
            CreateTargetBoxObj((int)TargetGroup.Cars, out _);
            CreateTargetBoxObj((int)TargetGroup.Bikes, out _);
        }

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
        RayControll((int)TargetGroup.Cars);
        var carDirection = _hitCpuPoint[(int)TargetGroup.Cars].position - _hitPlayerPoint[(int)TargetGroup.Cars].position;

        NearestTarget(_movingTargetParents, (int)TargetGroup.Bikes);
        RayControll((int)TargetGroup.Bikes);
        var bikeDirection = _hitCpuPoint[(int)TargetGroup.Bikes].position - _hitPlayerPoint[(int)TargetGroup.Bikes].position;

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
    private void NearestTarget(Transform[] parentTrans, int parentTransIndex)
    {
        var oldDirection = _targetTrans[parentTransIndex].position - _carTrans.position;
        if (parentTransIndex == 1)
        {
            Debug.Log("Change Object to " + oldDirection.z);
        }
        if (oldDirection.z > 0) return;
        if (_childIndex[parentTransIndex] == parentTrans[parentTransIndex].childCount - 1) return;//last object
        _childIndex[parentTransIndex]++;
        _targetTrans[parentTransIndex] = parentTrans[parentTransIndex].GetChild(_childIndex[parentTransIndex]);

        TargetBoxControll(parentTrans, parentTransIndex);
    }
    private void RayControll(int parentTransIndex)
    {
        Vector3 direction = _targetTrans[parentTransIndex].position - _carTrans.position;
        Vector3 rayPositionOffset = new Vector3(0, 0.6f, 0);
        Ray rayToCpu = new Ray(_carTrans.position + rayPositionOffset, direction);
        RayHitControll(rayToCpu, _hitCpuPoint, 1 << 7, parentTransIndex);

        Ray rayToPlayer = new Ray(_targetTrans[parentTransIndex].position + rayPositionOffset, -direction);
        RayHitControll(rayToPlayer, _hitPlayerPoint, 1 << 6, parentTransIndex);
    }
    private void RayHitControll(Ray ray, Transform[] hitPoint, int layerMask, int parentTransIndex)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            if (hitPoint[parentTransIndex] == null)
            {
                hitPoint[parentTransIndex] = GameObject.Instantiate(_targetSphereObject, hit.point, Quaternion.identity).transform;
                if (_isTargetVisualize)
                {
                    hitPoint[parentTransIndex].GetComponent<Renderer>().material.SetColor("_UnlitColor", Color.yellow);
                }
            }
            hitPoint[parentTransIndex].position = hit.point;
            if (!_isTargetVisualize) return;
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.yellow, 0, false);
        }
        else
        {
            Debug.Log("Ray Hit Nothing!!");
        }
    }
    private void TargetBoxControll(Transform[] parentTrans, int parentTransIndex)
    {
        if (!_isTargetVisualize) return;
        string objName;
        CreateTargetBoxObj(parentTransIndex, out objName);
        parentTrans[parentTransIndex].GetChild(_childIndex[parentTransIndex] - 1).Find(objName).gameObject.SetActive(false);
    }
    private void CreateTargetBoxObj(int parentTransIndex, out string objName)
    {
        GameObject obj = GameObject.Instantiate(_targetBoxObject, _targetTrans[parentTransIndex]);
        if (parentTransIndex == (int)TargetGroup.Cars)
        {
            obj.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            obj.transform.localScale = new Vector3(2.5f, 2.0f, 5.5f);
        }
        else
        {
            obj.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            obj.transform.localScale = new Vector3(1.0f, 2.0f, 2.1f);
        }
        objName = obj.name;
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
}

