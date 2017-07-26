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
    public static extern void getNumberOfBlobs(uint[] numOfBlobs);

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "estimatePose")]
    //Imported function estimatePose()
    public static extern void estimatePose(uint[] init_pose, double[] cMo);

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "initFourBlobTracker")]
    //Imported function initFourBlobTracker()
    public static extern void initFourBlobTracker(uint[] init_pose);


    public WebCamTexture webcamTexture;
    public Color32[] data;
//    public bool[] isClicked;
    public uint[] numOfBlobs;
    public uint[] init_done;
    public uint[] init_pose;
    public int[] pose_uc;
    public double[] pose_ucoords;
    public double[] cogX;
    public double[] cogY;
    public double[] cMo;
    public int SceneWidth;
    public int SceneHeight;
    public int WebCamWidth;
    public int WebCamHeight;
    public int cutoffX;
    public int cutoffY;
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

        init_pose = new uint[1];
        init_pose[0] = 0;

        numOfBlobs = new uint[1];

        // Initialize cogX, cogY [center of gravity]
        cogX = new double[1];
        cogY = new double[1];

        pose_ucoords = new double[2];
        pose_uc = new int[2];

        /*
        ######################################################
        ############ cMo = pose estimation vector ############
        ######################################################

        pose estimation matrix = [ cMo[0] cMo[1] cMo[2] cMo[3]
                                   cMo[4] cMo[5] cMo[6] cMo[7]
                                   cMo[8] cMo[9] cMo[10] cMo[11]
                                   0      0      0      1    ]


        pose estimation matrix = [R(3*3) t(3*1)
                                  0      1   ]

        */

        cMo = new double[12];

        webcamTexture = new WebCamTexture();

        webcamTexture.requestedHeight = 320;
        webcamTexture.requestedWidth = 240;
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

        // Passing the initial frame
        passFrame(Color32ArrayToByteArray(webcamTexture.GetPixels32()), webcamTexture.height, webcamTexture.width);
        initFourBlobTracker(init_pose);

        Debug.Log("webcamTexture.width");
        Debug.Log(webcamTexture.width);
        Debug.Log("webcamTexture.height");
        Debug.Log(webcamTexture.height);

        Debug.Log("Screen.width");
        Debug.Log(Screen.width);
        Debug.Log("Screen.height");
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
            Debug.Log(Input.mousePosition[0]);
            Debug.Log(Input.mousePosition[1]);
            //getMouseX = Input.mousePosition[0] - cutoffX;
            //getMouseY = WebCamHeight - Debug.Log(Input.mousePosition[1]);
	      }

        getMouseX = 1;
        getMouseY = 1;
        passFrame(Color32ArrayToByteArray(webcamTexture.GetPixels32()), webcamTexture.height, webcamTexture.width);


/*
        ############################################################
        ############## User initiallized Blob tracker ##############
        ############################################################

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
*/

        getNumberOfBlobs(numOfBlobs);
        Debug.Log("Number Of blobs");
        Debug.Log(numOfBlobs[0]);

        if (numOfBlobs[0] == 4) {
          estimatePose(init_pose, cMo);
          init_pose[0] = 0;

          pose_ucoords[0] = 1.333 * cMo[7] + 123;
          pose_ucoords[1] = 320 - 1.333 * cMo[3];

          pose_uc[0] = (int)(Math.Floor(pose_ucoords[0]));
          pose_uc[1] = (int)(Math.Floor(pose_ucoords[1]));

          Debug.Log("x-pose in unity cam coordinates");
          Debug.Log(pose_uc[0]);
          Debug.Log("y-pose in unity cam coordinates");
          Debug.Log(pose_uc[1]);
          Debug.Log("z-pose");
          Debug.Log(cMo[11]);
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
