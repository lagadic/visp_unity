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
  \example Script_mbt.cs
  Unity CSharp script that allows tracking a cube using ViSPUnity plugin.
  The cube has on one face an ArilTag. For optimal tracking the cube should be textured on the other faces.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class Script_mbt : MonoBehaviour
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
  [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_MbGenericTracker_CreateCaoFile")]
  public static extern void Visp_MbGenericTracker_CreateCaoFile(double cube_edge_size);
  [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_MbGenericTracker_SetFeatureType")]
  public static extern void Visp_MbGenericTracker_SetFeatureType(int feature_type);
  [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_MbGenericTracker_SetMovingEdgesSettings")]
  public static extern void Visp_MbGenericTracker_SetMovingEdgesSettings(int range, double sample_step);
  [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_MbGenericTracker_SetKeypointSettings")]
  public static extern void Visp_MbGenericTracker_SetKeypointSettings(double quality, int mask_border);
  [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_MbGenericTracker_Init")]
  public static extern void Visp_MbGenericTracker_Init(double angle_appear, double angle_disappear, double projection_error_threshold);
  [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_MbGenericTracker_Process")]
  public static extern bool Visp_MbGenericTracker_Process(double tag_size, float[] visible_edges_pointx, float[] visible_edges_pointy,
  int[] visible_edges_number, float[] cube_cMo, double[] tracking_time);

  WebCamTexture m_webCamTexture;
  Renderer m_renderer;
  WebCamDevice[] m_devices;
  GameObject[] m_line;

  bool m_wct_resolution_updated = false;
  float m_aspect_ratio;
  // For debug log
  bool m_log_start = true;
  bool m_log_process = true;

  // For results returned by the tracker
  double[] m_tracking_time = new double[1];
  float[] m_visible_edges_pointx = new float[24];
  float[] m_visible_edges_pointy = new float[24];
  int[] m_visible_edges_number = new int[1];     // number of visisble and tracked edges
  float[] m_cube_cMo = new float[16];

  public enum FeatureType {
    Edge_Tracking,
    Hybrid_Tracking
  };
  public enum DebugType {
    Enabled,
    Disabled
  };

  [Header("Camera Identifier")]
  public int camera_id = 0;

  [Header("Camera Parameters")] //some default values provided
  public double cam_px = 600;
  public double cam_py = 600;
  public double cam_u0 = 320;
  public double cam_v0 = 240;

  [Header("Cube Settings in [m]")]
  public double tag_size = 0.053;
  public double cube_size = 0.125;

  [Header("Tag Detection Settings")]
  public float quad_decimate = 1;
  public int nthreads = 1;

  [Header("Model-Based Tracker Settings")]
  public Script_mbt.FeatureType feature_type = FeatureType.Hybrid_Tracking;         // selected by default
  public int range = 8;
  public double sample_step = 4;
  public double quality=0.01;
  public int mask_border=5;
  public double angle_appear=80;
  public double angle_disappear=85;
  public double projection_error_threshold=30;

  [Header("Debugging Settings")]
  public Script_mbt.DebugType debug_display = DebugType.Disabled;

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
    m_webCamTexture.Play(); //Start capturing image using webcam

    m_line = GameObject.FindGameObjectsWithTag("Line");

    Visp_EnableDisplayForDebug((debug_display == DebugType.Enabled) ? true : false);
    Visp_CameraParameters_Init(cam_px, cam_py, cam_u0, cam_v0);
    Visp_DetectorAprilTag_Init(quad_decimate, nthreads);
    Visp_MbGenericTracker_CreateCaoFile(cube_size);
    Visp_MbGenericTracker_SetFeatureType((feature_type == FeatureType.Hybrid_Tracking) ? 1 : 0);
    Visp_MbGenericTracker_SetMovingEdgesSettings(range, sample_step);
    Visp_MbGenericTracker_SetKeypointSettings(quality, mask_border);
    Visp_MbGenericTracker_Init(angle_appear, angle_disappear, projection_error_threshold);

    // For debugging purposes, prints available devices to the console
    if(m_log_start) {
      for(int i = 0; i < m_devices.Length; i++) {
        Debug.Log("Webcam " + i + " available: " + m_devices[i].name);
      }
      Debug.Log("Device name: " + m_webCamTexture.deviceName);
      Debug.Log("Web Cam Texture Resolution init : " + m_webCamTexture.width + " " + m_webCamTexture.height);
      //Debug.Log("Screen resolution : " + Screen.currentResolution.width + " " + Screen.currentResolution.height);
      //Debug.Log("Video rotation angle: " + m_webCamTexture.videoRotationAngle);
      Debug.Log("Tag detection settings: quad_decimate=" + quad_decimate + " nthreads=" + nthreads);
      Debug.Log("Camera parameters: u0=" + cam_u0 + " v0=" + cam_v0 + " px=" + cam_px + " py=" + cam_py);
      Debug.Log("Tag size [m]: " + tag_size);
      Debug.Log("Cube size [m]: " + cube_size);
      Debug.Log("line length [m]: " + m_line.Length);
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
        m_aspect_ratio = (float)m_webCamTexture.width / m_webCamTexture.height;
        transform.localScale = new Vector3(m_aspect_ratio, 1f, 1f);
      }
      else {
        return;
      }
    }

    if (m_log_start) {
      Debug.Log("Image size: " + m_webCamTexture.width + " x " + m_webCamTexture.height);
      m_log_start = false;
    }

    // Update image
    Visp_ImageUchar_SetFromColor32Array(m_webCamTexture.GetPixels32(), m_webCamTexture.height, m_webCamTexture.width);
    // Cube tracking
    bool success = Visp_MbGenericTracker_Process(tag_size, m_visible_edges_pointx, m_visible_edges_pointy, m_visible_edges_number,  m_cube_cMo, m_tracking_time);
    if (success) {
      if (m_log_process) {
        Debug.Log("cMo:\n" + m_cube_cMo[0] + " " + m_cube_cMo[1] + " " + m_cube_cMo[2]  + " " + m_cube_cMo[3] + "\n"
                           + m_cube_cMo[4] + " " + m_cube_cMo[5] + " " + m_cube_cMo[6]  + " " + m_cube_cMo[7] + "\n"
                           + m_cube_cMo[8] + " " + m_cube_cMo[9] + " " + m_cube_cMo[10] + " " + m_cube_cMo[11]);
        Debug.Log("Tracking process time: " + m_tracking_time[0] + " ms");
        m_log_process = false;
      }

      // Debug.Log("Num of Edges: "+ m_visible_edges_number[0]);

      // Draw lines that are visible and tracked currently: controlled by m_edges_number
      for (int i = 0; i < m_visible_edges_number[0]; i++) {
        // Scaling according to plane
        // Height of m_webCamTexture plane remains fixed (10 units) but width = 10*m_aspect_ratio
        // Debug.Log("Edge " + i + ": " + m_visible_edges_pointx[2 * i] + " , " + m_visible_edges_pointy[2 * i] + " - " + m_visible_edges_pointx[2 * i+1] + " , " + m_visible_edges_pointy[2 * i + 1]);
        float x1 = -10f * m_aspect_ratio * (m_visible_edges_pointx[2 * i] / m_webCamTexture.width - 1f / 2f);
        float x2 = -10f * m_aspect_ratio * (m_visible_edges_pointx[2 * i + 1] / m_webCamTexture.width - 1f / 2f);
        float y1 = -10f * (m_visible_edges_pointy[2 * i] / m_webCamTexture.height - 1f / 2f);
        float y2 = -10f * (m_visible_edges_pointy[2 * i + 1] / m_webCamTexture.height - 1f / 2f);

        // Draw visible edges
        m_line[i].SetActive(true);
        m_line[i].GetComponent<LineRenderer>().SetPosition(0, new Vector3(x1, y1, -9f));
        m_line[i].GetComponent<LineRenderer>().SetPosition(1, new Vector3(x2, y2, -9f));

        //Following line is for debugging purpose only: shows lines only in the 'Scene' view, not in 'Game' view:
        Debug.DrawLine(new Vector3(x1, y1, -19f), new Vector3(x2, y2, -19f));
      }
      // Hide non visible edges
      for (int i = m_visible_edges_number[0]; i < m_line.Length; i++) {
        m_line[i].SetActive(false);
      }
    }
    else {
      // When cube is not tracked hide all the edges
      for (int i = 0; i < m_line.Length; i++) {
        m_line[i].SetActive(false);
      }
    }
  }

  void OnApplicationQuit()
  {
    Visp_WrapperFreeMemory();
    Debug.Log("Application ending after " + Time.time + " seconds");
  }
}
