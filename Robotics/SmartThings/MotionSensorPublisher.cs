using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;


public class MotionSensorPublisher : MonoBehaviour
{
    public string topicName = "smartthings_sensors_motion_door";
    [SerializeField] Transform sensor;
    [SerializeField] Transform motion_object;
    [SerializeField] float distance;
    ROSConnection ros;

    // Publish the cube's position and rotation every N secondsffffff
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
            Vector3 tmp = sensor.position - motion_object.position;
            BoolMsg cubePos = new BoolMsg();
            cubePos.data = tmp.magnitude < distance;
            
            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, cubePos);

            timeElapsed = 0;
        }
    }
}
