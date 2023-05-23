using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Car))]
[CanEditMultipleObjects]
class CarEditorGUI : Editor
{
    private SerializedProperty carColorType;
    private SerializedProperty carColorProperty;
    private SerializedProperty carNumberType;

    private SerializedProperty wheelRotateMode;
    private SerializedProperty wheelRotateSpeed;

    private SerializedProperty maxWheelRotateAngle;
    private SerializedProperty wheelRotateValue;

    private SerializedProperty changeMeshMode;
    private SerializedProperty changeMeshRotateSpeed;
    private SerializedProperty moveSpeed;

    private SerializedProperty enableLight;
    private SerializedProperty lightEmissionEnable;
    private SerializedProperty lightIntensity;

    private SerializedProperty carLightRangeValue;
    private SerializedProperty carLightSpotAngle;
    private SerializedProperty carLightIntensity;
    private SerializedProperty carLightShadowType;

    private GUIStyle labelsGUIStyle;
    private void OnEnable()
    {
        carColorProperty = serializedObject.FindProperty("carColorSelected");
        carColorType = serializedObject.FindProperty("carColorType");
        carNumberType = serializedObject.FindProperty("selectedCarNumberType");

        wheelRotateMode = serializedObject.FindProperty("wheelRotateMode");
        wheelRotateSpeed = serializedObject.FindProperty("wheelConstantRotateSpeed");
        maxWheelRotateAngle = serializedObject.FindProperty("maxWheelRotateAngle");
        wheelRotateValue = serializedObject.FindProperty("wheelRotateValue");

        changeMeshMode = serializedObject.FindProperty("changeWheelMeshMode");
        changeMeshRotateSpeed = serializedObject.FindProperty("changeMaterialRotateSpeed");

        moveSpeed = serializedObject.FindProperty("moveSpeed");

        enableLight = serializedObject.FindProperty("enableLight");
        lightEmissionEnable = serializedObject.FindProperty("enableLightEmission");
        lightIntensity = serializedObject.FindProperty("emissionIntensity");

        carLightRangeValue = serializedObject.FindProperty("carLightRange");
        carLightSpotAngle = serializedObject.FindProperty("carLightSpotAngle");
        carLightIntensity = serializedObject.FindProperty("carLightIntensity");
        carLightShadowType = serializedObject.FindProperty("carLightShadowType");
        labelsGUIStyle = new GUIStyle();
        labelsGUIStyle.fontStyle = FontStyle.Bold;
    }

    private int conter = 0;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginVertical("Button");
        GUILayout.Label("Car settings:", labelsGUIStyle);

        EditorGUILayout.BeginVertical("Button");
        GUILayout.Label("Color:", labelsGUIStyle);
        carColorType.intValue = (int)(Car.CarColorType)EditorGUILayout.EnumPopup("Car color type:", (Car.CarColorType)carColorType.intValue);

