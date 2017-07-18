using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class demo : MonoBehaviour {

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dot_prod")]
    //Imported function dot_prod()
    public static extern double dot_prod(uint[] vec);

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "passFrame")]
    //Imported function passFrame()
    public static extern void passFrame(byte[] bitmap, int height, int width);

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "initBlobTracker")]
    //Imported function initBlobTracker()
    public static extern void initBlobTracker(double getMouseX, double getMouseY, uint[] init_done);

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "trackBlob")]
    //Imported function trackBlob()
    public static extern void trackBlob();

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getBlobCoordinates")]
    //Imported function getBlobCoordinates()
    public static extern void getBlobCoordinates(double[] cogX, double[] cogY, uint[] init_done);

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getNumberOfBlobs")]
    //Imported function getNumberOfBlobs()
    public static extern int getNumberOfBlobs();

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "estimatePose")]
    //Imported function estimatePose()
    public static extern void estimatePose(uint[] init_pose);

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "initFourBlobTracker")]
    //Imported function initFourBlobTracker()
    public static extern void initFourBlobTracker(uint[] init_pose);

    public WebCamTexture webcamTexture;
    public Color32[] data;
//    public bool[] isClicked;
    public uint[] init_done;
    public uint[] init_pose;
    public double[] cogX;
    public double[] cogY;
    public int SceneWidth;
    public int SceneHeight;
    public int WebCamWidth;
    public int WebCamHeight;
    public int cutoffX;
    public int cutoffY;
    public int numOfBlobs;
    public double getMouseX;
    public double getMouseY;

    //vectors:
    //a = {a1, a2, a3};
    //b = {b1, b2, b3};

    public uint[] vec;

    void Start()
    {
        //isClicked = new bool[1];
        //isClicked[0] = true;
        init_done = new uint[1];
        init_done[0] = 0;

        // Initialize cogX, cogY [center of gravity]
        cogX = new double[1];
        cogY = new double[1];

        webcamTexture = new WebCamTexture();
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = webcamTexture;
        data = new Color32[webcamTexture.width * webcamTexture.height];
        webcamTexture.Play();

        vec = new uint[6];
        vec[0] =  1;
        vec[1] =  2;
        vec[2] =  3;
        vec[3] =  1;
        vec[4] =  2;
        vec[5] =  3;
        Debug.Log("Dot Product of the vectors is:");
        Debug.Log(dot_prod(vec));

        initFourBlobTracker(init_pose);
        trackBlob();
/*
        Debug.Log("Cam width");
        Debug.Log(webcamTexture.width);
        Debug.Log("Cam height");
        Debug.Log(webcamTexture.height);

        Debug.Log("Window width");
        Debug.Log(Screen.width);
        Debug.Log("Window height");
        Debug.Log(Screen.height);
*/
        SceneWidth = Screen.width;
        WebCamWidth = webcamTexture.width;

        SceneHeight = Screen.height;
        WebCamHeight = webcamTexture.height;

        cutoffX = (SceneWidth - WebCamWidth) / 2;
    }

    void Update()
    {
        // ##############################################################################
        // ### Get the coordinates getMouseX and getMouseY from the scene as doubles. ###
        // ##############################################################################
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Pressed left click.");
            Debug.Log(Input.mousePosition[0]);
            Debug.Log(Input.mousePosition[1]);
            //getMouseX = Input.mousePosition[0] - cutoffX;
            //getMouseY = WebCamHeight - Debug.Log(Input.mousePosition[1]);
	      }

        getMouseX = 1;
        getMouseY = 1;
        //Debug.Log(vec[5]);
        passFrame(Color32ArrayToByteArray(webcamTexture.GetPixels32()), webcamTexture.height, webcamTexture.width);
        if(init_done[0] == 0)
    		{
          //Debug.Log(init_done[0]);
          initBlobTracker(getMouseX, getMouseY, init_done);
        }
        else
        {
          Debug.Log("tracking");
          trackBlob();
          getBlobCoordinates(cogX, cogY, init_done);
          Debug.Log(cogX[0]);
          Debug.Log(cogY[0]);
        }

        numOfBlobs = getNumberOfBlobs();
        if (numOfBlobs == 4) {
          estimatePose(init_pose);
          init_pose[0] = 0;
        }
        else {
          init_pose[0] = 1;
        }
    }

    private static byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        if (colors == null || colors.Length == 0)
            return null;

        int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
        int length = colors.Length;
        byte[] bytes = new byte[length];

        GCHandle handle = default(GCHandle);

        int value = 0;

        handle = GCHandle.Alloc(value, GCHandleType.Pinned);
        for (int i=0; i< colors.Length; i++)
        {
            value = (colors[i].r + colors[i].g + colors[i].b) / 3;
            bytes[colors.Length - i -1] = (byte)value;
        }

        return bytes;
    }
}
