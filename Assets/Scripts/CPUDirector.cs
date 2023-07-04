using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AgentAvoidancePriority
{
    [SerializeField] private int _minAgentPriority = 40;
    [SerializeField] private int _maxAgentPriority = 60;
    [SerializeField] private int _lookedAgentPirority = 30;
    [SerializeField] private int _bikeAgentPriority = 70;
    public int MinAgentPriority { get => _minAgentPriority; }
    public int MaxAgentPriority { get => _maxAgentPriority; }
    public int LookedAgentPriority { get => _lookedAgentPirority; }
    public int BikeAgentPriority { get => _bikeAgentPriority; }
}
public class CPUDirector : MonoBehaviour
{
    [SerializeField] private AgentAvoidancePriority _priority;
    [SerializeField] float _navmeshAreaBorder;
    [SerializeField] float _crossInterval = 36;
    public float NavMeshAreaBorder { get => _navmeshAreaBorder; }
    public float CrossInterval { get => _crossInterval; }
    public AgentAvoidancePriority Priority { get => _priority; }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 drawPosition = new Vector3(NavMeshAreaBorder, 0, 0);
        Gizmos.DrawLine(drawPosition - Vector3.forward * 10, drawPosition + Vector3.forward * 10000);
    }
}
