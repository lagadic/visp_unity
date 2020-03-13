#pragma once
#define TESTDLL_API __declspec(dllexport)

#include <visp3/visp_core.h> 
#include <visp3/io/vpImageIo.h>
#include <visp3/blob/vpDot2.h>
#include <visp3/detection/vpDetectorAprilTag.h>
#include <visp3/mbt/vpMbGenericTracker.h>
#include <visp3/detection/vpDetectorAprilTag.h>
#include <visp3/gui/vpDisplayGDI.h>

#include <iostream>
#include <string>
#include <sstream>
#include <fstream>
#include <ios>
#include "System.h"
#include <math.h>


using namespace std;
extern "C" {
	TESTDLL_API void Shutdown();
	TESTDLL_API void Init(const char* path_to_vocab, const char* path_to_cam_params, const int bitmap_height, const int bitmap_width);

	TESTDLL_API void PoseFromAprilTag(unsigned char* const bitmap,
										int bitmap_height, int bitmap_width,
										double cam_px, double cam_py,
										double cam_u0, double cam_v0,
									    double cam_kud, double cam_kdu,		
										double tag_size,
										double* tag_centre_image,
										double* camera_pose_U, double* camera_pose_V, double* camera_pose_W, double *camera_pose_T,
										double* tag_bb_height, double* tag_bb_width,
										double* tag_dimensions,
										int* tag_id,
										int* is_tag_detected);

	TESTDLL_API void PoseFromAprilTagWithSlam(unsigned char* const bitmap,
												int bitmap_height, int bitmap_width,
												double cam_px, double cam_py,
												double cam_u0, double cam_v0,
											    double cam_kud, double cam_kdu,
												double tag_size,
												double distance_to_tag_to_reinit,
												double* tag_centre_image,
												double* camera_pose_U, double* camera_pose_V, double* camera_pose_W, double *camera_pose_T,
												double* tag_bb_height, double* tag_bb_width,
												double* tag_dimensions,
												int* tag_id,
												int* is_tag_detected, double* distance, int* slam_tracking_state);

	vpHomogeneousMatrix PoseFromAprilTag_cMo(unsigned char* const bitmap,
												int bitmap_height, int bitmap_width,
												double cam_px, double cam_py,
												double cam_u0, double cam_v0,
										        double cam_kud, double cam_kdu,
												double tag_size,
												double* tag_centre_image,
												double* camera_pose_U, double* camera_pose_V, double* camera_pose_W, double *camera_pose_T,
												double* tag_bb_height, double* tag_bb_width,
												double* tag_dimensions,
												int* tag_id,
												int* is_tag_detected);

	unsigned char* const FlipBitmap(unsigned char* const bitmap, int height, int width);
	cv::Mat PoseFromSlam(int frame_count_tracking);
	int Mat2ViSP(const cv::Mat& mat_in, vpHomogeneousMatrix& visp_out);
}
