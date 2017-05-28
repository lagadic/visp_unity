#include "visp-demo.h"
extern "C" {
  double dot_prod(unsigned int* const A){
    //Defining (1 X 3) Row Vector
    vpRowVector r(3);
    r[0] = A[0];
    r[1] = A[1];
    r[2] = A[2];
    //Defining (3 X 1) Coulmn Vector
    vpColVector c(3);
    c[0] = A[3];
    c[1] = A[4];
    c[2] = A[5];
    //Dot Product
    double product = r * c;
    return product;
  }
}
