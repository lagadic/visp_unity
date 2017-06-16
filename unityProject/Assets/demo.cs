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
    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "displayImage")]
    //Imported function displayImage()
    public static extern double displayImage(byte[] bitmap, int height, int width);



    public WebCamTexture webcamTexture;
    public Color32[] data;

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
    }

    void Update()
    {

        double time = displayImage(Color32ArrayToByteArray(webcamTexture.GetPixels32()), webcamTexture.height, webcamTexture.width);

        Debug.Log(webcamTexture.height);
        Debug.Log(webcamTexture.width);
        Debug.Log(time); 
    }

    private static byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        if (colors == null || colors.Length == 0)
            return null;

        int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
        int length = lengthOfColor32 * colors.Length;
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

