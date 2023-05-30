using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class FrameRateController
{
    public int _frameRate = 60;
}
[System.Serializable]
public class ExportExcel
{
    public bool _isExportFile;
    private Transform _carTrans;
    private CarController _car;
    private string _path;
    float _time;
    // Start is called before the first frame update
    public void StreamWriterStart()
    {
        if (!_isExportFile) return;
        _carTrans = GameObject.FindGameObjectWithTag("Player").transform;
        _car = _carTrans.GetComponent<CarController>();

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
        using (StreamWriter sw = new StreamWriter(_path, false, Encoding.GetEncoding("Shift_JIS")))
        {
            sw.WriteLine(string.Join(",", s));
        }
    }
    public void SaveData()
    {
        if (!_isExportFile) return;

        var carPos = _carTrans.position;
        if (carPos.z < 0.01f) return;

        var rotateY = _carTrans.rotation.eulerAngles.y;
        var velocity = _carTrans.GetComponent<Rigidbody>().velocity;
        _time += Time.deltaTime;
        rotateY = (rotateY > 180) ? rotateY - 360 : rotateY;

        string format = "f4";
        string[] s = new string[]
        {
            DateTime.Now.ToLongTimeString(),
            _time.ToString(format),
            Time.deltaTime.ToString(format),
            carPos.x.ToString(format),
            carPos.z.ToString(format),
            (velocity.magnitude*3600/1000).ToString(format),
            rotateY.ToString(format),
            String.Empty,
            _car.AccelInput.ToString(format),
            _car.SteeringInput.ToString(format),
            _car.BrakeInput.ToString(format)
        };
        using (StreamWriter sw = new StreamWriter(_path, true, Encoding.GetEncoding("Shift_JIS")))
        {
            sw.WriteLine(string.Join(",", s));
        }
    }
}
public class MainSceneDirector : MonoBehaviour
{
    [SerializeField] private FrameRateController frameRateController;
    [SerializeField] private ExportExcel exportExcel;
    void Start()
    {
        Application.targetFrameRate = frameRateController._frameRate;

        exportExcel.StreamWriterStart();
    }
    void FixedUpdate()
    {
        exportExcel.SaveData();
    }
}

