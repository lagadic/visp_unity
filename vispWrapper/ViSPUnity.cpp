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
 * Unity plugin that wraps some ViSP functionalities.
 *
 *****************************************************************************/
#include "ViSPUnity.h"

#include <visp3/gui/vpDisplayGDI.h>
#include <visp3/gui/vpDisplayOpenCV.h>
#include <visp3/gui/vpDisplayX.h>

/*!
  \file
  \brief ViSPUnity plugin functions definition.
 */

extern "C" {

/*!
 * Global variables for debug
 */
#if (VISP_CXX_STANDARD >= VISP_CXX_STANDARD_11)
static vpDisplay *m_debug_display = nullptr; //!< Display associated to internal image m_I.
#else
static vpDisplay *m_debug_display = NULL; //!< Display associated to image m_I.
#endif
static bool m_debug_enable_display = false; //!< Flag used to enable/disable display associated to internal image m_I.
static bool m_debug_display_is_initialized = false; //!< Flag used to know if display associated to internal image m_I is initialized.

/*!
 * Global variables that are common.
 */
static vpImage<unsigned char> m_I; //!< Internal image updated using Visp_ImageUchar_SetFromColor32Array().
static vpCameraParameters m_cam; //!< Internal camera parameters updated using Visp_CameraParameters_Init().

/*!
 * Global variables for vpDetectorAprilTag
 */
static vpDetectorAprilTag m_detector; //!< Internal AprilTag detector instance initialized using Visp_DetectorAprilTag_Init().
static float m_detector_quad_decimate = 1.0; //!< Internal parameter associated to AprilTag detector instance modified using Visp_DetectorAprilTag_Init().
static int m_detector_nthreads = 1; //!< Internal parameter associated to AprilTag detector instance modified using Visp_DetectorAprilTag_Init().

/*!
 * Global variables for vpMbGenericTracker
 */
typedef enum {
  state_detection, //!< Tracker is in detection state until an AprilTag is detected. This state can also be reached when tracking fails.
  state_tracking, //!< Tracker is in tracking state when AprilTag pose allows to initialize the tracker and when tracking succeed.
} state_t;

static vpMbGenericTracker m_tracker; //!< Internal generic based-model tracker instance initialized using Visp_MbGenericTracker_Init().
static double m_projection_error_threshold = 40.; //!< Internal parameter associated to generic based-model tracker instance and updated using Visp_MbGenericTracker_Init().
static state_t m_state = state_detection; //!< Internal generic based-model tracker state updated during tracking using Visp_MbGenericTracker_Process().

void Visp_EnableDisplayForDebug(bool enable_display)
{
  m_debug_enable_display = enable_display;
}

void Visp_WrapperFreeMemory()
{
  if (m_debug_display) {
    delete m_debug_display;
    m_debug_enable_display = false;
    m_debug_display_is_initialized = false;
#if (VISP_CXX_STANDARD >= VISP_CXX_STANDARD_11)
    m_debug_display = nullptr;
#else
    m_debug_display = NULL;
#endif
  }
}

/*!
 * Set vpImage from Unity Color32 array image.
 * \param bitmap : Bitmap color 32 array that contains the color RGBA [height x width] image.
 * \param height : Image height.
 * \param width : Image width.
 */
void Visp_ImageUchar_SetFromColor32Array(unsigned char *bitmap, int height, int width)
{
  m_I.resize(static_cast<unsigned int>(height), static_cast<unsigned int>(width));
  vpImageConvert::RGBaToGrey(bitmap, m_I.bitmap, static_cast<unsigned int>(width * height));
  vpImageTools::flip(m_I);

  if (m_debug_enable_display && ! m_debug_display_is_initialized) {
#if defined(VISP_HAVE_X11)
    m_debug_display = new vpDisplayX(m_I);
    m_debug_display_is_initialized = true;
#elif defined VISP_HAVE_GDI
    m_debug_display = new vpDisplayGDI(m_I);
    m_debug_display_is_initialized = true;
#elif defined VISP_HAVE_OPENCV
    m_debug_display = new vpDisplayOpenCV(m_I);
    m_debug_display_is_initialized = true;
#endif
  }
}

void Visp_MbGenericTracker_SetFeatureType(int feature_type)
{
  if (feature_type == 0)
    m_tracker.setTrackerType(vpMbGenericTracker::EDGE_TRACKER);
#ifdef VISP_HAVE_OPENCV
  else if (feature_type == 1)
    m_tracker.setTrackerType(vpMbGenericTracker::EDGE_TRACKER | vpMbGenericTracker::KLT_TRACKER);
#endif
}

void Visp_MbGenericTracker_SetMovingEdgesSettings(int range, double sample_step)
{
  vpMe me;
  me.setMaskSize(5);
  me.setMaskNumber(180);
  me.setRange(static_cast<unsigned int>(range));
  me.setThreshold(10000);
  me.setMu1(0.5);
  me.setMu2(0.5);
  me.setSampleStep(sample_step);
  m_tracker.setMovingEdge(me);
}

void Visp_MbGenericTracker_SetKeypointSettings(double quality, int mask_border)
{
  if (m_tracker.getTrackerType() & vpMbGenericTracker::KLT_TRACKER) {
    vpKltOpencv klt_settings;
    klt_settings.setMaxFeatures(300);
    klt_settings.setWindowSize(5);
    klt_settings.setQuality(quality);
    klt_settings.setMinDistance(8);
    klt_settings.setHarrisFreeParameter(0.01);
    klt_settings.setBlockSize(3);
    klt_settings.setPyramidLevels(3);
    m_tracker.setKltOpencv(klt_settings);
    m_tracker.setKltMaskBorder(static_cast<unsigned int>(mask_border));
  }
}

void Visp_MbGenericTracker_Init(double angle_appear, double angle_disappear, double projection_error_threshold)
{
  m_projection_error_threshold = projection_error_threshold;
  // camera calibration params
  m_tracker.setCameraParameters(m_cam);

  // model definition
  m_tracker.loadModel("cube.cao");
  m_tracker.setDisplayFeatures(m_debug_enable_display);
  m_tracker.setAngleAppear(vpMath::rad(angle_appear));
  m_tracker.setAngleDisappear(vpMath::rad(angle_disappear));
  m_tracker.setProjectionErrorComputation(true);

  m_state = state_detection;
}

bool Visp_MbGenericTracker_Process(double tag_size,
                                  float *visible_edges_pointx, float *visible_edges_pointy, int *visible_edges_number,
                                  float *cube_cMo, double *tracking_time)
{
  double t_start = vpTime::measureTimeMs();

  vpHomogeneousMatrix cMo;

  // If the image contains an aprilTag we pick the first one
  unsigned int tag_id = 0;

  if (m_debug_enable_display && m_debug_display_is_initialized) {
    vpDisplay::display(m_I);
  }

  if (m_state == state_detection) {
    std::vector<vpHomogeneousMatrix> cMo_vec;

    // Detection
    bool tag_detected = m_detector.detect(m_I, tag_size, m_cam, cMo_vec);
    if (tag_detected && m_detector.getNbObjects() > 0) { // if tag detected, we pick the first one
      cMo = cMo_vec[tag_id];
      m_state = state_tracking;
    }

    // Initialize the tracker with the result of the detection
    if (m_state == state_tracking) {
      m_tracker.initFromPose(m_I, cMo);
    }
  }

  if (m_state == state_tracking) {
    try {
      m_tracker.track(m_I);

      m_tracker.getPose(cMo);

      int visible_edges_counter = 0; // counter of the number of edges actually visible and currently tracked

      // Get the lines currently tracked of the model
      std::list<vpMbtDistanceLine *> edges;
      m_tracker.getLline("Camera", edges, 0);
      int i = 0;

      //*nEdges = edges.size();
      for (std::list<vpMbtDistanceLine *>::const_iterator it = edges.begin(); it != edges.end(); ++it) {

        // Part of the functionality from the display() function is implemented from the following source:
        // http://visp-doc.inria.fr/doxygen/visp-daily/vpMbtDistanceLine_8cpp_source.html
        if ((*it)->isvisible && (*it)->isTracked()) {
          visible_edges_counter ++; // increment count of number of edges that are visible and being tracked with visibility
          vpPoint *P1 = (*it)->p1;
          vpPoint *P2 = (*it)->p2;
          P1->project(cMo);
          P2->project(cMo);
          vpImagePoint iP1, iP2;
          vpMeterPixelConversion::convertPoint(m_cam, P1->get_x(), P1->get_y(), iP1);
          vpMeterPixelConversion::convertPoint(m_cam, P2->get_x(), P2->get_y(), iP2);
          visible_edges_pointx[i] = static_cast<float>(iP1.get_u());
          visible_edges_pointy[i] = static_cast<float>(iP1.get_v());
          i ++;
          visible_edges_pointx[i] = static_cast<float>(iP2.get_u());
          visible_edges_pointy[i] = static_cast<float>(iP2.get_v());
          i ++;
        }
      }

      // Update number of visible edges
      *visible_edges_number = visible_edges_counter;

      if (m_debug_enable_display && m_debug_display_is_initialized) {
        m_tracker.display(m_I, cMo, m_cam, vpColor::red, 2);
        vpDisplay::displayFrame(m_I, cMo, m_cam, tag_size / 2, vpColor::none, 3);
      }

      // Detect tracking error
      double projection_error = m_tracker.computeCurrentProjectionError(m_I, cMo, m_cam);
      if (m_debug_enable_display && m_debug_display_is_initialized) {
        std::stringstream ss;
        ss << "Projection error: " << projection_error << std::endl;
        vpDisplay::displayText(m_I, 40, 20, ss.str(), vpColor::red);
      }
      if (projection_error > m_projection_error_threshold) {
        m_state = state_detection;
      }
      else {
        m_state = state_tracking;
      }
    }
    catch (...) {
      m_state = state_detection;
    }
  }

  // Update output pose array
  for (unsigned int i = 0; i < 16; i++) {
    cube_cMo[i] = static_cast<float>(cMo.data[i]);
  }
  *tracking_time = vpTime::measureTimeMs() - t_start;

  if (m_debug_enable_display && m_debug_display_is_initialized) {
    std::stringstream ss;
    ss << "Loop time: " << *tracking_time << std::endl;
    vpDisplay::displayText(m_I, 20, 20, ss.str(), vpColor::red);
    vpDisplay::flush(m_I);
  }

  return (m_state == state_tracking ? true : false);
}

void Visp_MbGenericTracker_CreateCaoFile(double cube_edge_size)
{
  std::ofstream fileStream;
  fileStream.open("cube.cao", std::ofstream::out | std::ofstream::trunc);
  fileStream << "V1\n";
  fileStream << "# 3D Points\n";
  fileStream << "8                  # Number of points\n";
  fileStream <<  cube_edge_size / 2 << " " <<  cube_edge_size / 2 << " " << 0 << "    # Point 0: (X, Y, Z)\n";
  fileStream <<  cube_edge_size / 2 << " " << -cube_edge_size / 2 << " " << 0 << "    # Point 1\n";
  fileStream << -cube_edge_size / 2 << " " << -cube_edge_size / 2 << " " << 0 << "    # Point 2\n";
  fileStream << -cube_edge_size / 2 << " " <<  cube_edge_size / 2 << " " << 0 << "    # Point 3\n";
  fileStream << -cube_edge_size / 2 << " " <<  cube_edge_size / 2 << " " << -cube_edge_size << "    # Point 4\n";
  fileStream << -cube_edge_size / 2 << " " << -cube_edge_size / 2 << " " << -cube_edge_size << "    # Point 5\n";
  fileStream <<  cube_edge_size / 2 << " " << -cube_edge_size / 2 << " " << -cube_edge_size << "    # Point 6\n";
  fileStream <<  cube_edge_size / 2 << " " <<  cube_edge_size / 2 << " " << -cube_edge_size << "    # Point 7\n";
  fileStream << "# 3D Lines\n";
  fileStream << "0                  # Number of lines\n";
  fileStream << "# Faces from 3D lines\n";
  fileStream << "0                  # Number of faces\n";
  fileStream << "# Faces from 3D points\n";
  fileStream << "6                  # Number of faces\n";
  fileStream << "4 0 3 2 1          # Face 0: [number of points] [index of the 3D points]...\n";
  fileStream << "4 1 2 5 6\n";
  fileStream << "4 4 7 6 5\n";
  fileStream << "4 0 7 4 3\n";
  fileStream << "4 5 2 3 4\n";
  fileStream << "4 0 1 6 7          # Face 5\n";
  fileStream << "# 3D cylinders\n";
  fileStream << "0                  # Number of cylinders\n";
  fileStream << "# 3D circles\n";
  fileStream << "0                  # Number of circles\n";
  fileStream.close();
}

void Visp_CameraParameters_Init(double cam_px, double cam_py, double cam_u0, double cam_v0)
{
  m_cam.initPersProjWithoutDistortion(cam_px, cam_py, cam_u0, cam_v0);
}

void Visp_DetectorAprilTag_Init(float quad_decimate, int nthreads)
{
  // Initialize AprilTag detector
  m_detector_quad_decimate = quad_decimate;
  m_detector_nthreads = nthreads;
  m_detector.setAprilTagFamily(vpDetectorAprilTag::TAG_36h11);
  m_detector.setAprilTagQuadDecimate(m_detector_quad_decimate);
  m_detector.setAprilTagNbThreads(m_detector_nthreads);
  m_detector.setDisplayTag(m_debug_enable_display, vpColor::none, 3);
}

bool Visp_DetectorAprilTag_Process(double tag_size, float *tag_cog, float *tag_length, float *tag_cMo, double *detection_time)
{
  double t_start = vpTime::measureTimeMs();

  if (m_debug_enable_display && m_debug_display_is_initialized) {
    vpDisplay::display(m_I);
  }
  // Detection
  std::vector<vpHomogeneousMatrix> cMo_v;
  bool tag_detected = m_detector.detect(m_I, tag_size, m_cam, cMo_v);

  if (tag_detected) {
    // If the image contains an aprilTag we pick the first one
    unsigned int tag_id = 0;
    // Tag characteristics
    vpImagePoint cog = m_detector.getCog(tag_id);
    tag_cog[0] = static_cast<float>(cog.get_u());
    tag_cog[1] = static_cast<float>(cog.get_v());

    std::vector <vpImagePoint> corners = m_detector.getPolygon(tag_id);
    tag_length[0] = static_cast<float>(vpImagePoint::distance(corners[0], corners[1])); // side1
    tag_length[1] = static_cast<float>(vpImagePoint::distance(corners[1], corners[2])); // side2
    tag_length[2] = static_cast<float>(vpImagePoint::distance(corners[2], corners[3])); // side3
    tag_length[3] = static_cast<float>(vpImagePoint::distance(corners[3], corners[0])); // side4
    tag_length[4] = static_cast<float>(vpImagePoint::distance(corners[0], corners[2])); // diagonal1
    tag_length[5] = static_cast<float>(vpImagePoint::distance(corners[1], corners[3])); // diagonal2

    // Update output pose array
    for (unsigned int i = 0; i < 16; i++) {
      tag_cMo[i] = static_cast<float>(cMo_v[tag_id].data[i]);
    }

    if (m_debug_enable_display && m_debug_display_is_initialized) {
      vpDisplay::displayFrame(m_I, cMo_v[tag_id], m_cam, tag_size / 2, vpColor::none, 3);
    }
  }

  *detection_time = vpTime::measureTimeMs() - t_start;

  if (m_debug_enable_display && m_debug_display_is_initialized) {
    std::stringstream ss;
    ss << "Loop time: " << *detection_time << std::endl;
    vpDisplay::displayText(m_I, 20, 20, ss.str(), vpColor::red);
    vpDisplay::flush(m_I);
  }

  return tag_detected;
}
}
