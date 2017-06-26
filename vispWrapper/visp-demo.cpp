#include "visp-demo.h"
extern "C" {

	// Declaring VpImage globally
	vpImage<unsigned char> image;

	// Declaring blob
	vpDot2 blob;

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

	void initBlobTracker(int getMouseX, int getMouseY, bool isClicked, bool init_done)
	{
		blob.setGraphics(true);
		blob.setGraphicsThickness(2);

		// Define Blob initial tracking pixel as a vpImagePoint
		vpImagePoint germ(getMouseX, getMouseY);

		//Initialize blob pixel
		if(isClicked) {
			blob.initTracking(img, germ);
			init_done = true;
		}

	}

	void getBlobCoordinates(double cogX, double cogY)
	{
		blob.track(image);
		vpImagePoint cog = d.getCog();
		cogX = cog.get_i;
		cogY = cog.get_j;
	}
}
