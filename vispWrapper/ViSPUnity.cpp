#include "ViSPUnity.h"
#include <visp3/core/vpArray2D.h>
#include <visp3/core/vpRect.h>

extern "C" {

	vpImage<unsigned char> I;
	vpDetectorAprilTag detector(vpDetectorAprilTag::TAG_36h11);
	std::vector<vpHomogeneousMatrix> cMo;
	double tagSize = 0.053;
	vpCameraParameters cam;

	vpArray2D<double> v, u, t;
	double* theta = new double[3];
	double* translation = new double[3];
	double* h = new double[1];
	double* w = new double[1];
	double* apr = new double[6];

	void AprilTagFunctionsCombined(unsigned char* const bitmap, int height, int width, double cam_px, double cam_py, double cam_u0, double cam_v0, double* array, double* arrayU, double* arrayV, double *arrayT, double* h, double* w, double* apr) {
		cam.initPersProjWithoutDistortion(cam_px, cam_py, cam_u0, cam_v0);
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

			//COORDINATES OF APRILTAG
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

			//SIZE OF BOUNDING-BOX
			//vpRect bbox = detector.getBBox(0);
			h[0] = bbox.getHeight();
			w[0] = bbox.getWidth();

			//SIZE OF POLYGON
			vpPolygon polygon(detector.getPolygon(0));

			vector <vpImagePoint> corners = polygon.getCorners();
			apr[0] = vpImagePoint::distance(corners[0], corners[1]); // side1
			apr[1] = vpImagePoint::distance(corners[1], corners[2]); // side2
			apr[2] = vpImagePoint::distance(corners[2], corners[3]); // side3
			apr[3] = vpImagePoint::distance(corners[3], corners[0]); // side4
			apr[4] = vpImagePoint::distance(corners[0], corners[2]); // diagonal1
			apr[5] = vpImagePoint::distance(corners[1], corners[3]); // diagonal2

			//ROTATION OF APRILTAG
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
			array = 0;

			h[0] = -1;
			w[0] = -1;

			arrayU = 0;
			arrayV = 0;
			arrayT = 0;
		}
	}
}
