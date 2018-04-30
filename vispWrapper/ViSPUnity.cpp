#include "ViSPUnity.h"
#include <visp3/core/vpArray2D.h>
#include <visp3/core/vpRect.h>

extern "C" {

	vpImage<unsigned char> I;
	vpDetectorAprilTag detector(vpDetectorAprilTag::TAG_36h11);
	std::vector<vpHomogeneousMatrix> cMo;
	double tagSize = 0.053;
	vpCameraParameters cam;

	vpArray2D<double> v,u,t;
	double* theta = new double[3];
	double* translation = new double[3];

	// Functions present and their uses:

	// 1. GetVerticalAngle()		    	Returns vertical   field of view angle for camera (with given parameters)
	// 2. GetHorizontalAngle()		      	Returns horizontal field of view angle for camera (with given parameters)
	// 3. ApriltagDetect(~)					Returns true/false depending on whether aprilTag was found or not
	// 4. ApriltagPoseHomogenous(~)			Returns the pose of aprilTag (used for rotating the cube)
	// 5. CenterCoord(~)					Returns the coordinates of aprilTag in the image
	// 6. ApriltagPoseTheta(~)				[used earlier for getting theta-u-vector]
	// 7. ApriltagPoseTranslation(~)		[used earlier for getting translation-vector]

	//Function that returns the Vertical FOV after computation
	double GetVerticalAngle() {

		//The parameters given are specific to webcam, they need to be changed according to the camera being used
		cam.initPersProjWithoutDistortion(1131.561907, 1085.157822, 588.2376812, 191.1328903);

		//Since the image was 1280*720 (16:9)
		cam.computeFov(1280, 720);
		return cam.getVerticalFovAngle();
	}


	//Function that returns the Horizonatal FOV after computation
	double GetHorizontalAngle() {

		//The parameters given are specific to webcam, they need to be changed accordingly
		cam.initPersProjWithoutDistortion(1131.561907, 1085.157822, 588.2376812, 191.1328903);

		//Since the image was 1280*720 (16:9)
		cam.computeFov(1280, 720);
		return cam.getHorizontalFovAngle();
	}


	//Function that returns true if there is an aprilTag in the image
	bool ApriltagDetect(unsigned char* const bitmap, int height, int width) {
		cam.initPersProjWithoutDistortion(1131.561907, 1085.157822, 588.2376812, 191.1328903);

		//Flipping the bitmap horizontally-this step is important: this takes care of the right and left handed systems 

		for (int r = 0; r < height; r++) {
			for (int c = 0; c < width / 2; c++) {
				unsigned char temp = bitmap[r*width + c];
				bitmap[r*width + c] = bitmap[r*width + width - c - 1];
				bitmap[r*width + width - c - 1] = temp;
			}
		}

		I.resize(height, width);
		I.bitmap = bitmap;

		bool check = detector.detect(I, tagSize, cam, cMo);
		return check;
	}

	// Added rotation functionality using homogeneous matrices
	//Function that stores the ThetaUVector in array after computation
	void ApriltagPoseHomogenous(unsigned char* const bitmap, int height, int width, double* arrayU, double* arrayV, double *arrayT) {
		cam.initPersProjWithoutDistortion(1131.561907, 1085.157822, 588.2376812, 191.1328903);

		//The following loop flips the bitmap
		for (int r = 0; r < height; r++) {
			for (int c = 0; c < width / 2; c++) {
				unsigned char temp = bitmap[r*width + c];
				bitmap[r*width + c] = bitmap[r*width + width - c - 1];
				bitmap[r*width + width - c - 1] = temp;
			}
		}

		I.resize(height, width);
		I.bitmap = bitmap;

		bool check = detector.detect(I, tagSize, cam, cMo);

		//If the image contains aprilTag
		if (check) {

			u = cMo[0].getCol(0);
			v = cMo[0].getCol(1);
			t = cMo[0].getCol(3);

			for (size_t i = 0; i < u.size(); i++)
			{
				arrayU[i] = u.data[i];
				arrayV[i] = v.data[i];
				arrayT[i] = t.data[i];
			}
		}
		
		else {
			arrayU = 0;
			arrayV = 0;
			arrayT = 0;
		}
	}

	//Function for getting coordinates of aprilTag
	void CenterCoord(unsigned char* const bitmap, int height, int width, double* array) {
		cam.initPersProjWithoutDistortion(1131.561907, 1085.157822, 588.2376812, 191.1328903);
		
		//The following loop flips the bitmap
		for (int r = 0; r < height; r++) {
			for (int c = 0; c < width / 2; c++) {
				unsigned char temp = bitmap[r*width + c];
				bitmap[r*width + c] = bitmap[r*width + width - c - 1];
				bitmap[r*width + width - c - 1] = temp;
			}
		}

		I.resize(height, width);
		I.bitmap = bitmap;		

		bool check = detector.detect(I, tagSize, cam, cMo);

		if (check) {
			vpRect bbox = detector.getBBox(0);
			double* coord = new double[4];
			vpImagePoint X = bbox.getCenter();
			coord[0] = X.get_i();
			coord[1] = X.get_j();
			coord[2] = X.get_u();
			coord[3] = X.get_v();

			for (size_t i = 0; i < 3; i++)
			{
				array[i] = coord[i];
			}
		}
		else
			array = 0;
	}

	//Function that stores the ThetaUVector in array after computation
	void ApriltagPoseTheta(unsigned char* const bitmap, int height, int width, double* array) {
		cam.initPersProjWithoutDistortion(1131.561907, 1085.157822, 588.2376812, 191.1328903);

		//The following loop flips the bitmap
		for (int r = 0; r < height; r++) {
			for (int c = 0; c < width / 2; c++) {
				unsigned char temp = bitmap[r*width + c];
				bitmap[r*width + c] = bitmap[r*width + width - c - 1];
				bitmap[r*width + width - c - 1] = temp;
			}
		}

		I.resize(height, width);
		I.bitmap = bitmap;

		bool check = detector.detect(I, tagSize, cam, cMo);

		//If the image contains aprilTag
		if (check) {

			//Store the theta vector in v
			v = cMo[0].getThetaUVector();

			//Get the theta vector from v and store it in array
			for (size_t i = 0; i < 3; i++)
			{
				theta[i] = v.data[i];
				array[i] = theta[i];
			}
		}
		else
			array = 0;
	}

	//Function that stores the TranslationVector in array after computation
	void ApriltagPoseTranslation(unsigned char* const bitmap, int height, int width, double* array) {
		cam.initPersProjWithoutDistortion(1131.561907, 1085.157822, 588.2376812, 191.1328903);

		//The following loop flips the bitmap
		for (int r = 0; r < height; r++) {
			for (int c = 0; c < width / 2; c++) {
				unsigned char temp = bitmap[r*width + c];
				bitmap[r*width + c] = bitmap[r*width + width - c - 1];
				bitmap[r*width + width - c - 1] = temp;
			}
		}

		I.resize(height, width);
		I.bitmap = bitmap;

		bool check = detector.detect(I, tagSize, cam, cMo);

		//If the image contains aprilTag
		if (check) {

			//Store the translation vector in v
			v = cMo[0].getTranslationVector();

			//Get the translation vector from v and store it in array				
			for (size_t i = 0; i < 3; i++)
			{
				translation[i] = v.data[i];
				array[i] = translation[i];
			}
		}
		else
			array = 0;
	}


}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               