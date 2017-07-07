#include <visp3/gui/vpDisplayOpenCV.h>
#include <visp3/core/vpImage.h>
#include <visp3/core/vpImageConvert.h>
#include <visp3/blob/vpDot2.h>
#include <visp3/core/vpPixelMeterConversion.h>
#include <visp3/vision/vpPose.h>

/*
  This example detects blobs in an image acquired by an usb camera.
  When 4 blobs are detected, we compute the pose of the target wrt the camera.

  See tutorials:
  - http://visp-doc.inria.fr/doxygen/visp-daily/tutorial-grabber.html
  - http://visp-doc.inria.fr/doxygen/visp-daily/tutorial-tracking-blob.html#tracking_blob_auto
  - http://visp-doc.inria.fr/doxygen/visp-daily/tutorial-pose-estimation.html
 */
void computePose(std::vector<vpPoint> &point, const std::list<vpDot2> &dot,
                 const vpCameraParameters &cam, bool init, vpHomogeneousMatrix &cMo)
{
  vpPose pose;
  double x=0, y=0;
  unsigned int i = 0;
  for (std::list<vpDot2>::const_iterator it=dot.begin(); it != dot.end(); ++it) {
    vpPixelMeterConversion::convertPoint(cam, (*it).getCog(), x, y);
    point[i].set_x(x);
    point[i].set_y(y);
    pose.addPoint(point[i]);
    i++;
  }

  if (init == true) {
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


/*!
  Usage :
    To get the help    : ./tutorial-grabber-v4l2 --help
    To set the device  : ./tutorial-grabber-v4l2 --device 1 (to use /dev/video1)
    To subsample images: ./tutorial-grabber-v4l2 --scale 2
 */
int main(int argc, const char* argv[])
{
#ifdef VISP_HAVE_OPENCV
  try {
    unsigned int opt_device = 0;
    unsigned int opt_scale = 2; // Default value is 2 in the constructor. Turn it to 1 to avoid subsampling

    for (int i=0; i<argc; i++) {
      if (std::string(argv[i]) == "--device")
        opt_device = (unsigned int)atoi(argv[i+1]);
      else if (std::string(argv[i]) == "--scale")
        opt_scale = (unsigned int)atoi(argv[i+1]);
      else if (std::string(argv[i]) == "--help") {
        std::cout << "Usage: " << argv[0] << " [--device <camera device>] [--scale <subsampling factor>] [--help]" << std::endl;
        return 0;
      }
    }

		cv::VideoCapture cap(opt_device); // open the default camera
		if (!cap.isOpened()) { // check if we succeeded
			std::cout << "Failed to open the camera" << std::endl;
			return -1;
		}

		// trick to rescale image to be able to acquire 320x240 images 
		double cap_width = cap.get(cv::CAP_PROP_FRAME_WIDTH);
		double cap_height = cap.get(cv::CAP_PROP_FRAME_HEIGHT);
		std::cout << "Capture width: " << cap_width << std::endl;
		std::cout << "Capture height: " << cap_height << std::endl;
		int scale_width = (int)(cap_width / 320.);
		int scale_height = (int)(cap_height / 240.);
		int scale = std::max(scale_width, scale_height);
		if (scale > 1) {
			cap.set(cv::CAP_PROP_FRAME_WIDTH, cap_width / scale);
			cap.set(cv::CAP_PROP_FRAME_HEIGHT, cap_height / scale);
		}

		cv::Mat frame;
		int i = 0;
		while ((i++ < 100) && !cap.read(frame)) {}; // warm up camera by skiping unread frames

		vpImage<unsigned char> I;
		vpImageConvert::convert(frame, I);

		std::cout << "Image size: " << I.getWidth() << " " << I.getHeight() << std::endl;

		vpDisplayOpenCV d(I);

    if (0) { // code used to learn the characteristics of a blob that we want to retrieve auto;atically
      vpDot2 blob;
			cap >> frame; // get a new frame from camera
										// Convert the image in ViSP format and display it
			vpImageConvert::convert(frame, I);
			vpDisplay::display(I);
      vpDisplay::flush(I);

      // Learn the characteristics of the blob to auto detect
      blob.setGraphics(true);
      blob.setGraphicsThickness(1);
      blob.initTracking(I);
      blob.track(I);
      std::cout << "Blob characteristics: " << std::endl;
      std::cout << " width : " << blob.getWidth() << std::endl;
      std::cout << " height: " << blob.getHeight() << std::endl;
#if VISP_VERSION_INT > VP_VERSION_INT(2,7,0)
      std::cout << " area: " << blob.getArea() << std::endl;
#endif
      std::cout << " gray level min: " << blob.getGrayLevelMin() << std::endl;
      std::cout << " gray level max: " << blob.getGrayLevelMax() << std::endl;
      std::cout << " grayLevelPrecision: " << blob.getGrayLevelPrecision() << std::endl;
      std::cout << " sizePrecision: " << blob.getSizePrecision() << std::endl;
      std::cout << " ellipsoidShapePrecision: " << blob.getEllipsoidShapePrecision() << std::endl;
      return EXIT_SUCCESS;
      // Produces the following output on 4.5cm diameter black dot at 30cm from an usb sphere camera
      /*
       Image size: 320 240
       Blob characteristics:
       width : 38
       height: 35
       area: 977.5
       gray level min: 26
       gray level max: 123
       grayLevelPrecision: 0.8
       sizePrecision: 0.65
       ellipsoidShapePrecision: 0.65
      */
    }

    vpDot2 blob;
    blob.setWidth(40);
    blob.setHeight(40);
    blob.setArea(1000);
    blob.setGrayLevelMin(0);
    blob.setGrayLevelMax(150);
    blob.setSizePrecision(0.65);
    blob.setEllipsoidShapePrecision(0.65);

    // Define the 3D model of a target defined by 4 blobs arranged as a square
    std::vector<vpPoint> point;
    point.push_back( vpPoint(-0.06, -0.06, 0) );
    point.push_back( vpPoint( 0.06, -0.06, 0) );
    point.push_back( vpPoint( 0.06,  0.06, 0) );
    point.push_back( vpPoint(-0.06,  0.06, 0) );
    vpHomogeneousMatrix cMo;

    // Define the camera parameters (should be obtained by calibration)
    vpCameraParameters cam(840, 840, I.getWidth()/2, I.getHeight()/2);

    bool pose_init = true;

    while(1) {
			cap >> frame; // get a new frame from camera
										// Convert the image in ViSP format and display it
			vpImageConvert::convert(frame, I);
			vpDisplay::display(I);
      vpDisplay::displayText(I, 10, 10, "A click to quit", vpColor::red);
      std::list<vpDot2> blob_list;
      blob.searchDotsInArea(I, 0, 0, I.getWidth(), I.getHeight(), blob_list);

      std::cout << "Found " << blob_list.size() << " blob(s)" << std::endl;

      // track the blobs that are found (could be skiped)
      for(std::list<vpDot2>::iterator it=blob_list.begin(); it != blob_list.end(); ++it) {
        (*it).setGraphics(true);
        (*it).setGraphicsThickness(3);
        (*it).track(I);
      }

      // Compute pose from 4 blobs
      if (blob_list.size() == 4) {
        computePose(point, blob_list, cam, pose_init, cMo);
        vpDisplay::displayFrame(I, cMo, cam, 0.05, vpColor::none);
        pose_init = false;
      }
      else {
        pose_init = true;
      }

      vpDisplay::flush(I);
      if (vpDisplay::getClick(I, false)) break;
    }
    return EXIT_SUCCESS;
  }
  catch(vpException &e) {
    std::cout << "Catch an exception: " << e << std::endl;
    return EXIT_FAILURE;
  }
#else
  (void)argc;
  (void)argv;

	std::cout << "ViSP is not build with OpenCV as 3rd party..." << std::endl;
#endif
}
