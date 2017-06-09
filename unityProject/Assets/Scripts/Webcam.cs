using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Webcam : MonoBehaviour {

	public GameObject webcamTexturePrefab;

	void Start () {
        GameObject go = Instantiate(webcamTexturePrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        go.transform.parent = gameObject.transform;
        WebCamTexture webcamTexture = new WebCamTexture();
        go.transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = webcamTexture;
        webcamTexture.Play();
	}

	void Update () {

	}
}
