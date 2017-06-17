#define TESTDLL_API __declspec(dllexport)

#include <visp/vpMatrix.h>
#include <visp/vpMath.h>
#include <visp/vpTranslationVector.h>

#include <visp3/core/vpImage.h>
#include <visp3/core/vpTime.h>
#include <visp3/io/vpImageIo.h>
#include <visp/vpImageConvert.h>

#include <iostream>
#include <string>
#include <sstream>

using namespace std;
extern "C" {
	TESTDLL_API double dot_prod(unsigned int* const A);
	TESTDLL_API void passFrame(unsigned char* const bitmap, int height, int width);
}
