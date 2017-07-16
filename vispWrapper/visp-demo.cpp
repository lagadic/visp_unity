#include "visp-demo.h"
extern "C" {

	// Declaring VpImage
	vpImage<unsigned char> image;

	// Declaring blob
	vpDot2 blob;

	// Declaring initiallizing pixel
	vpImagePoint germ;

	// Declaring Center of Gravity
	vpImagePoint cog;

	vector<vpPoint> point;
  vpHomogeneousMatrix cMo;
	list<vpDot2> blob_list;
	vpCameraParameters cam;
  vpPose pose;

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

	void initBlobTracker(double getMouseX, double getMouseY, unsigned int* const init_done)
	{
		// Define Blob initial tracking pixel as a vpImagePoint
		germ.set_ij(getMouseX, getMouseY);

		//Initialize blob pixels
		blob.initTracking(image, germ);
		init_done[0] = 1;
	}

	void trackBlob()
	{
		blob.track(image);
	}

	void getBlobCoordinates(double* cogX, double* cogY, unsigned int* const init_done)
	{
		try {
			// Get the Center of Gravity of the tracked blob
			vpImagePoint cog = blob.getCog();
			cogX[0] = cog.get_i();
			cogY[0] = cog.get_j();
		}
		catch(...) {
			*init_done = 0;
		}
	}

	void initFourBlobTracker(unsigned int* const init_pose)
	{
		if (0) { // code used to learn the characteristics of a blob that we want to retrieve automatically
      // Learn the characteristics of the blob to auto detect
      blob.initTracking(image);
      blob.track(image);
    }
		// Set blob characteristics for the auto detection

		blob.setWidth(40);
    blob.setHeight(40);
    blob.setArea(1000);
    blob.setGrayLevelMin(0);
    blob.setGrayLevelMax(150);
    blob.setSizePrecision(0.65);
    blob.setEllipsoidShapePrecision(0.65);

		// Define the 3D model of a target defined by 4 blobs arranged as a square
		point.push_back( vpPoint(-0.06, -0.06, 0) );
    point.push_back( vpPoint( 0.06, -0.06, 0) );
    point.push_back( vpPoint( 0.06,  0.06, 0) );
    point.push_back( vpPoint(-0.06,  0.06, 0) );

		cam.initPersProjWithoutDistortion(840, 840, image.getWidth()/2, image.getHeight()/2);
		init_pose[0] = 1;
	}

	int getNumberOfBlobs()
	{
		blob.searchDotsInArea(image, 0, 0, image.getWidth(), image.getHeight(), blob_list);

		// Make a seprate track function that takes a list of blobs into consideration.
		for(std::list<vpDot2>::iterator it=blob_list.begin(); it != blob_list.end(); ++it) {
			(*it).track(image);
		}
		return blob_list.size();
	}

	void estimatePose(unsigned int* const init_pose)
	{
	  double x=0, y=0;
	  unsigned int i = 0;
	  for (std::list<vpDot2>::const_iterator it=blob_list.begin(); it != blob_list.end(); ++it) {
	    vpPixelMeterConversion::convertPoint(cam, (*it).getCog(), x, y);
	    point[i].set_x(x);
	    point[i].set_y(y);
	    pose.addPoint(point[i]);
	    i++;
	  }

	  if (init_pose[0] == 1) {
	    vpHomogeneousMatrix cMo_dem;
	    vpHomogeneousMatrix cMo_lag;
	    pose.computePose(vpPose::DEMENTHON, cMo_dem);
	    pose.computePose(vpPose::LAGRANGE, cMo_lag);
	    double residual_dem = pose.computeResidual(cMo_dem);
	    double residual_lag = pose.computeResidual(cMo_lag);
	    if (residual_dem < residual_lag)
	      cMo = cMo_dem;
	    else
	      cMo = cMo_lag;
	  }
	  pose.computePose(vpPose::VIRTUAL_VS, cMo);
	}

}
