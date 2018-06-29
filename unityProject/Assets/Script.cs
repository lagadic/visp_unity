using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class Script : MonoBehaviour
{

    // FUNCTION IMPORTED FROM DLL:
    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "AprilTagFunctionsCombined")]
    public static extern void AprilTagFunctionsCombined(byte[] bitmap, int height, int width, double cam_px, double cam_py, double cam_u0, double cam_v0, double[] coord, double[] U, double[] V, double[] T, double[] h, double[] w, double[] apr);

    //For storing U, V and T vectors
    double[] U = new double[4];
    double[] V = new double[4];
    double[] T = new double[4];

    WebCamTexture wct;
    Renderer rend;
    Texture2D tex;

    //For storing coordinates (pixel) of apriltag 
    double[] coord = new double[4];

    double[] h = new double[1];
    double[] w = new double[1];
    double[] apr = new double[6];

    [Header("Enter your Camera Resolution here")]
    public int X = 640;   // for example 640
    public int Y = 480;   // for example 480

    [Header("Enter your Camera Parameters here")]
    public double cam_px = 1131.561907;
    public double cam_py = 1085.157822;
    public double cam_u0 = 588.2376812;
    public double cam_v0 = 191.1328903;

    void Start()
    {

        wct = new WebCamTexture();
        //wct = new WebCamTexture(WebCamTexture.devices[0].name, 640, 480, 30);
        rend = GetComponent<Renderer>();
        rend.material.mainTexture = wct;

        wct.Play(); //Start capturing image using webcam  
    }


    void Update()
    {
        AprilTagFunctionsCombined(Color32ArrayToByteArray(wct.GetPixels32()), wct.height, wct.width, cam_px, cam_py, cam_u0, cam_v0, coord, U, V, T, h, w, apr);

        double x = (float)13.333 / X * coord[2] - 13.33 / 2;
        double y = (float)10 / Y * coord[0] - 10 / 2;
        Vector3 vec = new Vector3((float)-x, (float)-y, -9);

        //Reference for GameObject Cube, that is to be moved on the plane 
        GameObject cube = GameObject.Find("Cube");

        //Reference for GameObject Cube_pivot, that is to be rotated
        GameObject cube_pivot = GameObject.Find("Cube_pivot");

        //change the coordinates of Cube by setting them equal to vector3 vec
        //cube.GetComponent<Transform>().position = vec;
        cube_pivot.GetComponent<Transform>().position = vec;

        // Max value of side was <480. CHECKED USING apr[0]
        // Comparing the value of diagonals of polygon: this is more accurate than comparing sides of bounding box.
        double max_dim = System.Math.Max(apr[4], apr[5]);

        // Scaling factor for Cube: {scale the cube by a factor of 10 if the value of side (diagonal/sqrt(2)) is 480}
        float scale = (float)(10.0f / Y) * (float)max_dim / (float)Math.Sqrt(2);


        //Vector3 vector = new Vector3(0.5f * scale, 0f, 0f);
        cube.GetComponent<Transform>().localPosition = new Vector3(0.5f*scale,0,0);


        cube.transform.localScale = new Vector3((float)scale, (float)scale, (float)scale);
        //cube_pivot.transform.localScale = new Vector3((float)scale, (float)scale, (float)scale);

        //cube.transform.rotation = Quaternion.LookRotation(new Vector3((float)U[0], (float)U[1], (float)U[2]), new Vector3((float)V[0], (float)V[1], (float)V[2]));
        cube_pivot.transform.rotation = Quaternion.LookRotation(new Vector3((float)U[0], (float)U[1], (float)U[2]), new Vector3((float)V[0], (float)V[1], (float)V[2]));
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
