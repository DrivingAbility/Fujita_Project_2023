using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class Car : MonoBehaviour
{
    [SerializeField]
    private string carBodyMaterialName = "m_body"; //car material name
    [SerializeField]
    private string carLightMaterialName = "m_reflector"; //light material name
    
    [SerializeField]
    private List<CarTexture> carMatList = new List<CarTexture>(); //different colors textures
    
    [Serializable]
    private class CarTexture
    {
        public Texture albedoTexture;
    }
    
    [SerializeField]
    private CarColorType carColorType; //type of car color
    
    public enum CarColorType
    {
        RandomlyAssigned,
        AssignedManually
    }
    
    [SerializeField]
    private CarNumberType selectedCarNumberType; //selected car number type
    
    public enum CarNumberType
    {
        None = 0,
        US = 1,
        EU = 2,
        CN = 3
    } 
    [SerializeField] 
    private List<CarNumber> carNumbersObjects = new List<CarNumber>();

    [Serializable]
    private class CarNumber
    {
        public CarNumberType carNumberType;
        public GameObject numberObject;
    }

    [SerializeField]
    private CarColor carColorSelected; //selected car color
    
    public enum CarColor
    {
        Color_a = 0,
        Color_b = 1,
        Color_c = 2,
        Color_d = 3,
        Color_e = 4
    }
    
    [SerializeField] 
    private WheelRotateMode wheelRotateMode; //rotate mode for wheels
    
    public enum WheelRotateMode
    {
        NoRotate,
        AutoDetectMovement,
        ConstantSpeed
    }

    [SerializeField] 
    private float wheelConstantRotateSpeed = 150f; //if we are using ConstantSpeed mode - we are using current value for wheel 

    [SerializeField] 
    private ChangeWheelMesh changeWheelMeshMode;  //change wheel to blurred wheel or not
    
    public enum ChangeWheelMesh
    {
        DontActivate,
        SpeedMoreThen
    }
    
    [SerializeField] 
    private float changeMaterialRotateSpeed = 100f; //when speed > then changeMaterialRotateSpeed we are change model of wheel

    [SerializeField] 
    private float wheelSpeedTokmH = 41f; //wheel speed to km/h speed
    
    [SerializeField] 
    private List<CarWheel> wheelObject = new List<CarWheel>(); //car wheel object
    
    [Serializable]
    private class CarWheel
    {
        public Transform wheelSocket;
        public GameObject wheelObject;
        public GameObject wheelBluredObject;
        [HideInInspector]
        public Material wheelBluredMaterial;
        [HideInInspector] 
        public float startWheelAngle = 0f;
        public bool FL = false;
        public bool FR = false;
        public bool RL = false;
        public bool RR = false;
    }

    private bool wheelsBlurred = false;
    
    [SerializeField] 
    private float maxWheelRotateAngle = 20f; //max angle for rotate front wheels
    [SerializeField] 
    private float wheelRotateValue = 0f;//front wheel rotate 
    
    public float WheelRotateValue //public variable for set wheels rotation
    {
        set
        {
            wheelRotateValue = value;
        }
    }
    
    [SerializeField] 
    private bool enableLight = false; //enable light property
    
    //Lights properties
    [SerializeField] 
    private List<Light> carLights;
    [SerializeField] 
    private float carLightRange = 10f;
    [SerializeField] 
    private float carLightSpotAngle = 115f;
    [SerializeField] 
    private float carLightIntensity = 5f;
    [SerializeField] 
    private LightShadows carLightShadowType = LightShadows.Hard;
    [SerializeField] 
    private bool enableLightEmission = false;
    [SerializeField] 
    private float emissionIntensity = 1;

    [SerializeField] 
    private float moveSpeed = 0f; //calculated car speed

    private Vector3 lastPosition = Vector3.zero; //previos frame car position
    private float animationTime = 0f; //time for fade animation
    
    private bool bloorStared = false;
    private bool bloorEnd = false;
    
    private MaterialPropertyBlock materialPropertyBody;
    private MaterialPropertyBlock materialPropertyLight;

    // Start is called before the first frame update
    void Start()
    {
        //Set car color
        if (carColorType == CarColorType.RandomlyAssigned)
        {
            int randomIndex = Random.Range(0, carMatList.Count);
            SetColorByIndex(randomIndex);
        }
        else if (carColorType == CarColorType.AssignedManually)
        {
            SetColorByIndex((int)carColorSelected);
        }
        
        lastPosition = transform.position;
        
        if (enableLight)
        {
            EnableLight();
        }
        else
        {
            DisableLight();
        }

        SetCarNumberType();
        
        //Init wheels
        foreach (CarWheel wheel in wheelObject)
        {
            if (!wheel.wheelSocket)
            {
                Debug.Log("No wheel socket!");
            }
            
            if (wheel.wheelObject)
            {
                wheel.wheelObject.SetActive(true);
            }
            else
            {
                Debug.Log("No wheel object");
            }

            if (wheel.wheelBluredObject)
            {
                wheel.wheelBluredObject.SetActive(false);
                if (wheel.wheelBluredObject.GetComponent<Renderer>())
                {
                    if (wheel.wheelBluredObject.GetComponent<Renderer>().sharedMaterial)
                    {
                        wheel.wheelBluredMaterial = wheel.wheelBluredObject.GetComponent<Renderer>().sharedMaterial;
                    } 
                    else
                    {
                        Debug.Log("No material on the blured object");
                    }
                }
                else
                {
                    Debug.Log("No renderer component on the blured object");
                }
            } 
            else
            {
                Debug.Log("No blured wheel object");
            }

            if (moveSpeed >= changeMaterialRotateSpeed)
            {
                if (wheel.wheelObject)
                {
                    wheel.wheelObject.SetActive(false);
                }

                if (wheel.wheelBluredObject)
                {
                    wheel.wheelBluredObject.SetActive(true);
                }
                animationTime = 1f;
                bloorStared = false;
                bloorEnd = false;
            }
            else
            {
                animationTime = 0f;
            }

            if (wheel.FL || wheel.FR)
            {
                wheel.startWheelAngle = wheel.wheelSocket.eulerAngles.y;
            }
        }
       
    }

    private void SetColorByIndex(int index = 0)
    {
        if (materialPropertyBody == null)
        {
            materialPropertyBody = new MaterialPropertyBlock();
        }
        if (carMatList.Count > 0)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    Material mat = renderer.sharedMaterials[i];
                    if (mat.name == carBodyMaterialName || mat.name == carBodyMaterialName + " (Instance)")
                    {
                        if (carMatList[index].albedoTexture != null)
                        {
                            materialPropertyBody.Clear();
                            materialPropertyBody.SetTexture("_MainTex", carMatList[index].albedoTexture);
                            renderer.SetPropertyBlock(materialPropertyBody, i);
                        }
                    }
                }
            }
        }
    }

    private void SetCarNumberType()
    {
        foreach (CarNumber carNumber in carNumbersObjects)
        {
            if (carNumber.numberObject)
            {
                carNumber.numberObject.SetActive(carNumber.carNumberType == selectedCarNumberType);
            }
        }    
    }
    
    //public method for enable light
    public void EnableLight()
    {
        if (materialPropertyLight == null)
        {
            materialPropertyLight = new MaterialPropertyBlock();
        } 
        foreach (Light light in carLights)
        {
            light.enabled = true;
        }
        
        if (enableLightEmission)
        {
            EnableLightEmission();
        }

        foreach (Light light in carLights)
        {
            light.range = carLightRange;
            light.spotAngle = carLightSpotAngle;
            light.intensity = carLightIntensity;
            light.shadows = carLightShadowType;
        }
    } 
    
    //public method for disable light
    public void DisableLight()
    {
        if (materialPropertyLight == null)
        {
            materialPropertyLight = new MaterialPropertyBlock();
        } 
        if (enableLightEmission)
        {
            DisableLightEmission();
        }

        foreach (Light light in carLights)
        {
            if (light)
            {
                light.enabled = false;
            }
        }
    }
    
    private void EnableLightEmission()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i].name == carLightMaterialName || renderer.sharedMaterials[i].name == carLightMaterialName + " (Instance)")
                {
                    float factor = Mathf.Pow(2,emissionIntensity);
                    Material mat = renderer.sharedMaterials[i];
                    materialPropertyLight.SetColor("_EmissionColor", new Color(mat.color.r*factor,mat.color.g*factor,mat.color.b*factor));
                    renderer.SetPropertyBlock(materialPropertyLight, i);
                }
            }
        }
    }
    
    private void DisableLightEmission()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i].name == carLightMaterialName || renderer.sharedMaterials[i].name == carLightMaterialName + " (Instance)")
                {
                    float factor = Mathf.Pow(2,emissionIntensity * -1);
                    Material mat = renderer.sharedMaterials[i];
                    materialPropertyLight.SetColor("_EmissionColor", new Color(mat.color.r*factor,mat.color.g*factor,mat.color.b*factor));
                    renderer.SetPropertyBlock(materialPropertyLight, i);
                }
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            if (carColorType == CarColorType.AssignedManually)
            {
                SetColorByIndex((int)carColorSelected);
            }
            if (enableLight)
            {
                EnableLight();
            }
            else
            {
                DisableLight();
            }

            SetCarNumberType();
        }
        else
        {
            if (wheelRotateMode != WheelRotateMode.NoRotate)
            {
                switch (wheelRotateMode)
                {
                    case WheelRotateMode.AutoDetectMovement:
                        moveSpeed = 4f * ((transform.position - lastPosition).magnitude / Time.deltaTime);
                        lastPosition = transform.position;
                       
                        break;
                    case WheelRotateMode.ConstantSpeed:
                        moveSpeed = 60f * wheelConstantRotateSpeed * Time.deltaTime;
                        break;
                }
                moveSpeed = Mathf.Round(moveSpeed * 100f) / 100f;
            }
            
            foreach (CarWheel wheel in wheelObject)
            {
                if (wheel.wheelSocket)
                {

                    if (wheel.FL)
                    {
                        float angleLeftWheel = Mathf.Clamp(wheel.wheelSocket.eulerAngles.y + (wheelRotateValue * maxWheelRotateAngle), (-1 * maxWheelRotateAngle + 270), (maxWheelRotateAngle + 270));
                        if (wheelRotateValue == 0)
                        {
                            angleLeftWheel = wheel.startWheelAngle;
                        }

                        Quaternion rot = Quaternion.Euler(new Vector3(wheel.wheelSocket.eulerAngles.x, angleLeftWheel, wheel.wheelSocket.eulerAngles.z));
                        wheel.wheelSocket.rotation = Quaternion.Slerp(wheel.wheelSocket.rotation, rot, Time.deltaTime * 3f);
                    }

                    if (wheel.FR)
                    {
                        float angleRightWheel = Mathf.Clamp(wheel.wheelSocket.eulerAngles.y + (wheelRotateValue * maxWheelRotateAngle), (-1 * maxWheelRotateAngle + 90), (maxWheelRotateAngle + 90));
                        if (wheelRotateValue == 0)
                        {
                            angleRightWheel = wheel.startWheelAngle;
                        }

                        Quaternion rot = Quaternion.Euler(new Vector3(wheel.wheelSocket.eulerAngles.x, angleRightWheel, wheel.wheelSocket.eulerAngles.z));
                        wheel.wheelSocket.rotation = Quaternion.Slerp(wheel.wheelSocket.rotation, rot, Time.deltaTime * 3f);
                    }

                    if (wheelRotateMode != WheelRotateMode.NoRotate)
                    {
                        if (wheel.wheelBluredObject && wheel.wheelBluredObject.activeSelf)
                        {
                            wheel.wheelSocket.rotation *= Quaternion.AngleAxis((moveSpeed / 3) * ((wheel.RR || wheel.FR) ? 1 : -1), new Vector3(0, 0, 1));
                        }
                        else
                        {
                            wheel.wheelSocket.rotation *= Quaternion.AngleAxis(moveSpeed * ((wheel.RR || wheel.FR) ? 1 : -1), new Vector3(0, 0, 1));
                        }
                    }
                }

                if (changeWheelMeshMode == ChangeWheelMesh.SpeedMoreThen)
                {
                    if (wheel.wheelObject && wheel.wheelBluredObject)
                    {
                        if (moveSpeed >= changeMaterialRotateSpeed || bloorStared)
                        {
                            animationTime = animationTime + (Time.deltaTime);
                            bloorStared = true;
                            if (animationTime >= 1f)
                            {
                                wheel.wheelObject.SetActive(false);
                                animationTime = 1f;
                                bloorStared = false;
                            }

                            wheel.wheelBluredObject.SetActive(true);
                        }
                        else if (moveSpeed < changeMaterialRotateSpeed || bloorEnd)
                        {
                            animationTime = animationTime - (Time.deltaTime);
                            bloorEnd = true;
                            if (animationTime <= 0f)
                            {
                                bloorEnd = false;
                                animationTime = 0f;
                                wheel.wheelBluredObject.SetActive(false);
                            }

                            wheel.wheelObject.SetActive(true);
                        }
                        else
                        {
                            wheel.wheelObject.SetActive(true);
                            wheel.wheelBluredObject.SetActive(false);
                            animationTime = 1f;
                        }
                        if (wheel.wheelBluredMaterial)
                        {
                            wheel.wheelBluredMaterial.SetColor("_Color", new Color(wheel.wheelBluredMaterial.color.r, wheel.wheelBluredMaterial.color.g, wheel.wheelBluredMaterial.color.b, animationTime));
                        }
                    }
                }
            }
        }
    }
}
