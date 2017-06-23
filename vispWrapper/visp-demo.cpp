#include "visp-demo.h"
extern "C" {

	// Defining VpImage globally
	vpImage<unsigned char> image;

	// Defining  blob
	vpDot2 blob;
	blob.setGraphics(true);
	blob.setGraphicsThickness(2);


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

	// Greyscale bitmap from Unity to vpImage
	void passFrame(unsigned char* const bitmap, int height, int width){
		// Resize frame according to Webcam input from Unity
		image.resize(height,width);

		// Grey Scale Image to be passed in the tracker pipeline
		image.bitmap = bitmap;
	}

	 void initBlobTracker(int blobCenterX, int blobCenterY, bool isClicked)
	 {
		 vpImagePoint germ(blobCenterX, blobCenterY);
		 if(isClicked) {
						 blob.initTracking(image, germ);
				}
		}

	 trackBlob(char* newBitmap, int height, int width)
	 {
	 }
}
