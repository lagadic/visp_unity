#include "ViSPUnity.h"

extern "C" {

	vpImage<unsigned char> I;
	vpDetectorAprilTag detector(vpDetectorAprilTag::TAG_36h11);
	vpHomogeneousMatrix cMo;
	vpCameraParameters cam;
	vpMbGenericTracker tracker;

	vpArray2D<double> v, u, ww, t;
	double* theta = new double[3];
	double* translation = new double[3];
	double* h = new double[1];
	double* w = new double[1];
	double* apr = new double[6];
	//double tagSize = 0.053;
	//double tagSize = 0.06;
	//double tagSize = 0.04;
	//double tagSize = 0.076;
	
	int* flag_state = new int[1];

	typedef enum {
		state_detection,
		state_tracking,
		state_quit
	} state_t;

	state_t state;

	int opt_device = 0;
	vpDetectorAprilTag::vpAprilTagFamily opt_tag_family = vpDetectorAprilTag::TAG_36h11;
	vpDetectorAprilTag::vpPoseEstimationMethod pose_est_meth = vpDetectorAprilTag::vpPoseEstimationMethod::BEST_RESIDUAL_VIRTUAL_VS;

	double opt_tag_size = 0.08;
	float opt_quad_decimate = 1.0;
	int opt_nthreads = 1;
	std::string opt_intrinsic_file = "";
	std::string opt_camera_name = "cam1";
	double opt_cube_size = 0.125; // 12.5cm by default: also sent from unity for creating cao file
	double projection_error_threshold = 40.;

	//Function called for detection only
	state_t detectAprilTag(const vpImage<unsigned char> &I, vpDetectorAprilTag &detector,
		double tagSize, const vpCameraParameters &cam, vpHomogeneousMatrix &cMo)
	{
		std::vector<vpHomogeneousMatrix> cMo_vec;

		// Detection
		bool ret = detector.detect(I, tagSize, cam, cMo_vec);
		if (ret && detector.getNbObjects() > 0) { // if tag detected, we pick the first one
			cMo = cMo_vec[0];
			return state_tracking;
		}
		return state_detection;
	}


	void InitMBT(double cam_px, double cam_py, double cam_u0, double cam_v0, int t) {
		cam.initPersProjWithoutDistortion(cam_px, cam_py, cam_u0, cam_v0);

		// Initialize AprilTag detector
		//detector.set (opt_tag_family);
		detector.setAprilTagQuadDecimate(opt_quad_decimate);
		detector.setAprilTagNbThreads(opt_nthreads);
		detector.setAprilTagPoseEstimationMethod(pose_est_meth);

		// Prepare MBT
		if (t == 0)
			tracker.setTrackerType(vpMbGenericTracker::EDGE_TRACKER);

		if (t == 1)
			tracker.setTrackerType(vpMbGenericTracker::EDGE_TRACKER | vpMbGenericTracker::KLT_TRACKER);

		tracker.getCameraParameters(cam);
		/*bool displayFullModel = false;*/

		// edges
		vpMe me;
		me.setMaskSize(5);
		me.setMaskNumber(180);
		me.setRange(12);
		me.setThreshold(10000);
		me.setMu1(0.5);
		me.setMu2(0.5);
		me.setSampleStep(4);
		tracker.setMovingEdge(me);

		if (t == 1) {
			//textures
			vpKltOpencv klt_settings;
			if (t == 1) {
				klt_settings.setMaxFeatures(300);
				klt_settings.setWindowSize(5);
				klt_settings.setQuality(0.015);
				klt_settings.setMinDistance(8);
				klt_settings.setHarrisFreeParameter(0.01);
				klt_settings.setBlockSize(3);
				klt_settings.setPyramidLevels(3);
				tracker.setKltOpencv(klt_settings);
				tracker.setKltMaskBorder(5);
			}
		}
		// camera calibration params
		tracker.setCameraParameters(cam);

		// model definition
		tracker.loadModel("cube.cao");
		tracker.setDisplayFeatures(true);
		tracker.setAngleAppear(vpMath::rad(70));
		tracker.setAngleDisappear(vpMath::rad(80));

		state = state_detection;
	}

	void AprilTagMBT(unsigned char* const bitmap, int height, int width, 
		double* pointx, double* pointy, 
		double* kltX, double* kltY, int* kltNumber, 
		int t, int e, int* flag_state, int *nEdges) {

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

		if (state == state_detection) {
			state = detectAprilTag(I, detector, opt_tag_size, cam, cMo);
			*flag_state = 0;

			// Initialize the tracker with the result of the detection
			if (state == state_tracking) {
				tracker.initFromPose(I, cMo);
				*flag_state = 1;
			}
		}

		if (state == state_tracking) {
			//state = track(I, tracker, opt_projection_error_threshold, opt_camera_name, cMo);
			try {
				tracker.track(I);

				// get the lines currently tracked of the model
				std::list<vpMbtDistanceLine *> edges;
				tracker.getLline("Camera", edges, 0);
				int i = 0;
				*flag_state = 1;
				//*nEdges = edges.size();
				*nEdges = 0; // counter of the number of edges actually visible and currently tracked
				for (std::list<vpMbtDistanceLine *>::const_iterator it = edges.begin(); it != edges.end(); ++it) {

					// Part of the functionality from the display() function is implemented from the following source:
					// http://visp-doc.inria.fr/doxygen/visp-daily/vpMbtDistanceLine_8cpp_source.html
					
					if (e == 0) {
						if (!(*it)->isvisible || !(*it)->isTracked()) // no difference with isTracked() 
							continue;
					}
					*nEdges += 1; // increment count of number of edges that are visible and being tracked with visibility
					vpPoint *P1 = (*it)->p1;
					vpPoint *P2 = (*it)->p2;
					P1->changeFrame(cMo);
					P2->changeFrame(cMo);
					P1->project();
					P2->project();
					vpImagePoint IP1, IP2;
					vpMeterPixelConversion::convertPoint(cam, P1->get_x(), P1->get_y(), IP1);
					vpMeterPixelConversion::convertPoint(cam, P2->get_x(), P2->get_y(), IP2);
					pointx[i] = IP1.get_u();
					pointy[i] = IP1.get_v();
					i++;
					pointx[i] = IP2.get_u();
					pointy[i] = IP2.get_v();
					i++;
				}

				// GETTING THE KLT POINTS/FEATURES
				//  getkltimagepoints: Get the current list of KLT points for the reference camera.
				//	This function convert and copy the OpenCV KLT points into vpImagePoints.

				if (t == 1) {
					std::vector<vpImagePoint> kltPoints = tracker.getKltImagePoints();
					for (int i = 0; i < kltPoints.size(); i++) {
						kltX[i] = kltPoints[i].get_u();
						kltY[i] = kltPoints[i].get_v();
					}

					//kltNumber = kltPoints.size();
					*kltNumber = tracker.getKltNbPoints();
				}

				tracker.getPose(cMo);

				// Detect tracking error
				double projection_error = tracker.computeCurrentProjectionError(I, cMo, cam);
				if (projection_error > projection_error_threshold) {
					state = state_detection;
				}
				else {
					state = state_tracking;
				}
			}
			catch (...) {
				state = state_detection;
			}
		}
	}

	// Creates a cube.cao file in your current directory (in unityProject folder)
	// cubeEdgeSize : size of cube edges in meters 
	void createCaoFile(double cubeEdgeSize)
	{
		std::ofstream fileStream;
		fileStream.open("cube.cao", std::ofstream::out | std::ofstream::trunc);
		fileStream << "V1\n";
		fileStream << "# 3D Points\n";
		fileStream << "8                  # Number of points\n";
		fileStream << cubeEdgeSize / 2 << " " << cubeEdgeSize / 2 << " " << 0 << "    # Point 0: (X, Y, Z)\n";
		fileStream << cubeEdgeSize / 2 << " " << -cubeEdgeSize / 2 << " " << 0 << "    # Point 1\n";
		fileStream << -cubeEdgeSize / 2 << " " << -cubeEdgeSize / 2 << " " << 0 << "    # Point 2\n";
		fileStream << -cubeEdgeSize / 2 << " " << cubeEdgeSize / 2 << " " << 0 << "    # Point 3\n";
		fileStream << -cubeEdgeSize / 2 << " " << cubeEdgeSize / 2 << " " << -cubeEdgeSize << "    # Point 4\n";
		fileStream << -cubeEdgeSize / 2 << " " << -cubeEdgeSize / 2 << " " << -cubeEdgeSize << "    # Point 5\n";
		fileStream << cubeEdgeSize / 2 << " " << -cubeEdgeSize / 2 << " " << -cubeEdgeSize << "    # Point 6\n";
		fileStream << cubeEdgeSize / 2 << " " << cubeEdgeSize / 2 << " " << -cubeEdgeSize << "    # Point 7\n";
		fileStream << "# 3D Lines\n";
		fileStream << "0                  # Number of lines\n";
		fileStream << "# Faces from 3D lines\n";
		fileStream << "0                  # Number of faces\n";
		fileStream << "# Faces from 3D points\n";
		fileStream << "6                  # Number of faces\n";
		fileStream << "4 0 3 2 1          # Face 0: [number of points] [index of the 3D points]...\n";
		fileStream << "4 1 2 5 6\n";
		fileStream << "4 4 7 6 5\n";
		fileStream << "4 0 7 4 3\n";
		fileStream << "4 5 2 3 4\n";
		fileStream << "4 0 1 6 7          # Face 5\n";
		fileStream << "# 3D cylinders\n";
		fileStream << "0                  # Number of cylinders\n";
		fileStream << "# 3D circles\n";
		fileStream << "0                  # Number of circles\n";
		fileStream.close();
	}

	void AprilTagFunctionsCombined(unsigned char* const bitmap, int height, int width,
		double cam_px, double cam_py, double cam_u0, double cam_v0, double tagSize,
		double* array, double* arrayU, double* arrayV, double* arrayW, double *arrayT,
		double* h, double* w, double* apr, int* tag_id) {

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

		// Detection
		std::vector<vpHomogeneousMatrix> cMo_v;
		bool check = detector.detect(I, tagSize, cam, cMo_v);
		////if (check && detector.getNbObjects() > 0) { // if tag detected, we pick the first one

		//if (check) { // if tag detected, we pick the first one
		//	cMo = cMo_v[0];
		//}

		//If the image contains aprilTag
		if (check) {

			std::string message = detector.getMessage(0);
			std::cout << message;
			std::size_t tag_id_pos = message.find("id: ");
			int id = -1;
			if (tag_id_pos != std::string::npos) {
				id = atoi(message.substr(tag_id_pos + 4).c_str());
			}
			tag_id[0] = id;

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
			u = cMo_v[0].getCol(0);
			v = cMo_v[0].getCol(1);
			ww = cMo_v[0].getCol(2);
			t = cMo_v[0].getCol(3);

			for (size_t i = 0; i < u.size(); i++)
			{
				arrayU[i] = u.data[i];
				arrayV[i] = v.data[i];
				arrayW[i] = ww.data[i];
				arrayT[i] = t.data[i];
			}
		}

		else {
			array = 0;

			h[0] = -1;
			w[0] = -1;

			arrayU = 0;
			arrayV = 0;
			arrayW = 0;
			arrayT = 0;
			
		}
	}


}
