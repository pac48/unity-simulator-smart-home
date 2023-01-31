using System;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class RosDepthPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "unity_camera/depth/image_raw";
    public string cameraInfoTopicName = "unity_camera/depth/camera_info";

    // The game object
    public Camera ImageCamera;
    public string FrameId = "unity_camera/depth_frame";
    public int resolutionWidth = 640;
    public int resolutionHeight = 480;
    public float publishMessageFrequency = 1.0f/20.0f;
    private float timeElapsed;
    private Texture2D texture2D;
    private Rect rect;
    private byte[] data;
    private RenderTexture finalRT;

    [Header("Shader Setup")] public Shader uberReplacementShader;

    static private void SetupCameraWithReplacementShader(Camera cam, Shader shader, Color clearColor)
    {
        // modes
        // ObjectId = 0,
        // CatergoryId = 1,
        // DepthCompressed = 2,
        // DepthMultichannel = 3,
        // Normals = 4
        int mode = 2;
        var cb = new CommandBuffer();
        cb.SetGlobalFloat("_OutputMode", mode); 
        cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cb);
        cam.AddCommandBuffer(CameraEvent.BeforeFinalPass, cb);
        cam.SetReplacementShader(shader, "");
        cam.backgroundColor = clearColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.allowHDR = false;
        cam.allowMSAA = false;
    }

    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);
        ros.RegisterPublisher<CameraInfoMsg>(cameraInfoTopicName);
        texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RFloat, false);
        data = new byte[resolutionWidth * resolutionHeight * 4];
        rect = new Rect(0, 0, resolutionWidth, resolutionHeight);

        // default fallbacks, if shaders are unspecified
        if (!uberReplacementShader)
            uberReplacementShader = Shader.Find("Hidden/UberReplacement");
        //set up camera shader
        SetupCameraWithReplacementShader(ImageCamera, uberReplacementShader, Color.white);

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


    private IEnumerator WaitForEndOfFrameAndSave()
    {
        yield return new WaitForEndOfFrame();
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

            string encoding = "32FC1";
            byte is_bigendian = 0;
            uint step = 4 * width;
            byte[] data_local = texture2D.GetRawTextureData();
            uint ind = 0;
            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    long ind2 = (height - y - 1) * width + x;
                    data[ind] = data_local[4 * ind2];
                    data[ind + 1] = data_local[4 * ind2 + 1];
                    data[ind + 2] = data_local[4 * ind2 + 2];
                    data[ind + 3] = data_local[4 * ind2 + 3];
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