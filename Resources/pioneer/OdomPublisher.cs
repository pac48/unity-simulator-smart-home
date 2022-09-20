using System;
using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

public class OdomPublisher : MonoBehaviour
{
    public Transform transform;
    ROSConnection ros;
    public string topicName = "odom";

    public string FrameId = "odom";

    // Publish the cube's position and rotation every N seconds
    public float publishMessageFrequency = 1.1f / 60.0f;

    public ArticulationBody wheelLeft;
    public ArticulationBody wheelRight;
    public float wheelLeftRadius = 0.1f;
    public float wheelRightRadius = 0.1f;
    public float wheelSeparation = 0.2f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<OdometryMsg>(topicName);
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            OdometryMsg msg = new OdometryMsg();
            msg.pose.pose.orientation.w = transform.rotation.w;
            msg.pose.pose.orientation.x = transform.rotation.x;
            msg.pose.pose.orientation.y = transform.rotation.y;
            msg.pose.pose.orientation.z = transform.rotation.z;

            msg.pose.pose.position.x = transform.position.x;
            msg.pose.pose.position.y = transform.position.y;
            msg.pose.pose.position.z = transform.position.z;

            msg.header.frame_id = FrameId;
            msg.child_frame_id = "base_link";

            msg.twist.twist.linear.x = -(wheelRight.jointVelocity[0] * wheelRightRadius + wheelLeft.jointVelocity[0] * wheelLeftRadius) / 2.0f;
            msg.twist.twist.angular.z = (wheelRight.jointVelocity[0] * wheelRightRadius - wheelLeft.jointVelocity[0] * wheelLeftRadius)/wheelSeparation;
            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, msg);

            timeElapsed = 0;
        }
    }
}