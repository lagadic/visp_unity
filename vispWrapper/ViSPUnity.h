#pragma once
// #define TESTDLL_API __declspec(dllexport)

#include <visp3/visp_core.h> 
#include <visp3/io/vpImageIo.h>
#include <visp3/blob/vpDot2.h>
#include <visp3/detection/vpDetectorAprilTag.h>
#include <visp3/mbt/vpMbGenericTracker.h>

#include <iostream>
#include <string>
#include <sstream>
#include <fstream>
#include <ios>
#include "System.h"

using namespace std;
extern "C" {

	VISP_EXPORT void createCaoFile(double cubeEdgeSize);
	VISP_EXPORT void AprilTagMBT(unsigned char* const bitmap, int height, int width, 
		double *pointx, double *pointy, 
		double* kltX, double* kltY, int* kltNumber, 
		int t, int e, int* flag_state, int* nEdges);
	VISP_EXPORT void InitMBT(double cam_px, double cam_py, double cam_u0, double cam_v0, int t);
	VISP_EXPORT void AprilTagFunctionsCombined(unsigned char* const bitmap, int height, int width, 
		double cam_px, double cam_py, 
		double cam_u0, double cam_v0, 
		double* array, 
		double* arrayU, double* arrayV, double* arrayW, double *arrayT,
		double* h, double* w, 
		double* apr,
		int* tag_id);
}
