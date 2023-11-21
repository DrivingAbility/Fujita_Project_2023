using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PlayerCameraController : MonoBehaviour
{
    [SerializeField]Camera _fCamera;
    [SerializeField]Camera _rCamera;
    [SerializeField]Camera _lCamera;
    [SerializeField]float _horizontalFov=90.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!Application.isPlaying){
            float fov=CalcVerticalFOV(_horizontalFov,_fCamera.aspect);
            _fCamera.fieldOfView=_rCamera.fieldOfView=_lCamera.fieldOfView=fov;
            
            Vector3 angle=_rCamera.transform.localEulerAngles;
            angle.y=_horizontalFov;
            _rCamera.transform.localEulerAngles=angle;

            angle=_lCamera.transform.localEulerAngles;
            angle.y=-_horizontalFov;
            _lCamera.transform.localEulerAngles=angle;
        }
    }
    private float CalcVerticalFOV(float horizontalFOV, float aspect) {
        return Mathf.Atan(Mathf.Tan(horizontalFOV / 2f * Mathf.Deg2Rad) / aspect) * 2f * Mathf.Rad2Deg;
    }
}
