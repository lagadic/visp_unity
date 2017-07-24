#include <visp3/core/vpConfig.h>
#include <visp3/core/vpMatrix.h>
#include <visp3/core/vpMath.h>
#include <visp3/core/vpTranslationVector.h>
#include <visp3/core/vpImage.h>
#include <visp3/io/vpImageIo.h>
#include <visp3/core/vpImageConvert.h>
#include <visp3/blob/vpDot2.h>
#include <visp3/core/vpImagePoint.h>
#include <visp3/core/vpPixelMeterConversion.h>
#include <visp3/vision/vpPose.h>

#include <iostream>
#include <string>
#include <sstream>

using namespace std;
extern "C" {
	VISP_EXPORT double dot_prod(unsigned int* const A);
	VISP_EXPORT void passFrame(unsigned char* const bitmap, int height, int width);
	VISP_EXPORT void initBlobTracker(double getMouseX, double getMouseY, unsigned int* const init_done);
	VISP_EXPORT void getBlobCoordinates(double* cogX, double* cogY, unsigned int* const init_done);
	VISP_EXPORT void trackBlob();
	VISP_EXPORT void getNumberOfBlobs(unsigned int* const numOfBlobs);
	VISP_EXPORT void estimatePose(unsigned int* const init_pose, double* cMo_pass);
	VISP_EXPORT void initFourBlobTracker(unsigned int* const init_pose);
}
