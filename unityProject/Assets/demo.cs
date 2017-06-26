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
    public static extern void initBlobTracker(int getMouseX, int getMouseY, bool isClicked, bool init_done);

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getBlobCoordinates")]
    //Imported function getBlobCoordinates()
    public static extern void getBlobCoordinates(double cogX, double cogY);

    public WebCamTexture webcamTexture;
    public Color32[] data;
    public bool isClicked;
    public bool init_done;
    public double cogX;
    public double cogY;
    public int SceneWidth;
    public int SceneHeight;
    public int WebCamWidth;
    public int WebCamHeight;
    public int cutoffX;
    public int cutoffY;
    //vectors:
    //a = {a1, a2, a3};
    //b = {b1, b2, b3};

    private uint[] vec = {1,2,3,1,2,3};

    void Start()
    {
        isClicked = false;
        init_done = false;

        // Initialize cogX, cogY [center of gravity]
        cogX = 0;
        cogY = 0;

        webcamTexture = new WebCamTexture();
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = webcamTexture;
        data = new Color32[webcamTexture.width * webcamTexture.height];
        webcamTexture.Play();

        uint[] vec = { 1, 2, 3, 1, 2, 3 };
        Debug.Log("Dot Product of the vectors is:");
        Debug.Log(dot_prod(vec));

        Debug.Log("Cam width");
        Debug.Log(webcamTexture.width);
        Debug.Log("Cam height");
        Debug.Log(webcamTexture.height);

        Debug.Log("Window width");
        Debug.Log(Screen.width);
        Debug.Log("Window height");
        Debug.Log(Screen.height);

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
            getMouseX = Input.mousePosition[0] - cutoffX;
            getMouseY = WebCamHeight - Debug.Log(Input.mousePosition[1]);
        }

        passFrame(Color32ArrayToByteArray(webcamTexture.GetPixels32()), webcamTexture.height, webcamTexture.width);
        if(!init_done)
    		{
          initBlobTracker(int getMouseX, int getMouseY, bool isClicked, bool init_done);
        }
        else
        {
          getBlobCoordinates(double cogX, double cogY)
          Debug.Log(cogX);
          Debug.Log(cogY);
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
