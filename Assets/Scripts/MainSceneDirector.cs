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
    public void StreamWriterStart(string[] strArry)
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
            sw.WriteLine(string.Join(",", strArry));
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
    [SerializeField] private Transform[] _movingTargetParents;
    private Transform[] _targetTrans;
    private int[] childIndex;
    public override string[] _startStrArray()
    {
        childIndex = new int[_movingTargetParents.Count()];
        _targetTrans = new Transform[]{
            _movingTargetParents[0].GetChild(childIndex[0]=0),
            _movingTargetParents[1].GetChild(childIndex[1]=0)
        };

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
        NearestTarget(_movingTargetParents, 0);
        var carDistanceVt3 = _targetTrans[0].position - _carTrans.position;

        NearestTarget(_movingTargetParents, 1);
        var bikeDistanceVt3 = _targetTrans[1].position - _carTrans.position;

        string[] s = new string[]{
            string.Empty,
            carDistanceVt3.x.ToString(_format),
            carDistanceVt3.z.ToString(_format),
            carDistanceVt3.magnitude.ToString(_format),
            bikeDistanceVt3.x.ToString(_format),
            bikeDistanceVt3.z.ToString(_format),
            bikeDistanceVt3.magnitude.ToString(_format)
        };
        s = base._updateStrArray().Concat(s).ToArray();

        return s;
    }
    private void NearestTarget(Transform[] parentTrans, int parentTransIndex)
    {
        var oldDistanceVt3 = _targetTrans[parentTransIndex].position - _carTrans.position;
        if (oldDistanceVt3.z > 0) return;
        childIndex[parentTransIndex]++;
        _targetTrans[parentTransIndex] = parentTrans[parentTransIndex].GetChild(childIndex[parentTransIndex]);
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
            _exportDistance.StreamWriterStart(_exportDistance._startStrArray());
        }
    }
    void FixedUpdate()
    {
        if (_exportDistance._isExportCsvfile)
        {
            _exportDistance.SaveData(_exportDistance._updateStrArray());
        }
    }
}

