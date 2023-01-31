using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;


public class TransformData
{
    public double posX;
    public double posY;
    public double posZ;
    public double quatW;
    public double quatX;
    public double quatY;
    public double quatZ;
    public int sec;
    public uint nanosec;
    public string frameID { get; set; }
}

public class TFBridge : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "unity_tf_bridge";
    public float publishMessageFrequency = 1.0f/20.0f;    
    
    public List<Transform> transforms;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    
    // Start is called before the first frame update
    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<StringMsg>(topicName, 1);
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            // Update ROS Message
            double timeStamp = Time.timeAsDouble + .2;
            int sec = (int) Math.Truncate(timeStamp);
            uint nanosec = (uint)( (timeStamp - sec)*1e+9 );
            
            StringMsg msg = new StringMsg();
            List<TransformData> transformDataList = new List<TransformData>(); 
            foreach (var transform in transforms)
            {
                TransformData transformData = new TransformData();
                transformData.frameID = transform.name;
                transformData.posX = transform.position.x;
                transformData.posY = transform.position.y;
                transformData.posZ = transform.position.z;
                transformData.quatW = transform.rotation.w;
                transformData.quatX = transform.rotation.x;
                transformData.quatY = transform.rotation.y;
                transformData.quatZ = transform.rotation.z;
                transformData.sec = sec;
                transformData.nanosec = nanosec;

                transformDataList.Add(transformData);
            }
            
            msg.data = JsonConvert.SerializeObject(transformDataList);
            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, msg);

            timeElapsed = 0;
        }
       
    }
}