        if (carColorType.intValue == 0)
        {
            EditorGUILayout.HelpBox("The color will be assigned randomly when placing the car in the scene and each time the application starts", MessageType.Info);
        }
        if (carColorType.intValue == 1)
        {
            EditorGUILayout.HelpBox("Color is selected manually and will not change", MessageType.Info);
            carColorProperty.intValue = (int)(Car.CarColor)EditorGUILayout.EnumPopup("Car color:", (Car.CarColor)carColorProperty.intValue);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical("Button");
        GUILayout.Label(" License plate region:", labelsGUIStyle);
        carNumberType.intValue = (int)(Car.CarNumberType)EditorGUILayout.EnumPopup("License Plate Region Selection:", (Car.CarNumberType)carNumberType.intValue);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("Button");
        GUILayout.Label("Headlight:", labelsGUIStyle);

        enableLight.boolValue = EditorGUILayout.Toggle(new GUIContent("Enable light"), enableLight.boolValue);
        EditorGUILayout.HelpBox("When you turn on the headlights, the light source control parameters appear simulating the headlights", MessageType.Info);
        if (enableLight.boolValue)
        {
            carLightRangeValue.floatValue = EditorGUILayout.FloatField(new GUIContent("Light range"), carLightRangeValue.floatValue);
            carLightSpotAngle.floatValue = EditorGUILayout.Slider(new GUIContent("Spot angle"), carLightSpotAngle.floatValue, 1f, 179f);
            carLightIntensity.floatValue = EditorGUILayout.FloatField(new GUIContent("Light intensity"), carLightIntensity.floatValue);
            carLightShadowType.intValue = (int)(LightShadows)EditorGUILayout.EnumPopup("Shadow cast mode:", (LightShadows)carLightShadowType.intValue);
            lightEmissionEnable.boolValue = EditorGUILayout.Toggle(new GUIContent("Light emission"), lightEmissionEnable.boolValue);
            if (lightEmissionEnable.boolValue)
            {
                lightIntensity.floatValue = EditorGUILayout.Slider(new GUIContent("Light intensity"), lightIntensity.floatValue, 1f, 2f);
            }
            EditorGUILayout.HelpBox("Headlights can be controlled manually or from the animation.\nEnableLight/DisableLight events for managing an external script is also available.", MessageType.Info);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("Button");
        GUILayout.Label("Runtime parameters will work only in play mode:", labelsGUIStyle);

        EditorGUILayout.BeginVertical("Button");
        GUILayout.Label("Wheels turn:", labelsGUIStyle);

        maxWheelRotateAngle.floatValue = EditorGUILayout.Slider(new GUIContent("Turn angle limitation"), maxWheelRotateAngle.floatValue, 0f, 90f);
        wheelRotateValue.floatValue = EditorGUILayout.Slider(new GUIContent("Turn wheels value"), wheelRotateValue.floatValue, -1f, 1f);
        EditorGUILayout.HelpBox("Turn wheels value can be controlled manually or from the animation.\nWheelRotateValue property for managing an external script is available.", MessageType.Info);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("Button");
        GUILayout.Label("Wheels rotation:", labelsGUIStyle);

        EditorGUILayout.LabelField("Current speed:", moveSpeed.floatValue.ToString("f2") + " km/h");

        wheelRotateMode.intValue = (int)(Car.WheelRotateMode)EditorGUILayout.EnumPopup("Wheel rotate mode", (Car.WheelRotateMode)wheelRotateMode.intValue);
        if (wheelRotateMode.intValue == 0)
        {
            EditorGUILayout.HelpBox("No rotation - used if the car will not move in the scene (parked)", MessageType.Info);
        }
        if (wheelRotateMode.intValue == 1)
        {
            EditorGUILayout.HelpBox("Auto Detect Movement - includes tracking the movement of the car and automatically rotates the wheels at the right speed", MessageType.Info);
        }
        else if (wheelRotateMode.intValue == 2)
        {
            EditorGUILayout.HelpBox("Constant Speed - no tracking the movement of the car and automatically rotate the wheels at the constant speed", MessageType.Info);
            wheelRotateSpeed.floatValue = EditorGUILayout.FloatField(new GUIContent("Wheel rotate speed"), wheelRotateSpeed.floatValue);
            EditorGUILayout.HelpBox("Constant wheel rotating speed, km/h", MessageType.Info);
        }

        if (wheelRotateMode.intValue != 0)
        {
            changeMeshMode.intValue = (int)(Car.ChangeWheelMesh)EditorGUILayout.EnumPopup("Turn on blurring wheels during rotation:", (Car.ChangeWheelMesh)changeMeshMode.intValue);
            if (changeMeshMode.intValue == 1)
            {
                changeMeshRotateSpeed.floatValue = EditorGUILayout.FloatField(new GUIContent("Speed threshold km/h"), changeMeshRotateSpeed.floatValue);
                EditorGUILayout.HelpBox("Value of speed in kilometers, after which blur will turn on", MessageType.Info);
                if (changeMeshRotateSpeed.floatValue <= 0)
                {
                    EditorGUILayout.HelpBox("Value must be more then 0", MessageType.Error);
                }
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
        this.Repaint();
        serializedObject.ApplyModifiedProperties();
    }
}