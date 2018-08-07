using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class Script_mbt : MonoBehaviour
{
    // FUNCTIONS IMPORTED FROM DLL:

    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "createCaoFile")]
    public static extern void createCaoFile(double cubeEdgeSize);

    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "InitMBT")]
    public static extern void InitMBT(double cam_px, double cam_py, double cam_u0, double cam_v0, int t);

    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "AprilTagMBT")]
    public static extern void AprilTagMBT(byte[] bitmap, int height, int width, double[] pointx, double[] pointy, double[] kltX, double[] kltY, int[] kltNumber, int t, int e, int[] flag_state, int[] nEdges);
    
    WebCamTexture wct;
    Renderer rend;
    Texture2D tex;

    //[Header("Enter your Camera Resolution here")]
    //public int X = 640;   // for example 640
    //public int Y = 480;   // for example 480

    [Header("Enter your Camera Parameters here")] //some default values provided
    public double cam_px = 1131.561907;   
    public double cam_py = 1085.157822;
    public double cam_u0 = 588.2376812;
    public double cam_v0 = 191.1328903;

    [Header("Aspect ratio (depending on resolution of camera used)")]
    public double aspect_ratio;

    double[] pointx = new double[24];
    double[] pointy = new double[24];
    double[] kltX = new double[300];
    double[] kltY = new double[300];

    int[] kltNumber = new int[1];

    public enum tracking{
        Edge_Tracking,
        Edge_Tracking_with_KLT
    };
    public enum edges
    {
        Visible_Edge_tracking_only,
        All_Edges_tracking
    };

    [Header("Tracking Method Selection")]
    public Script_mbt.tracking trackingMethod = tracking.Edge_Tracking;         // selected by default
    public Script_mbt.edges edgesVisibility = edges.Visible_Edge_tracking_only; // selected by default

    int tr = 0; // tracking method
    int e = 0;  // view all edges
    int[] flag_state = new int[1]; // state of ViSP tracker: detection or tracking
    int[] nEdges = new int[1];     // number of visisble and tracked edges

    void Start()
    {
        wct = new WebCamTexture();
        //wct = new WebCamTexture(WebCamTexture.devices[0].name, 640, 480, 30);
        rend = GetComponent<Renderer>();
        rend.material.mainTexture = wct;

        wct.Play(); //Start capturing image using webcam  

        aspect_ratio = (double)wct.width / wct.height;
        Debug.Log("Aspect Ratio of camera used: " + aspect_ratio);
        Debug.Log("Resolution X: " + wct.width);
        Debug.Log("Resolution Y: " + wct.height);

        //changing the height and width of webcamtexture plane according to the camera resolution:
        transform.localScale = new Vector3((float)aspect_ratio, (float)1f, (float)1f);

        createCaoFile(0.125);

        if (trackingMethod == tracking.Edge_Tracking_with_KLT)
            tr = 1;
        else
            tr = 0;

        if (edgesVisibility == edges.All_Edges_tracking)
            e = 1;
        else
            e = 0;

        //Change camera parameters in the inspector window
        InitMBT(cam_px, cam_py, cam_u0, cam_v0, tr);
        //Debug.Log(Application.persistentDataPath);
    }

    double x1,x2;
    double y1,y2;

    void Update()
    {
        AprilTagMBT(Color32ArrayToByteArray(wct.GetPixels32()), wct.height, wct.width, pointx, pointy, kltX, kltY, kltNumber,tr, e, flag_state, nEdges);
        GameObject[] line = GameObject.FindGameObjectsWithTag("Line");
        int maxEdges = 12;
        if (flag_state[0] == 1) // tracking
        {
            Debug.Log("nEdges = "+ nEdges[0]);

            // Draw lines that are visible and tracked currently: controlled by nEdges
            for (int i = 0; i < nEdges[0]; i++)
            {
                //Scaling according to plane
                x1 = -((float)(10 * aspect_ratio) / wct.width * pointx[2 * i] - (10 * aspect_ratio) / 2);
                x2 = -((float)(10 * aspect_ratio) / wct.width * pointx[2 * i + 1] - (10 * aspect_ratio) / 2);
                y1 = -((float)10 / wct.height * pointy[2 * i] - 10 / 2);
                y2 = -((float)10 / wct.height * pointy[2 * i + 1] - 10 / 2);

                //Draw lines
                line[i].GetComponent<LineRenderer>().SetPosition(0, new Vector3((float)x1, (float)y1, -9f));
                line[i].GetComponent<LineRenderer>().SetPosition(1, new Vector3((float)x2, (float)y2, -9f));

                //Following line is for debugging purpose only: shows lines only in the 'Scene' view, not in 'Game' view:    
                Debug.DrawLine(new Vector3((float)x1, (float)y1, -19f), new Vector3((float)x2, (float)y2, -19f));
            }
            // Remove edges that are not visible and tracked currently
            for (int i = nEdges[0]; i < maxEdges; i++)
            {
                //Remove lines
                line[i].GetComponent<LineRenderer>().SetPosition(0, new Vector3(0, 0, 0));
                line[i].GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, 0));
            }
        }
        else //When aprilTag is not detected: Remove all edges
        {
            for (int i = 0; i < 12; i++)
            {
                //Remove lines
                line[i].GetComponent<LineRenderer>().SetPosition(0, new Vector3(0, 0, 0));
                line[i].GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, 0));
            }
        }

        // Testing KLT in Unity:
        //if (tr==1)
        //    Debug.Log("Num of KLT Feature Points:" + kltNumber[0]);
        
        //if(tr==0)
        //    Debug.Log("Edge Tracking");

        //For showing the KLT feature points:

        //for (int i = 0; i < kltNumber[0]; i++) {
        //    GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    s.tag = "kltPoint";
        //    s.transform.localScale = new Vector3(1, 1, 1);
        //    x1 = -((float)13.33 / X * kltX[i] * 2 - 13.33 / 2);
        //    y1 = -((float)10 / Y * kltY[i] * 2 - 10 / 2);
        //    s.transform.localPosition = new Vector3((float)x1, (float) x2, -20f);
        //}
        //GameObject[] dest = GameObject.FindGameObjectsWithTag("kltPoint");
    }

    //Function for converting into Byte Array to be sent to functions in DLL
    private static byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        if (colors == null || colors.Length == 0)
            return null;

        int length = colors.Length;
        byte[] bytes = new byte[length];
        int value = 0;

        for (int i = 0; i < colors.Length; i++)
        {
            value = (colors[i].r + colors[i].g + colors[i].b) / 3;
            bytes[colors.Length - i - 1] = (byte)value;
        }

        return bytes;
    }
}
