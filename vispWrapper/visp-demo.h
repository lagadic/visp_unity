// On Microsoft Windows, use dllexport to tag symbols.
# if defined(_WIN32) || defined(__CYGWIN__)
#   define VISP_WRAPPER_EXPORT __declspec(dllexport)
# else // On Linux
#   define VISP_WRAPPER_EXPORT
# endif // defined(_WIN32) || defined(__CYGWIN__)

#include <visp/vpMatrix.h>
#include <visp/vpMath.h>
#include <visp/vpTranslationVector.h>

#include <iostream>
#include <string>
#include <sstream>

using namespace std;
extern "C" {
  VISP_WRAPPER_EXPORT double dot_prod(unsigned int* const A);
}
