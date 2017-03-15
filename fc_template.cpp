// Szablon dla przedmiotu Fotogrametria cyfrowa
// Autor: Krzysztof Bruniecki bruno@eti.pg.gda.pl
// Gdañsk 2016.02.25

#include "stdafx.h"
#include "opencv2/opencv.hpp"

using namespace cv;
using namespace std;

vector< vector< Point3f > > object_points;
vector< vector< Point2f > > image_points;
vector< Point2f > corners;
vector< vector< Point2f > > left_img_points;

Mat img, gray;
Size im_size;

void setup_calibration(int board_width, int board_height, int num_imgs, 
                       float square_size, char* imgs_directory, char* imgs_filename,
                       char* extension) {
  Size board_size = Size(board_width, board_height);
  int board_n = board_width * board_height;

  for (int k = 1; k <= num_imgs; k++) {
    char img_file[100];
    sprintf(img_file, "%s%s%d.%s", imgs_directory, imgs_filename, k, extension);
    img = imread(img_file, CV_LOAD_IMAGE_COLOR);
    //cv::cvtColor(img, gray, CV_BGR2GRAY);

    bool found = false;
    found = cv::findChessboardCorners(img, board_size, corners,
                                      CV_CALIB_CB_ADAPTIVE_THRESH | CV_CALIB_CB_FILTER_QUADS);
    /*if (found)
    {
      cornerSubPix(gray, corners, cv::Size(5, 5), cv::Size(-1, -1),
                   TermCriteria(CV_TERMCRIT_EPS | CV_TERMCRIT_ITER, 30, 0.1));
      drawChessboardCorners(gray, board_size, corners, found);
    }*/
    
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


int _tmain(int argc, _TCHAR* argv[])
{
	/*cv::Mat m,outM;
	std::string s = "../data/sample.jpg";
	cv::imread(s);
	m = cv::imread(s);
	std::cout << m.channels() <<std::endl;
	std::cout << m.size()<<std::endl;
	
	std::cout << m.at<char>(0,0)<<std::endl;
	std::vector<cv::Point2f> pp;
	//cv::Mat pp;
	cv::findChessboardCorners(m,cv::Size(7,7),pp);
	std::cout << pp;
	std::cout << pp[48].x << " "<<pp[48].y; */
	//std::cout << pp.at<float>(48,0)<<" "<<pp.at<float>(48,1);
	setup_calibration(7,7,5,1,"C:\\Smietnik\\inf\\sem 3\\fotogrametria\\dane\\wTwojejDupie\\","","jpg");

	 printf("Starting Calibration\n");
  Mat K;
  Mat D;
  vector< Mat > rvecs, tvecs;
  int flag = 0;
  flag |= CV_CALIB_FIX_K4;
  flag |= CV_CALIB_FIX_K5;
  calibrateCamera(object_points, image_points, img.size(), K, D, rvecs, tvecs, flag);

  /*for(int i = 0; i<5; i++){
	for(int x =0;x<3;x++){
	
			cout<< "imejdz: " << i << "madzik walju rwacek:" << rvecs.at(i).at<double>(x,0)<<"\n";
			cout<< "imejdz: " << i << "madzik walju twacek:" << tvecs.at(i).at<double>(x,0)<<"\n";

			cout<<"\n";
		}
	}*/
	  cout << "K wtf? \n";
  for(int x =0;x<3;x++){
	  for(int y =0;y<3;y++){
		  cout<< K.at<double>(x,y)<<"\t";

	  }
	  cout << endl;
  }
  /*
  cout << "D wtf? \n";
  for(int x =0;x<5;x++){
		  cout<< D.at<double>(0,x)<<"\t";
	  cout << endl;
  }*/

	return 0;
}

