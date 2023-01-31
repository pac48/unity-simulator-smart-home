using System;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class RosImagePublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "unity_camera/color/image_raw";
    public string cameraInfoTopicName = "unity_camera/rgb/camera_info";

    // The game object
    public Camera ImageCamera;
    public string FrameId = "unity_camera/color_frame";
    public int resolutionWidth = 640;
    public int resolutionHeight = 480;
    public float publishMessageFrequency = 1.0f/20.0f;    
    private float timeElapsed;
    private Texture2D texture2D;
    private Rect rect;
    private byte[] data;
    private RenderTexture finalRT;

    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);
        ros.RegisterPublisher<CameraInfoMsg>(cameraInfoTopicName);
        texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGBA32, false);
        data = new byte[resolutionWidth * resolutionHeight * 4];
        rect = new Rect(0, 0, resolutionWidth, resolutionHeight);

        bool supportsAntialiasing = true;
        bool needsRescale = false;
        var depth = 32;
        var format = RenderTextureFormat.Default;
        var readWrite = RenderTextureReadWrite.Default;
        var antiAliasing = (supportsAntialiasing) ? Mathf.Max(1, QualitySettings.antiAliasing) : 1;

        finalRT = RenderTexture.GetTemporary(resolutionWidth, resolutionHeight, depth, format, readWrite, antiAliasing);
    }


    void Update()
    {
        if (texture2D != null)
            UpdateMessage();
    }

    private void UpdateMessage()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            // render to offscreen texture (readonly from CPU side)
            RenderTexture.active = finalRT;
            ImageCamera.targetTexture = finalRT;
            //
            ImageCamera.Render();
            texture2D.ReadPixels(rect, 0, 0);
            
            HeaderMsg header = new HeaderMsg();
            double timeStamp = Time.timeAsDouble;
            int sec = (int) Math.Truncate(timeStamp);
            uint nanosec = (uint)( (timeStamp - sec)*1e+9 );
            header.stamp.sec = sec;
            header.stamp.nanosec = nanosec;
            header.frame_id = FrameId;
            uint height = (uint)texture2D.height;
            uint width = (uint)texture2D.width;
            if (data.Length != height * width * 4)
            {
                data = new byte[height * width * 4];
            }

            string encoding = "rgba8";
            byte is_bigendian = 0;
            uint step = 4 * width;
            Color32[] colors = texture2D.GetPixels32();
            uint ind = 0;
            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    long ind2 = (height - y - 1) * width + x;
                    data[ind] = colors[ind2].r;
                    data[ind + 1] = colors[ind2].g;
                    data[ind + 2] = colors[ind2].b;
                    data[ind + 3] = colors[ind2].a;
                    ind += 4;
                }
            }
            ImageMsg imageMsg = new ImageMsg(header, height, width, encoding, is_bigendian, step, data);
            ros.Publish(topicName, imageMsg);

            // Camera Info message
            CameraInfoMsg cameraInfoMessage =
                CameraInfoGenerator.ConstructCameraInfoMessage(ImageCamera, header, 0.0f, 0.01f);
            ros.Publish(cameraInfoTopicName, cameraInfoMessage);

            timeElapsed = 0;
        }
    }
}