#define TESTDLL_API __declspec(dllexport)

#include <visp/vpMatrix.h>
#include <visp/vpMath.h>
#include <visp/vpTranslationVector.h>

#include <iostream>
#include <string>
#include <sstream>

using namespace std;
extern "C" {
	TESTDLL_API double dot_prod(unsigned int* const A);
}
