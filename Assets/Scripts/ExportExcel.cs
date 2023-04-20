using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class ExportExcel : MonoBehaviour
{
    private StreamWriter sw;
    CarController carController;
    Vector3 carPos;
    [NonSerialized] public static bool isExportExcel;
    int fileNum = 0;
    bool isFileFound = true;
    float sdkMax = 32767f;
    float bfSteering = 0, bfAccel = 0, bfBrake = 0, time = 0;
    float thetaM;
    // Start is called before the first frame update
    void Start()
    {
        if (!isExportExcel) return;
        carController = FindObjectOfType<CarController>();
        string datetimeStr = DateTime.Now.ToString("yyyy_MM_dd");
        string path = Application.dataPath + @"\Excel\" + datetimeStr + @"\";
        Directory.CreateDirectory(path);
        path += @"\" + "SaveData.csv";

        string path2 = path;
        while (isFileFound)
        {
            if (System.IO.File.Exists(path2))
            {
                fileNum++;
                path2 = path;
                path2 = path.Replace(".csv", fileNum.ToString() + ".csv");
            }
            else
            {
                isFileFound = false;
                path = path2;
            }
        }
        sw = new StreamWriter(path, true, Encoding.GetEncoding("Shift_JIS"));

        string[] s1 = new string[] { DateTime.Now.ToLongDateString(), "Time", "ΔTime", "Xpos", "Zpos", "Velocity", "RotateY", "Hit" };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);
        if ((LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0)))
        {
            s1 = s1.Concat(new string[] { "θ" }).ToArray();
            s1 = s1.Concat(new string[] { "Δθ" }).ToArray();
            s1 = s1.Concat(new string[] { "ΣθΔT" }).ToArray();
            s1 = s1.Concat(new string[] { "Accel" }).ToArray();
            s1 = s1.Concat(new string[] { "ΔAccel" }).ToArray();
            s1 = s1.Concat(new string[] { "Brake" }).ToArray();
            s1 = s1.Concat(new string[] { "ΔBrake" }).ToArray();
        }
        s2 = string.Join(",", s1);
        sw.WriteLine(s2);
    }
    public void SaveData()
    {
        if (!isExportExcel) return;
        LogitechGSDK.DIJOYSTATE2ENGINES rec;
        rec = LogitechGSDK.LogiGetStateUnity(0);

        carPos = carController.transform.position;
        if (carPos.z < 0.01f) return;
        time += Time.deltaTime;
        float rotateY = carController.transform.rotation.eulerAngles.y;
        rotateY = (rotateY > 180) ? rotateY - 360 : rotateY;

        string[] s1 = {
            DateTime.Now.ToLongTimeString(),
            time.ToString("f4"),
            Time.deltaTime.ToString("f4"),
            carPos.x.ToString("f4"),
            carPos.z.ToString("f4"),
            (carController.GetComponent<Rigidbody>().velocity.magnitude*3600/1000).ToString("f4"),
            rotateY.ToString("f4")
        };

        if ((LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0)))
        {
            float steering = rec.lX / sdkMax * 450;
            s1 = s1.Concat(new string[] { (steering).ToString("f4") }).ToArray();
            s1 = s1.Concat(new string[] { ((steering - bfSteering) / Time.deltaTime).ToString("f4") }).ToArray();

            thetaM += steering * Time.deltaTime;
            s1 = s1.Concat(new string[] { (thetaM).ToString("f4") }).ToArray();

            float accel = (-1 * rec.lY / sdkMax + 1) / 2;
            s1 = s1.Concat(new string[] { (accel).ToString("f4") }).ToArray();
            s1 = s1.Concat(new string[] { ((accel - bfAccel) / Time.deltaTime).ToString("f4") }).ToArray();

            float brake = (-1 * rec.lRz / sdkMax + 1) / 2;
            s1 = s1.Concat(new string[] { (brake).ToString("f4") }).ToArray();
            s1 = s1.Concat(new string[] { ((brake - bfBrake) / Time.deltaTime).ToString("f4") }).ToArray();

            bfSteering = steering;
            bfAccel = accel;
            bfBrake = brake;
        }
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SaveData();
    }
    private void OnApplicationQuit()
    {
        if (!isExportExcel) return;
        sw.Close();
    }

}
