using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class demo : MonoBehaviour {
		// Import DLL (visp-demo.dll)
//    [DllImport("visp-demo", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dot_prod")]
		//Imported function dot_prod()
//    public static extern double dot_prod(uint[] vec);

		//vectors:
		//a = {a1, a2, a3};
		//b = {b1, b2, b3};

		private uint[] vec = {1,2,3,1,2,3};

		void Start () {
			Debug.Log("Dot Product of the vectors is:");
//			Debug.Log(dot_prod(vec));
		}
}
