
#include "stdafx.h"
#include "opencv2/opencv.hpp"

using namespace cv;
using namespace std;

Mat img, gray;
Size im_size;

void setup_calibration(int board_width, int board_height, int num_imgs, 
                       float square_size, char* imgs_directory, char* imgs_filename,
					   char* extension, vector < Point2f > &corners, vector < vector < Point2f>> &image_points,  vector < vector < Point3f>> &object_points) {
  Size board_size = Size(board_width, board_height);
  int board_n = board_width * board_height;

  for (int k = 1; k <= num_imgs; k++) {
    char img_file[100];
    sprintf(img_file, "%s%s%d.%s", imgs_directory, imgs_filename, k, extension);
    img = imread(img_file, CV_LOAD_IMAGE_COLOR);
    
    bool found = false;
    found = cv::findChessboardCorners(img, board_size, corners,
                                      CV_CALIB_CB_ADAPTIVE_THRESH | CV_CALIB_CB_FILTER_QUADS);
       
    vector< Point3f > obj;
    for (int i = 0; i < board_height; i++)
      for (int j = 0; j < board_width; j++)
        obj.push_back(Point3f((float)j * square_size, (float)i * square_size, 0));

    if (found) {
      cout << k << ". Found corners!" << endl;
      image_points.push_back(corners);
      object_points.push_back(obj);
    }
  }
}

void calculate_calibration(char * path, int num_img){
	vector< vector< Point3f > > object_points;
	vector< vector< Point2f > > image_points;
	vector< Point2f > corners;
	vector< vector< Point2f > > left_img_points;

	setup_calibration(7,7,num_img,1,path,"","jpg",corners,image_points,object_points);

	printf("Starting Calibration\n");
	Mat K;
	Mat D;
	vector< Mat > rvecs, tvecs;
	int flag = 0;
	flag |= CV_CALIB_FIX_K4;
	flag |= CV_CALIB_FIX_K5;
	calibrateCamera(object_points, image_points, img.size(), K, D, rvecs, tvecs, flag);

	cout << "K? \n";
	for(int x =0;x<3;x++){
		for(int y =0;y<3;y++){
			cout<< K.at<double>(x,y)<<"\t";

		}
		cout << endl;
	}
}

int _tmain(int argc, _TCHAR* argv[])
{
	calculate_calibration("C:\\Smietnik\\inf\\sem 3\\fotogrametria\\dane\\fold1\\", 5);
	calculate_calibration("C:\\Smietnik\\inf\\sem 3\\fotogrametria\\dane\\fold2\\", 14);
	return 0;
}
