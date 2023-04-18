using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugSteering : MonoBehaviour
{
    [SerializeField] string path;
    string filepath;
    PlayerInput plInput;
    void Start()
    {
        filepath = path + @"\test.csv";
        plInput = FindObjectOfType<PlayerInput>();
    }

    void FixedUpdate()
    {
        var vt2 = plInput.currentActionMap["Horizontal"].ReadValue<Vector2>();
        using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.GetEncoding("shift_jis")))
        {
            sw.WriteLine(vt2.x.ToString() + "," + vt2.y.ToString());
        }
    }
}