using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentController : Agent
{
    [SerializeField] private List<GameObject> spawnedObjectsList = new List<GameObject>();
    public GameObject shovel;
    public GameObject marker;
    private bool hasShovel = false;
    public float minDistance = 2.83f;

    [SerializeField] private float moveSpeed = 4f;
    private Rigidbody rb;

    [SerializeField] private Transform environmentLocation;
    [SerializeField] private Transform agentLocation;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));
        CreateObjects();
    }

    private void CreateObjects()
    {
        if (spawnedObjectsList.Count != 0)
        {
            RemoveObjects(spawnedObjectsList);
        }

        GameObject newShovel = Instantiate(shovel);
        newShovel.transform.parent = environmentLocation;
        Vector3 shovelLocation = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));
        newShovel.transform.localPosition = shovelLocation;
        spawnedObjectsList.Add(newShovel);

        GameObject newMarker = Instantiate(marker);
        newMarker.transform.parent = environmentLocation;
        Vector3 markerLocation = new Vector3(Random.Range(-4f, 4f), -0.1f, Random.Range(-4f, 4f));

        while (!CheckOverlap(shovelLocation, markerLocation, minDistance))
        {
            markerLocation = new Vector3(Random.Range(-4f, 4f), -0.1f, Random.Range(-4f, 4f));
        }

        newMarker.transform.localPosition = markerLocation;
        spawnedObjectsList.Add(newMarker);
    }

    private bool CheckOverlap(Vector3 item1Location, Vector3 item2Location, float minDistance)
    { 
        float currentDistance = Vector3.Distance(item1Location, item2Location);
        if (minDistance <= currentDistance) 
        {
            return true;
        }
        return false;
    }

    public void RemoveObjects(List<GameObject> deleteObjectsList)
    {
        foreach (GameObject i in deleteObjectsList)
        {
            Destroy(i.gameObject);
        }
        deleteObjectsList.Clear();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(hasShovel);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        /*if (other.gameObject.tag == "Shovel") 
            {
            spawnedObjectsList.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(10f);
            if (spawnedObjectsList.Count == 0)
            {
                RemoveObjects(spawnedPelletsList);
                AddReward(5f);
                EndEpisode();
            }
        }*/

        if (other.gameObject.tag == "Shovel")
        {
            other.gameObject.transform.parent = agentLocation;
            Vector3 newShovelLocation = new Vector3(-0.65f, 0f, 0.5f);
            other.gameObject.transform.localPosition = newShovelLocation;
            AddReward(10f);
            hasShovel = true;
        }

        if (other.gameObject.tag == "TreasureMarker")
        {
            if (hasShovel)
            {
                RemoveObjects(spawnedObjectsList);
                AddReward(15f);
                hasShovel = false;
                EndEpisode();
            }
        }

        if (other.gameObject.tag == "Wall")
        {
            RemoveObjects(spawnedObjectsList);
            AddReward(-8f);
            hasShovel = false;
            EndEpisode();
        }
    }
}
