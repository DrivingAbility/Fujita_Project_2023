using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class FrameRateController
{
    public int _frameRate = 30;
}
[System.Serializable]
public class ExportExcel
{
    public bool _isExportCsvfile;
    protected Transform _carTrans;
    private CarController _playerCar;
    private ModelTypeController.ModelType _type;
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
            "Hitting",
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

        _type = GameObject.FindObjectOfType<MainSceneDirector>()._modelTypeController.Type;

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
            sw.WriteLine(string.Join(",", _type.ToString()));
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
            (_playerCar.CollisionInfo!=null).ToString(),
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
    [SerializeField] int _scooterCount = 7;
    [SerializeField] private Transform[] _targetCPUParents;
    private Transform[] _activeTargetCPUParents;
    private float _rayPositionY;
    private Collider[] _targetCollider;
    private Collider _playerCollider;
    private Transform[] _targetTrans;
    private int[] _childIndex;
    private Vector3[] _hitCpuPosition;
    private Vector3[] _hitPlayerPosition;
    public Vector3[] HitCpuPosition { get; private set; }
    public Vector3[] HitPlayerPosition { get; private set; }
    public bool Finished { get; private set; }
    public override string[] _startStrArray()
    {

        _rayPositionY = 0.6f;
        List<Transform> transList=new List<Transform>();
        for (int i = 0; i < _targetCPUParents.Count(); i++)
        {
            if (_targetCPUParents[i].gameObject.activeSelf) transList.Add(_targetCPUParents[i]);
        }
        _activeTargetCPUParents = transList.ToArray();

        _childIndex = new int[_activeTargetCPUParents.Count()];
        _targetTrans = new Transform[_activeTargetCPUParents.Count()];
        _targetCollider = new Collider[_activeTargetCPUParents.Count()];
        _hitPlayerPosition = new Vector3[_activeTargetCPUParents.Count()];
        _hitCpuPosition = new Vector3[_activeTargetCPUParents.Count()];
        _playerCollider = CarController.MeshCollider;
        Finished = false;
        List<string> strList = new List<string>();
        for (int i = 0; i < _activeTargetCPUParents.Count(); i++)
        {
            _targetTrans[i]=_activeTargetCPUParents[i].GetChild(_childIndex[i] = 0);
            TargetColliderSetting(i);
            strList.AddRange(
                new string[]{
                    _activeTargetCPUParents[i].name,
                    "dX",
                    "dZ",
                    "Distance"});
        }
        string[] s = base._startStrArray().Concat(strList.ToArray()).ToArray();
        return s;
    }
    public override string[] _updateStrArray()
    {
        var directions = new Vector3[_activeTargetCPUParents.Count()];
        for (int i = 0; i < _activeTargetCPUParents.Count(); i++)
        {
            NearestTarget(i);
            ClosestPointing(i);
            directions[i] = _hitCpuPosition[i] - _hitPlayerPosition[i];
        }

        HitCpuPosition = _hitCpuPosition;
        HitPlayerPosition = _hitPlayerPosition;
        List<string> strList = new List<string>();
        for (int i = 0; i < _activeTargetCPUParents.Count(); i++)
        {
            strList.AddRange(
                new string[]{
                    string.Empty,
                    directions[i].x.ToString(_format),
                    directions[i].z.ToString(_format),
                    directions[i].magnitude.ToString(_format)
                }
            );
        };
        string[] s = base._updateStrArray().Concat(strList.ToArray()).ToArray();

        return s;
    }
    private void TargetColliderSetting(int parentTransIndex)
    {
        if (_activeTargetCPUParents[parentTransIndex].gameObject.activeSelf == false) return;
        Vector3 direction = _targetTrans[parentTransIndex].position - _carTrans.position;
        RaycastHit hit;
        Vector3 origin = new Vector3(_carTrans.position.x, _rayPositionY, _carTrans.position.z);
        if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity))
        {
            _targetCollider[parentTransIndex] = hit.collider;
        }
    }
    private void NearestTarget(int parentTransIndex)
    {
        if (_childIndex[parentTransIndex] == _scooterCount && _activeTargetCPUParents[parentTransIndex].name=="Moving Bicycles")
        {
            Finished = true;
            return;
        }
        var oldDirection = _targetTrans[parentTransIndex].position - _carTrans.position;
        if (oldDirection.z > 0) return;
        _childIndex[parentTransIndex]++;
        _targetTrans[parentTransIndex] = _activeTargetCPUParents[parentTransIndex].GetChild(_childIndex[parentTransIndex]);

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
[System.Serializable]
public class ModelTypeController
{
    public enum ModelType
    {
        Normal,
        NormalFrame,
        NormalMask,
        NormalFrameMask,
        Wheel,
        WheelFrame,
        WheelMask,
        WheelMaskFrame
    }
    [SerializeField] ModelType _modelType;
    public ModelType Type { get => _modelType; }
    List<GameObject> frontPartsList = new List<GameObject>();
    [SerializeField] GameObject frameObjects;
    [SerializeField] GameObject maskObjects;
    public void ChangeType(CarController _playerCar)
    {
        var partsTf = _playerCar.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < partsTf.Length; i++)
        {
            if (partsTf[i].gameObject.CompareTag("FrontParts"))
            {
                frontPartsList.Add(partsTf[i].gameObject);
            }
        }

        switch (_modelType)
        {
            case ModelType.Normal:
                ActiveControll(true, false, false);
                break;
            case ModelType.NormalFrame:
                ActiveControll(true, true, false);
                break;
            case ModelType.NormalMask:
                ActiveControll(true, false, true);
                break;
            case ModelType.NormalFrameMask:
                ActiveControll(true, true, true);
                break;
            case ModelType.WheelMaskFrame:
                ActiveControll(false, true, true);
                break;
            case ModelType.Wheel:
                ActiveControll(false, false, false);
                break;
            case ModelType.WheelFrame:
                ActiveControll(false, true, false);
                break;
            case ModelType.WheelMask:
                ActiveControll(false, false, true);
                break;
        }
    }
    void ActiveControll(bool isFrontPartsActive, bool isFrameActive, bool isMaskActive)
    {
        foreach (GameObject obj in frontPartsList)
        {
            if (obj) obj.SetActive(isFrontPartsActive);
        }
        if (frameObjects) frameObjects.SetActive(isFrameActive);
        if (maskObjects) maskObjects.SetActive(isMaskActive);
    }
}
[System.Serializable]
public class CanvasController
{
    [SerializeField] GameObject _hitAlertPanel;
    [SerializeField] GameObject _endPanel;
    [SerializeField] TextMeshProUGUI _hitText;
    public void HitAlertPanelControll(CarController _playerCar)
    {
        if (!_hitAlertPanel) return;
        if (_playerCar.CollisionInfo != null)
        {
            _hitText.text = _playerCar.CollisionInfo.gameObject.name;
            _hitAlertPanel.SetActive(true);
            CoroutineHandler.StartStaticCoroutine(SwitchPanelActiveself(_hitAlertPanel));
        }
    }
    public void EndPanelControll(bool isFinished)
    {
        _endPanel.SetActive(isFinished);
    }
    IEnumerator SwitchPanelActiveself(GameObject panel)
    {
        yield return new WaitForSeconds(2f);
        panel.SetActive(false);
    }
}
public class MainSceneDirector : MonoBehaviour
{
    private CarController _playerCar { get => FindAnyObjectByType<CarController>(); }
    [SerializeField] private FrameRateController _frameRateController;
    [SerializeField] private ExportDistance _exportDistance;
    [SerializeField] private CanvasController _canvasController;
    public ModelTypeController _modelTypeController;
    void Start()
    {
        int level = QualitySettings.GetQualityLevel();
        Debug.Log($"Level:[{level}] name:[{QualitySettings.names[level]}]");
        Application.targetFrameRate = _frameRateController._frameRate;

        if (_exportDistance._isExportCsvfile)
        {
            _exportDistance.StreamWriterStart();
        }
    }
#if UNITY_EDITOR
    //警告除去
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += _OnValidate;
    }
    void _OnValidate()
    {
        UnityEditor.EditorApplication.delayCall -= _OnValidate;
        if (this == null) return;
        _modelTypeController.ChangeType(_playerCar);
    }
#endif
    void Update()
    {
        if (_exportDistance._isExportCsvfile)
        {
            _exportDistance.SaveData(_exportDistance._updateStrArray());
        }
        _canvasController.HitAlertPanelControll(_playerCar);
        _canvasController.EndPanelControll(_exportDistance.Finished);
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

