using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;


public class DoorSensorPublisher : MonoBehaviour
{
    public string topicName = "smartthings_sensors_door";
    [SerializeField] Transform sensor_1;
    [SerializeField] Transform sensor_2;
    ROSConnection ros;

    // Publish the cube's position and rotation every N seconds
    public float publishMessageFrequency = 0.5f;
    private float timeElapsed;

    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<BoolMsg>(topicName);
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            Vector3 tmp = sensor_1.position - sensor_2.position;
            BoolMsg cubePos = new BoolMsg();
            cubePos.data = tmp.magnitude > .05;
            
            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, cubePos);

            timeElapsed = 0;
        }
    }
}
