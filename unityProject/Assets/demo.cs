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
/*
    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "initBlobTracker")]
    //Imported function initBlobTracker()
    public static extern void initBlobTracker(int blobCenterX, int blobCenterY, bool isClicked, bool init_done);

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "trackBlob")]
    //Imported function trackBlob()
    public static extern void passFrame(byte[] bitmap, int height, int width);
*/

    // Import DLL (visp-demo.dll)
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "BlobTracker")]
    // Imported function BlobTracker()
    public static extern void BlobTracker(byte[] bitmap, int height, int width, int clickX, int clickY, bool isClicked, bool init_done, double retX, double retY);

    public WebCamTexture webcamTexture;
    public Color32[] data;
    public bool isClicked;
    public bool init_done;
    public double retX;
    public double retY;
    //vectors:
    //a = {a1, a2, a3};
    //b = {b1, b2, b3};

    private uint[] vec = {1,2,3,1,2,3};

    void Start()
    {
        webcamTexture = new WebCamTexture();
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = webcamTexture;
        data = new Color32[webcamTexture.width * webcamTexture.height];
        webcamTexture.Play();

        uint[] vec = { 1, 2, 3, 1, 2, 3 };

        Debug.Log("Dot Product of the vectors is:");
        Debug.Log(dot_prod(vec));

        isClicked = false;
        init_done = false;

        // Initialize as any values, since values of retX and retY are going to be updated anyway.
        retX = 0.0;
        retY = 0.0;
    }

    void Update()
    {
        // ########################################################################
        // ### Get the coordinates clickX and clickY from the scene as doubles. ###
        // ########################################################################
        //passFrame(Color32ArrayToByteArray(webcamTexture.GetPixels32()), webcamTexture.height, webcamTexture.width);
        BlobTracker(Color32ArrayToByteArray(webcamTexture.GetPixels32()), webcamTexture.height, webcamTexture.width, clickX, clickY, isClicked, init_done, retX, retY);
        Debug.Log(retX);
        Debug.Log(retY);
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
