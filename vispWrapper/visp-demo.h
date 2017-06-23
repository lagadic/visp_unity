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

using namespace std;
extern "C" {
	VISP_EXPORT double dot_prod(unsigned int* const A);
	VISP_EXPORT void passFrame(unsigned char* const bitmap, int height, int width);
	VISP_EXPORT void initBlobTracker(int blobCenterX, int blobCenterY, bool isClicked, bool init_done);
	VISP_EXPORT void trackBlob(char* newBitmap, int height, int width);
}
