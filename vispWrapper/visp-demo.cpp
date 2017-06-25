#include "visp-demo.h"
extern "C" {

	// Declaring VpImage globally
	vpImage<unsigned char> image;
/*
	// Declaring blob
	vpDot2 blob;
	blob.setGraphics(true);
	blob.setGraphicsThickness(2);
*/
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

	void BlobTracker(unsigned char* const bitmap, int height, int width, int blobCenterX, int blobCenterY, bool isClicked, bool init_done, double cogX, double cogY)
	{
		// Declaring VpImage
		vpImage<unsigned char> img;

		// Resize frame according to Webcam input from Unity
		img.resize(height,width);

		// Grey Scale Image to be passed in the tracker pipeline
		img.bitmap = bitmap;

		// Declaring blob
		vpDot2 blob;
		blob.setGraphics(true);
		blob.setGraphicsThickness(2);
		// Define Blob center as a vpImagePoint
		vpImagePoint germ(blobCenterX, blobCenterY);

		if(!init_done)
		{
			if(isClicked) {
				blob.initTracking(img, germ);
				init_done = true;
				cogX = blobCenterX;
				cogY = blobCenterY;
			}
		}
		else
		{
			blob.track(img);
			vpImagePoint cog = d.getCog();
			cogX = cog.get_i;
			cogY = cog.get_j;
		}
	}
}
	/*
	void getBlobCoordinates(char* newBitmap, int height, int width)
	{
		blob.track(image);
		vpImagePoint cog = d.getCog();
		// update newBitmap
		// How to get the modified frame, without flush function. Are the changes done on variable image itself?
	}
	*/
