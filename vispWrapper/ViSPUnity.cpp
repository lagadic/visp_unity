#include "ViSPUnity.h"
#include <iostream>

extern "C" {
	ORB_SLAM2::System* pSlam = NULL;
	vpDetectorAprilTag* pTag_detector = NULL;

	vpImage<unsigned char>* pI = NULL;
	vpDisplayGDI* pDisplay = NULL;
	
	bool is_slam_tracking = false;
	float last_distance_to_tag = std::numeric_limits<float>::max();
	int frame_count_since_slam_init = 0;
	vpHomogeneousMatrix cMo_last;
	vpHomogeneousMatrix cMo_at_slam_init;
	std::string path_to_slam_vocab = " ";
	std::string path_to_camera_params = "";
	
	void Shutdown()
	{
		if (pSlam != NULL)
		{
			pSlam->Shutdown();
			delete pSlam;
			pSlam = NULL;
		}

		if (pDisplay != NULL)
		{
			pDisplay->closeDisplay();
			delete pDisplay;
			pDisplay = NULL;
		}

		if (pI != NULL)
		{
			//pI->destroy();
			//delete pI;
			//pI = NULL;
		}

		if (pTag_detector != NULL)
		{
			delete pTag_detector;
			pTag_detector = NULL;
		}
	}

	void PoseFromAprilTag(unsigned char* const bitmap,
						int bitmap_height, int bitmap_width,
						double cam_px, double cam_py,
						double cam_u0, double cam_v0,
						double cam_kud, double cam_kdu,
						double tag_size,
						double* tag_centre_image,
						double* camera_pose_U, double* camera_pose_V, double* camera_pose_W, double *camera_pose_T,
						double* tag_bb_height, double* tag_bb_width,
						double* tag_dimensions,
						int* tag_id,
						int* is_tag_detected)
	{
		vpHomogeneousMatrix cMo = PoseFromAprilTag_cMo(bitmap,
														bitmap_height, bitmap_width,
														cam_px, cam_py,
														cam_u0, cam_v0,
														cam_kud, cam_kdu,
														tag_size,
														tag_centre_image,
														camera_pose_U, camera_pose_V, camera_pose_W, camera_pose_T,
														tag_bb_height, tag_bb_width,
														tag_dimensions,
														tag_id,
														is_tag_detected);
														
	}

	vpHomogeneousMatrix PoseFromAprilTag_cMo(unsigned char* const bitmap,
											int bitmap_height, int bitmap_width,
											double cam_px, double cam_py,
											double cam_u0, double cam_v0,
											double cam_kud, double cam_kdu,
											double tag_size,
											double* tag_centre_image,
											double* camera_pose_U, double* camera_pose_V, double* camera_pose_W, double *camera_pose_T,
											double* tag_bb_height, double* tag_bb_width,
											double* tag_dimensions,
											int* tag_id,
											int* is_tag_detected)
	{	
		vpCameraParameters cam;
		cam.initPersProjWithDistortion(cam_px, cam_py, cam_u0, cam_v0, cam_kud, cam_kdu);
		
		for (int r = 0; r < bitmap_height; r++) {
			for (int c = 0; c < bitmap_width / 2; c++) {
				unsigned char temp = bitmap[r*bitmap_width + c];
				bitmap[r*bitmap_width + c] = bitmap[r*bitmap_width + bitmap_width - c - 1];
				bitmap[r*bitmap_width + bitmap_width - c - 1] = temp;
			}
		}
		
		pI->resize(bitmap_height, bitmap_width);
		pI->bitmap = bitmap;
		
		// AprilTag detection
		std::vector<vpHomogeneousMatrix> vector_detected_cMo;
		vpHomogeneousMatrix cMo;
		bool check = pTag_detector->detect(*pI, tag_size, cam, vector_detected_cMo);
		vpDisplay::flush(*pI); //Mendatory to display the requested features
		vpDisplay::display(*pI); //display the image
		
		is_tag_detected[0] = (int)check;

		// If the image contains aprilTag
		if (check) {

			std::string message = pTag_detector->getMessage(0);
			std::cout << message;
			std::size_t tag_id_pos = message.find("id: ");
			int id = -1;
			if (tag_id_pos != std::string::npos) {
				id = atoi(message.substr(tag_id_pos + 4).c_str());
			}
			tag_id[0] = id;

			// Calc coordinates of AprilTag
			vpRect bbox = pTag_detector->getBBox(0);
			std::unique_ptr<double[]> coord = std::make_unique<double[]>(4);
			vpImagePoint X = bbox.getCenter();
			coord[0] = X.get_i();
			coord[1] = X.get_j();
			coord[2] = X.get_u();
			coord[3] = X.get_v();
			for (size_t i = 0; i < 4; i++)
			{
				tag_centre_image[i] = coord[i];
			}
			tag_bb_height[0] = bbox.getHeight();
			tag_bb_width[0] = bbox.getWidth();

			// Calc dims of tag
			vpPolygon polygon(pTag_detector->getPolygon(0));

			vector <vpImagePoint> corners = polygon.getCorners();
			tag_dimensions[0] = vpImagePoint::distance(corners[0], corners[1]); // side1
			tag_dimensions[1] = vpImagePoint::distance(corners[1], corners[2]); // side2
			tag_dimensions[2] = vpImagePoint::distance(corners[2], corners[3]); // side3
			tag_dimensions[3] = vpImagePoint::distance(corners[3], corners[0]); // side4
			tag_dimensions[4] = vpImagePoint::distance(corners[0], corners[2]); // diagonal1
			tag_dimensions[5] = vpImagePoint::distance(corners[1], corners[3]); // diagonal2

			// Calc rotation of AprilTag
			cMo = vector_detected_cMo[0];
			vpArray2D<double> col_u, col_v, col_w, col_t;
			col_u = cMo.getCol(0);
			col_v = cMo.getCol(1);
			col_w = cMo.getCol(2);
			col_t = cMo.getCol(3);

			for (size_t i = 0; i < col_u.size(); i++)
			{
				camera_pose_U[i] = col_u.data[i];
				camera_pose_V[i] = col_v.data[i];
				camera_pose_W[i] = col_w.data[i];
				camera_pose_T[i] = col_t.data[i]; 
			}
		}
		else {							// no tag detected
			tag_centre_image = 0;
			tag_bb_height[0] = -1;
			tag_bb_width[0] = -1;
			camera_pose_U = 0;
			camera_pose_V = 0;
			camera_pose_W = 0;
			camera_pose_T = 0;
			
		}
		
		return cMo;
	}

	void PoseFromAprilTagWithSlam(unsigned char* const bitmap, 
								  int bitmap_height, int bitmap_width,
								  double cam_px, double cam_py, 
								  double cam_u0, double cam_v0, 
		         				  double cam_kud, double cam_kdu,
								  double tag_size,
								  double distance_to_tag_to_reinit,
								  double* tag_centre_image, 
								  double* camera_pose_U, double* camera_pose_V, double* camera_pose_W, double *camera_pose_T,
								  double* tag_bb_height, double* tag_bb_width, 
								  double* tag_dimensions,
								  int* tag_id, 
								  int* is_tag_detected,
		                          double* distance,
								  int* slam_tracking_state) 
	{
		
		vpHomogeneousMatrix cMo_tag = PoseFromAprilTag_cMo(bitmap,
														bitmap_height, bitmap_width,
														cam_px, cam_py,
														cam_u0, cam_v0,
														cam_kdu, cam_kud,
														tag_size,
														tag_centre_image,
														camera_pose_U, camera_pose_V, camera_pose_W, camera_pose_T,
														tag_bb_height, tag_bb_width,
														tag_dimensions,
														tag_id,
														is_tag_detected);

		float distance_to_tag = std::numeric_limits<float>::max();
		slam_tracking_state[0] = 0;

		if (is_tag_detected[0] == 1)
		{
			distance_to_tag = sqrt(pow(camera_pose_T[0], 2) + pow(camera_pose_T[1], 2) + pow(camera_pose_T[2], 2));
		}
		distance[0] = distance_to_tag;
		
		if (pSlam == NULL)
		{
			if (is_tag_detected[0] == 1)
			{
				pSlam = new ORB_SLAM2::System(path_to_slam_vocab, path_to_camera_params, ORB_SLAM2::System::MONOCULAR, false);
				pSlam->SetScalingParams(distance_to_tag, ORB_SLAM2::TAG, tag_centre_image[2], tag_centre_image[3]);
				frame_count_since_slam_init = 0;
				cMo_last = cMo_tag;
				cv::Mat slam_pose = PoseFromSlam(frame_count_since_slam_init);
				++frame_count_since_slam_init;
				is_slam_tracking = false;
				return;
			}
			else
			{
				return;
			}
		}
		
		if (distance_to_tag < distance_to_tag_to_reinit && last_distance_to_tag >= distance_to_tag_to_reinit)
		{
			pSlam->Reset();
			pSlam->SetScalingParams(distance_to_tag, ORB_SLAM2::TAG, tag_centre_image[2], tag_centre_image[3]); 
			cMo_last = cMo_tag;
			frame_count_since_slam_init = 0;
			cv::Mat slam_pose = PoseFromSlam(frame_count_since_slam_init);
			++frame_count_since_slam_init;
			last_distance_to_tag = distance_to_tag;
			is_slam_tracking = false;
			slam_tracking_state[0] = 0;
			return;
		}

		
		last_distance_to_tag = distance_to_tag;

		if (!is_slam_tracking)
		{
			pSlam->SetScalingParams(distance_to_tag, ORB_SLAM2::TAG, tag_centre_image[2], tag_centre_image[3]);
		}

		cv::Mat slam_pose = PoseFromSlam(frame_count_since_slam_init);
		++frame_count_since_slam_init;

	

		if (!slam_pose.empty())
		{
			if (!is_slam_tracking)
			{
				cMo_at_slam_init = cMo_last;
				is_slam_tracking = true;
			}
			cMo_last = cMo_tag;
			
			if (distance_to_tag < distance_to_tag_to_reinit) // close to tag and tag found. prefer tag pose to slam pose.
			{
				return;
				slam_tracking_state[0] = 0;
			}

			slam_tracking_state[0] = 1;
			
			if (is_tag_detected[0] == 1)
				return;

			vpHomogeneousMatrix cMo_slam;
			Mat2ViSP(slam_pose, cMo_slam);

			vpHomogeneousMatrix cMo = cMo_slam * cMo_at_slam_init;

			vpArray2D<double> col_u, col_v, col_w, col_t;
			col_u = cMo.getCol(0);
			col_v = cMo.getCol(1);
			col_w = cMo.getCol(2);
			col_t = cMo.getCol(3);

			for (size_t i = 0; i < col_u.size(); i++)
			{
				camera_pose_U[i] = col_u.data[i];
				camera_pose_V[i] = col_v.data[i];
				camera_pose_W[i] = col_w.data[i];
				camera_pose_T[i] = col_t.data[i];
			}

			
		}
		else
		{
			cMo_last = cMo_tag;
			slam_tracking_state[0] = 0;
		}
			
		return;
	}
   
	cv::Mat PoseFromSlam(int frame_count_tracking) 
	{
		cv::Mat vid_frame;
		vpImageConvert::convert(*pI, vid_frame);

		cv::Mat slam_pose = pSlam->TrackMonocular(vid_frame, frame_count_tracking);
		
		return slam_pose;
	}
	
	unsigned char* const FlipBitmap(unsigned char* const bitmap, int height, int width) {

		for (int r = 0; r < height; r++) {
			for (int c = 0; c < width / 2; c++) {
				unsigned char temp = bitmap[r*width + c];
				bitmap[r*width + c] = bitmap[r*width + width - c - 1];
				bitmap[r*width + width - c - 1] = temp;
			}
		}
		return bitmap;
	}
	

	int Mat2ViSP(const cv::Mat& mat_in, vpHomogeneousMatrix& visp_out)
	{
		int ret = 0;

		if (mat_in.type() != CV_32FC1)
		{
			std::cout << "Mat input is not floating-point number! It's " << mat_in.type() << std::endl;
			ret = 1;
			return ret;
		}

		std::cout << "Mat input ok" << std::endl;

		for (int i = 0; i < mat_in.rows; i++)
		{
			for (int j = 0; j < mat_in.cols; j++)
			{
				visp_out[i][j] = mat_in.ptr<float>(i)[j];  // new memory is created and data is copied in this line
			}
		}
		return ret;
	}

	void Init(const char* path_to_vocab, const char* path_to_cam_params, const int bitmap_height, const int bitmap_width) 
	{

		path_to_slam_vocab = path_to_vocab;
		path_to_camera_params = path_to_cam_params;

		pI = new vpImage<unsigned char>();
		pI->init(bitmap_height, bitmap_width);
		//pDisplay = new vpDisplayGDI();
		//pDisplay->init(*pI, 150, 150, "Tag tracker");
		
		pTag_detector = new vpDetectorAprilTag(vpDetectorAprilTag::TAG_36h11);
		pTag_detector->setDisplayTag(true);

		is_slam_tracking = false;		
		frame_count_since_slam_init = 0;
		cMo_last.init();
		cMo_at_slam_init.init();
	}


}
