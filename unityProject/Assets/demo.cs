/*
C# Script
How to use visp library in unity.

This script establishes:
1. Basic data communication between visp and unity. Eg: finding dot product of two vectors.
2. User initialized blob tracking.
3. four blob detecting and tracking. Finally, finding the pose using four point blob algorithm.

vectors:
a = {a1, a2, a3};
b = {b1, b2, b3};

cMo = pose estimation vector

pose estimation matrix = [ cMo[0] cMo[1] cMo[2] cMo[3]
                           cMo[4] cMo[5] cMo[6] cMo[7]
                           cMo[8] cMo[9] cMo[10] cMo[11]
                           0      0      0      1    ]


pose estimation matrix = [R(3*3) t(3*1)
                          0      1   ]

*/

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


    public GameObject cube;
    public WebCamTexture webcamTexture;
    public Color32[] data;
    public Color blue;
    public Vector3 cam_direction;
    public Matrix4x4 cMo_mat;
    public Vector3 gameObjCoords;
    public Vector3 cam_coords;
    public uint[] numOfBlobs;
    public uint[] init_done;
    public uint[] init_pose;
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
    public double gameObjX;
    public double gameObjY;
    public double gameObjZ;

    public uint[] vec;

    void Start()
    {
        declVars();
        initVars();
        initGameObj();
        initWebcam();
        webcamTexture.Play();
        printDotProd();

        // Passing the initial frame
        passFrame(Color32ArrayToByteArray(webcamTexture.GetPixels32()), webcamTexture.height, webcamTexture.width);
        initFourBlobTracker(init_pose);

        //changing cube dimentions to make it visible in front of the camera
        cube.transform.localScale = new Vector3(0.1f,0.1f,0.1f);

        //setting cube color to blue
        cube.GetComponent<Renderer>().material.color = blue;
    }

    void Update()
    {
        // Get the coordinates getMouseX and getMouseY from the scene as doubles.

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Pressed left click.");
            Debug.Log(Input.mousePosition[0]);
            Debug.Log(Input.mousePosition[1]);
	      }

        getMouseX = 1;
        getMouseY = 1;
        passFrame(Color32ArrayToByteArray(webcamTexture.GetPixels32()), webcamTexture.height, webcamTexture.width);

        // User initiallized Blob tracker
        // ublobTrack();

        getNumberOfBlobs(numOfBlobs);
        Debug.Log("Number Of blobs");
        Debug.Log(numOfBlobs[0]);

        if (numOfBlobs[0] == 4) {
          estimatePose(init_pose, cMo);
          init_pose[0] = 0;
          cube.SetActive(true);
        }
        else {
          init_pose[0] = 1;
          cube.SetActive(false);
        }

        //gameObjCoords = cMo_mat.MultiplyPoint3x4(cam_coords);

        // Scaling the x,y screen coordinates.
        gameObjX = cMo[3];
        gameObjY = cMo[7];
        gameObjZ = cMo[11];

        gameObjCoords[0] = (float)gameObjX;
        gameObjCoords[1] = (float)gameObjY;
        gameObjCoords[2] = (float)(gameObjZ - 10);

        Debug.Log("Coordinates of Game Object: ");
        Debug.Log(gameObjCoords[0]);
        Debug.Log(gameObjCoords[1]);
        Debug.Log(gameObjCoords[2]);

        // update cube gameObj position
        cube.transform.position = gameObjCoords;
    }

    void printDotProd()
    {
      Debug.Log("Dot Product of the vectors is:");
      Debug.Log(dot_prod(vec));
    }

    void declVars()
    {
      // init flag variables
      init_done = new uint[1];
      init_pose = new uint[1];

      // Number of blobs detected
      numOfBlobs = new uint[1];

      // Initialize cogX, cogY [center of gravity]
      cogX = new double[1];
      cogY = new double[1];

      // Pose matrix cMo
      cMo = new double[12];
      cMo_mat = new Matrix4x4();

      // webCamTexture
      webcamTexture = new WebCamTexture();
    }

    void initVars()
    {
      // init flags as false
      init_done[0] = 0;
      init_pose[0] = 0;

      // main camera coordinates
      cam_coords[0] = Camera.main.transform.position.x;
      cam_coords[1] = Camera.main.transform.position.y;
      cam_coords[2] = Camera.main.transform.position.z;

      // main camera orentation in euler angles
      cam_direction = Camera.main.transform.eulerAngles;

      // init cMo pose matrix
      cMo_mat.SetRow(0, new Vector4(1f, 0f, 0f, 0f));
      cMo_mat.SetRow(1, new Vector4(0f, 1f, 0f, 0f));
      cMo_mat.SetRow(2, new Vector4(0f, 0f, 1f, 0f));
      cMo_mat.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

      // init cMo vector, to be populated by estimatePose()
      cMo[3] = 0;
      cMo[7] = 0;
      cMo[11]= 1;

      // init vector for passing through dot_prod()
      vec = new uint[] {1,2,3,1,2,3};
    }

    void ublobTrack()
    {
      // user initialized blob tracker
      if(init_done[0] == 0)
      {
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
    }

    void initGameObj()
    {
      // init cube GameObject
      cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
      cube.AddComponent<Rigidbody>();
    }

    void initWebcam()
    {
      // set webcamTexture height and width
      webcamTexture.requestedHeight = 320;
      webcamTexture.requestedWidth = 240;

      // init renderer
      Renderer renderer = GetComponent<Renderer>();
      renderer.material.mainTexture = webcamTexture;

      Debug.Log(renderer.bounds.size.x);
      Debug.Log(renderer.bounds.size.y);
      Debug.Log(renderer.bounds.size.z);
      // declare frame data as Color32
      data = new Color32[webcamTexture.width * webcamTexture.height];
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
