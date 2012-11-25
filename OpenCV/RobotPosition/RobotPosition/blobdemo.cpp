#include "cv.h"
#include "highgui.h"
#include "BlobResult.h"
#include "iostream"
using namespace std;


IplImage* GetThresholdedImageHSV( IplImage* img )  
{  
    // Create an HSV format image from image passed  
    IplImage* imgHSV = cvCreateImage( cvGetSize( img ),   
                                      8,   
                                      3 );     
  
    cvCvtColor( img, imgHSV, CV_BGR2HSV );  
  
    // Create binary thresholded image acc. to max/min HSV ranges  
    // For detecting blue gloves in "MOV.MPG - HSV mode  
    IplImage* imgThresh = cvCreateImage( cvGetSize( img ),   
                                         8,   
                                         1 );             
  
    cvInRangeS( imgHSV,  
                cvScalar( 104, 178, 70  ),  
                cvScalar( 130, 240, 124 ),  
                imgThresh );  
  
    // Tidy up and return thresholded image  
    cvReleaseImage( &imgHSV );  
    return imgThresh;  
}  

int main(int argc, char* argv[])
{
	CBlobResult blobs;
	CBlob* currentBlob;
	CvPoint pt1, pt2;  
    CvRect cvRect;

    cvNamedWindow( "INPUT", CV_WINDOW_AUTOSIZE);
	//cvNamedWindow( "CANN", CV_WINDOW_AUTOSIZE );

	cvNamedWindow( "BLOB", CV_WINDOW_AUTOSIZE);
    CvCapture* capture = cvCreateCameraCapture(0);
	IplImage* frame;
	while(1) {
		frame = cvQueryFrame( capture );
		cvFlip(frame,frame,1);
		if( !frame ) break;
		cvShowImage( "INPUT", frame );

		//assert( frame->width%2 == 0 && frame->height%2 == 0);
  //      IplImage* out = cvCreateImage( cvSize(frame->width/2,frame->height/2), frame->depth, frame->nChannels );
		//cvResize(frame,out);

		//IplImage* gray_out = cvCreateImage(cvGetSize(out),IPL_DEPTH_8U, 1); 
		//cvCvtColor(out,gray_out,CV_RGB2GRAY);
		//IplImage* cann_out = cvCreateImage(cvGetSize(out),IPL_DEPTH_8U, 1); 
		//cvCanny(gray_out,cann_out,10,100,3);
		//cvShowImage("CANN", cann_out);
  //      
		//cvReleaseImage(&out);
		//cvReleaseImage(&gray_out);
		//cvReleaseImage(&cann_out);

		IplImage* imgThresh = GetThresholdedImageHSV( frame );
		blobs = CBlobResult( imgThresh, NULL, 0 );   
		blobs.Filter( blobs,  
                      B_EXCLUDE,  
                      CBlobGetArea(),  
                      B_LESS,  
                      10 );

		int num_blobs = blobs.GetNumBlobs();  
  
        for ( int i = 0; i < num_blobs; i++ )    
        {                 
            currentBlob = blobs.GetBlob( i );               
            cvRect = currentBlob->GetBoundingBox();  
  
            pt1.x = cvRect.x;  
            pt1.y = cvRect.y;  
            pt2.x = cvRect.x + cvRect.width;  
            pt2.y = cvRect.y + cvRect.height;  
  
            // Attach bounding rect to blob in orginal video input  
            cvRectangle( frame,  
                         pt1,   
                         pt2,  
                         cvScalar(0, 0, 0, 0),  
                         1,  
                         8,  
                         0 );  
			cout << (pt1.x + pt2.x)/2 << " " << (pt1.y + pt2.y)/2 << endl;
        }
		cvShowImage( "BLOB", imgThresh );
	

		char c = cvWaitKey(22);
		if( c == 27 ) break;
		cvReleaseImage( &imgThresh ); 
	}
	cvReleaseCapture( &capture );
    cvDestroyAllWindows();
	return 0;
}