#pragma once

#define TESTDLL_API __declspec(dllexport)

#include <visp3/core/vpConfig.h>
#include <visp3/core/vpMatrix.h>
#include <visp3/core/vpMath.h>
#include <visp3/core/vpTranslationVector.h>
#include <visp3/core/vpImage.h>
#include <visp3/io/vpImageIo.h>
#include <visp3/core/vpImageConvert.h>
#include <visp3/blob/vpDot2.h>
#include <visp3/core/vpImagePoint.h>

#include <iostream>
#include <string>
#include <sstream>

#include <visp3/detection/vpDetectorAprilTag.h>
#include <visp3/io/vpImageIo.h>
#include <visp3/core/vpPoseVector.h>
#include <visp3/core/vpRect.h>


using namespace std;
extern "C" {
	TESTDLL_API bool ApriltagDetect(unsigned char* const bitmap, int height, int width);
	TESTDLL_API double GetHorizontalAngle();
	TESTDLL_API double GetVerticalAngle();
	TESTDLL_API void CenterCoord(unsigned char* const bitmap, int height, int width, double* array);
	TESTDLL_API void ApriltagPoseHomogenous(unsigned char* const bitmap, int height, int width, double* arrayU, double* arrayV, double *arrayT);

	TESTDLL_API void ApriltagPoseTheta(unsigned char* const bitmap, int height, int width, double* array);
	TESTDLL_API void ApriltagPoseTranslation(unsigned char* const bitmap, int height, int width, double* array);

}