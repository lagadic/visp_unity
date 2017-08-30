/*
   C# Script
   How to use visp library in unity.

   This script establishes basic data communication between visp and unity. Eg: finding dot product of two vectors.

	 vectors:
	 a = {a1, a2, a3};
	 b = {b1, b2, b3};

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class demo : MonoBehaviour {

	// Import DLL (visp-plugin-frame.dll)
	[DllImport("visp-plugin-frame", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dot_prod")]
	//Imported function dot_prod()
	public static extern double dot_prod(uint[] vec);

	// Import DLL (visp-plugin-frame.dll)
	[DllImport("visp-plugin-frame", CallingConvention = CallingConvention.Cdecl, EntryPoint = "passFrame")]
	//Imported function passFrame()
	public static extern void passFrame(byte[] bitmap, int height, int width);

	public WebCamTexture webcamTexture;
	public Color32[] data;
	public uint[] vec;
	public int SceneWidth;
	public int SceneHeight;
	public int WebCamWidth;
	public int WebCamHeight;
	public int cutoffX;
	public int cutoffY;
	public double getMouseX;
	public double getMouseY;

	void Start()
	{
		initVector();
		initWebcam();
		webcamTexture.Play();
		printDotProd();

		// Passing the initial frame
		passFrame(Color32ArrayToByteArray(webcamTexture.GetPixels32()), webcamTexture.height, webcamTexture.width);
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

	}

	void printDotProd()
	{
		Debug.Log("Dot Product of the vectors is:");
		Debug.Log(dot_prod(vec));
	}

	void initVector()
	{
		// init vector for passing through dot_prod()
		vec = new uint[] {1,2,3,1,2,3};
	}

	void initWebcam()
	{
		webcamTexture = new WebCamTexture();
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
