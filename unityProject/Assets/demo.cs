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
    public Vector3 cam_direction;
    public Matrix4x4 cMo_mat;
    public Vector3 gameObjCoords;
    public Vector3 cam_coords;
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
    public double x;
    public double y;
    public double z;
    //vectors:
    //a = {a1, a2, a3};
    //b = {b1, b2, b3};

    public uint[] vec;

    void Start()
    {
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

        cMo_mat = new Matrix4x4();

        cam_coords[0] = Camera.main.transform.position.x;
        cam_coords[1] = Camera.main.transform.position.y;
        cam_coords[2] = Camera.main.transform.position.z;

        cam_direction = Camera.main.transform.eulerAngles;
        Debug.Log("Orentation");
        Debug.Log(cam_direction);
        Debug.Log("coordinates");
        Debug.Log(cam_coords);
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
        cMo_mat.SetRow(0, new Vector4(1f, 0f, 0f, 0f));
        cMo_mat.SetRow(1, new Vector4(0f, 1f, 0f, 0f));
        cMo_mat.SetRow(2, new Vector4(0f, 0f, 1f, 0f));
        cMo_mat.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

        cMo[3] = 0;
        cMo[7] = 0;
        cMo[11]= 1;

        webcamTexture = new WebCamTexture();

        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.AddComponent<Rigidbody>();

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
/*
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
*/
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
/*
          cMo_mat.SetRow(0, new Vector4((float)cMo[0], (float)cMo[1], (float)cMo[2], (float)cMo[3]));
          cMo_mat.SetRow(1, new Vector4((float)cMo[4], (float)cMo[5], (float)cMo[6], (float)cMo[7]));
          cMo_mat.SetRow(2, new Vector4((float)cMo[8], (float)cMo[9], (float)cMo[10], (float)cMo[11]));


          cMo_mat.SetRow(0, new Vector4(0, 0, 0, (float)cMo[3]));
          cMo_mat.SetRow(1, new Vector4(0, 0, 0, (float)cMo[7]));
          cMo_mat.SetRow(2, new Vector4(0, 0, 0, -1*(float)cMo[11]));
          cMo_mat.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
          */
        }
        else {
          init_pose[0] = 1;
//          gameObjCoords = new Vector3(0.0f,1.0f,0.0f);
        }
        //gameObjCoords = cMo_mat.MultiplyPoint3x4(cam_coords);
        x = 10 * cMo[3] / cMo[11];
        y = 10 * cMo[7] / cMo[11];
        z = 0;

        gameObjCoords[0] = (float)x;
        gameObjCoords[1] = (float)y;
        gameObjCoords[2] = (float)z;

        Debug.Log("Coordinates of Game Object: ");
        Debug.Log(gameObjCoords[0]);
        Debug.Log(gameObjCoords[1]);
        Debug.Log(gameObjCoords[2]);
        cube.transform.position = gameObjCoords;
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
