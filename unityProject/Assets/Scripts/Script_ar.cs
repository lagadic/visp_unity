/****************************************************************************
 *
 * ViSP, open source Visual Servoing Platform software.
 * Copyright (C) 2005 - 2020 by Inria. All rights reserved.
 *
 * This software is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * See the file LICENSE.txt at the root directory of this source
 * distribution for additional information about the GNU GPL.
 *
 * For using ViSP with software that can not be combined with the GNU
 * GPL, please contact Inria about acquiring a ViSP Professional
 * Edition License.
 *
 * See http://visp.inria.fr for more information.
 *
 * This software was developed at:
 * Inria Rennes - Bretagne Atlantique
 * Campus Universitaire de Beaulieu
 * 35042 Rennes Cedex
 * France
 *
 * If you have questions regarding the use of this file, please contact
 * Inria at visp@inria.fr
 *
 * This file is provided AS IS with NO WARRANTY OF ANY KIND, INCLUDING THE
 * WARRANTY OF DESIGN, MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Description:
 * Unity application that shows how to use ViSPUnity plugin.
 *
 *****************************************************************************/

 /*!
  \example Script_ar.cs
  Unity CSharp script that allows to detect and AprilTag and display a cube in Augmented Reality using ViSPUnity plugin.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class Script_ar : MonoBehaviour
{
  // Functions imported FROM ViSPUnity wrapper (DLL on Windows, Bundle on OSX)
  [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_EnableDisplayForDebug")]
  public static extern void Visp_EnableDisplayForDebug(bool enable_display);
  [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_WrapperFreeMemory")]
  public static extern void Visp_WrapperFreeMemory();
  [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_ImageUchar_SetFromColor32Array")]
  public static extern void Visp_ImageUchar_SetFromColor32Array(Color32[] bitmap, int height, int width);
  [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_CameraParameters_Init")]
  public static extern void Visp_CameraParameters_Init(double cam_px, double cam_py, double cam_u0, double cam_v0);
  [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_DetectorAprilTag_Init")]
  public static extern void Visp_DetectorAprilTag_Init(float quad_decimate, int nthreads);
  [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_DetectorAprilTag_Process")]
  public static extern bool Visp_DetectorAprilTag_Process(double tag_size, float[] tag_cog, float[] tag_length, float[] tag_cMo, double[] detection_time);

  public enum DebugType {
    Enabled,
    Disabled
  };

  WebCamTexture m_webCamTexture;
  Renderer m_renderer;
  WebCamDevice[] m_devices;
  // Reference for GameObject Cube, that is to be moved on the plane
  GameObject m_cube;
  // Reference for GameObject Cube_pivot, that is to be rotated
  GameObject m_cube_pivot;

  // For storing tag characteristics returned by ViSPunity wrapper
  float[] m_tag_cog = new float[2];
  float[] m_tag_length = new float[6];
  float[] m_tag_cMo = new float[16];
  double[] m_detection_time = new double[1];

  bool m_wct_resolution_updated = false;
  float m_aspect_ratio;
  // For debug log
  bool m_log_start = true;
  bool m_log_process = true;

  //Quaternion m_baseRotation;

  [Header("Camera Identifier")]
  public int camera_id = 0;

  [Header("Camera Parameters")] //some default values provided
  public double cam_px = 600;
  public double cam_py = 600;
  public double cam_u0 = 320;
  public double cam_v0 = 240;

  [Header("Tag Size in [m]")]
  public double tag_size = 0.053;

  [Header("Tag Detection Settings")]
  public float quad_decimate = 1;
  public int nthreads = 1;

  [Header("Debugging Settings")]
  public Script_ar.DebugType debug_display = DebugType.Disabled;

  void Start()
  {
    m_devices = WebCamTexture.devices;

    if (m_devices.Length == 0) {
      throw new Exception("No camera device found");
    }

    int max_id = m_devices.Length - 1;
    if (camera_id > max_id) {
      if (m_devices.Length == 1) {
        throw new Exception("Camera with id " + camera_id + " not found. camera_id value should be 0");
      }
      else {
        throw new Exception("Camera with id " + camera_id + " not found. camera_id value should be between 0 and " + max_id.ToString());
      }
    }

    m_webCamTexture = new WebCamTexture(WebCamTexture.devices[camera_id].name, 640, 480, 30);

    m_renderer = GetComponent<Renderer>();
    m_renderer.material.mainTexture = m_webCamTexture;
    //m_baseRotation = transform.rotation;
    m_webCamTexture.Play(); //Start capturing image using webcam

    m_cube = GameObject.Find("Cube");
    m_cube_pivot = GameObject.Find("Cube_pivot");

    Visp_EnableDisplayForDebug((debug_display == DebugType.Enabled) ? true : false);
    // Initialize tag detection
    Visp_CameraParameters_Init(cam_px, cam_py, cam_u0, cam_v0);
    Visp_DetectorAprilTag_Init(quad_decimate, nthreads);

    // For debugging purposes, prints available devices to the console
    if(m_log_start) {
      for(int i = 0; i < m_devices.Length; i++) {
        Debug.Log("Webcam " + i + " available: " + m_devices[i].name);
      }
      Debug.Log("Device name: " + m_webCamTexture.deviceName);
      Debug.Log("Web Cam Texture Resolution init : " + m_webCamTexture.width + " " + m_webCamTexture.height);
      //Debug.Log("Screen resolution : " + Screen.currentResolution.width + " " + Screen.currentResolution.height);
      //Debug.Log("Base rotation : " + m_baseRotation);
      //Debug.Log("Video rotation angle: " + m_webCamTexture.videoRotationAngle);
      Debug.Log("Tag detection settings: quad_decimate=" + quad_decimate + " nthreads=" + nthreads);
      Debug.Log("Camera parameters: u0=" + cam_u0 + " v0=" + cam_v0 + " px=" + cam_px + " py=" + cam_py);
      Debug.Log("Tag size [m]: " + tag_size);
    }
  }

  /*
    When more than one camera is connected, create a square button on the top
    left part of the display that allows to change the device.
   */
  void OnGUI()
  {
    if (m_devices.Length > 1) {
      if( GUI.Button( new Rect(0,0,100,100), "Switch\nto next\ncamera" )) {
        camera_id ++;
        int id = (camera_id % m_devices.Length);
        Debug.Log("Camera id: " + id);
        m_webCamTexture.Stop();
        m_webCamTexture.deviceName = m_devices[id].name;
        Debug.Log("Switch to new device name: " + m_webCamTexture.deviceName);
        m_webCamTexture.Play();
      }
    }
  }

  void Update()
  {
    if (m_webCamTexture == null) {
      return;
    }
    // Warmup camera to get an updated web cam texture resolution up to Update
    // On OSX, m_webCamTexture.width and m_webCamTexture.height returns always 16 for width and height.
    // After a couple of seconds, Web Cam Texture size is updated
    if (! m_wct_resolution_updated) {
      if (m_webCamTexture.width > 100 && m_webCamTexture.height > 100) {

        Debug.Log("Web Cam Texture Resolution: " + m_webCamTexture.width + " " + m_webCamTexture.height);

        m_wct_resolution_updated = true;

        // Change height and width of m_webCamTexture plane according to the camera resolution
        m_aspect_ratio = (float) m_webCamTexture.width / m_webCamTexture.height;
        transform.localScale = new Vector3(m_aspect_ratio, 1f, 1f);
      }
      else {
        return;
      }
    }

    //transform.rotation = m_baseRotation * Quaternion.AngleAxis(m_webCamTexture.videoRotationAngle, Vector3.up);

    if (m_log_start) {
      Debug.Log("Image size: " + m_webCamTexture.width + " x " + m_webCamTexture.height);
      m_log_start = false;
    }
    // Update image
    Visp_ImageUchar_SetFromColor32Array(m_webCamTexture.GetPixels32(), m_webCamTexture.height, m_webCamTexture.width);
    // Detect tag
    bool success = Visp_DetectorAprilTag_Process(tag_size, m_tag_cog, m_tag_length, m_tag_cMo, m_detection_time);

    if (success) {
      if (m_log_process) {
        Debug.Log("tag cog: " + m_tag_cog[0] + " " + m_tag_cog[1]);
        Debug.Log("tag length: " + m_tag_length[0] + " " + m_tag_length[1] + " " + m_tag_length[2] + " " + m_tag_length[3] + " " + m_tag_length[4] + " " + m_tag_length[5]);
        Debug.Log("cMo:\n" + m_tag_cMo[0] + " " + m_tag_cMo[1] + " " + m_tag_cMo[2]  + " " + m_tag_cMo[3] + "\n"
                           + m_tag_cMo[4] + " " + m_tag_cMo[5] + " " + m_tag_cMo[6]  + " " + m_tag_cMo[7] + "\n"
                           + m_tag_cMo[8] + " " + m_tag_cMo[9] + " " + m_tag_cMo[10] + " " + m_tag_cMo[11]);
        Debug.Log("Detection process time: " + m_detection_time[0] + " ms");
        m_log_process = false;
      }

      // Height of m_webCamTexture plane remains fixed (10 units) but width = 10*m_aspect_ratio
      float x = -10f * m_aspect_ratio * (m_tag_cog[0] / m_webCamTexture.width - 1f / 2f);
      float y = -10f * (m_tag_cog[1] / m_webCamTexture.height - 1f / 2f);

      Vector3 vec = new Vector3(x, y, -9);

      // Show game objects
      m_cube.SetActive(true);
      m_cube_pivot.SetActive(true);

      // Change the coordinates of Cube by setting them equal to vector3 vec
      //m_cube.GetComponent<Transform>().position = vec;
      m_cube_pivot.GetComponent<Transform>().position = vec;

      // Comparing the value of diagonals of polygon: this is more accurate than comparing sides of bounding box.
      float max_dim = System.Math.Max(m_tag_length[4], m_tag_length[5]);

      // Scaling factor for Cube: {scale the cube by a factor of 10 if the value of side (diagonal/sqrt(2)) is 480}
      float scale = 10.0f / m_webCamTexture.height * max_dim / (float)Math.Sqrt(2);

      m_cube.GetComponent<Transform>().localPosition = new Vector3(0.5f * scale, 0, 0);

      m_cube.transform.localScale = new Vector3(scale, scale, scale);

      Vector3 forward;
      forward.x = m_tag_cMo[0];
      forward.y = m_tag_cMo[4];
      forward.z = m_tag_cMo[8];
      Vector3 upwards;
      upwards.x = m_tag_cMo[1];
      upwards.y = m_tag_cMo[5];
      upwards.z = m_tag_cMo[9];

      m_cube_pivot.transform.rotation = Quaternion.LookRotation(forward, upwards);
    }
    else {
      // Hide game objects
      m_cube.SetActive(false);
      m_cube_pivot.SetActive(false);
    }
  }

  void OnApplicationQuit()
  {
    Visp_WrapperFreeMemory();
    Debug.Log("Application ending after " + Time.time + " seconds");
  }
}
