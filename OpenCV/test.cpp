#include <stdio.h>

#include <cv.h>
#include <highgui.h>
#include <time.h>

int drawing = 0;
int last_x = 0;
int last_y = 0;
int this_x = 0;
int this_y = 0;

void sleep(unsigned int mseconds){
	clock_t upper_1 = mseconds + clock();
	while (upper_1 > clock());
}

void on_mouse(int event, int x, int y, int flags, void* param)
{
    last_x = this_x;
    last_y = this_y;
	this_x = x;
	this_y = y;

    if (event == CV_EVENT_LBUTTONDOWN)
    {
        // switches between On and Off
        if (drawing)
            drawing = 0;
        else
            drawing = 1;
    }
}


int main()
{
    CvCapture* capture = NULL;
   if ((capture = cvCaptureFromCAM(-1)) == NULL)
    {
        fprintf(stderr, "ERROR: capture is NULL \n");
        return -1;
    }

    cvNamedWindow("mywindow", CV_WINDOW_AUTOSIZE);

    cvQueryFrame(capture); // Sometimes needed to get correct data

    cvSetMouseCallback("mywindow",&on_mouse, 0);

    IplImage* frame = NULL;
    IplImage* drawing_frame = NULL;
    while (1)
    {
        if ((frame = cvQueryFrame(capture)) == NULL)
        {
            fprintf( stderr, "ERROR: cvQueryFrame failed\n");
            break;
        }

        if (frame == NULL)
        {
            fprintf( stderr, "WARNING: cvQueryFrame returned NULL, sleeping..\n");
            sleep(100000);
            continue;
        }

        if (!drawing_frame) // This frame is created only once
        {
            drawing_frame = cvCreateImage(cvSize(frame->width, frame->height), frame->depth, frame->nChannels);
            cvZero(drawing_frame);
        }

        if (drawing)
        {
            //cvCircle(drawing_frame, cvPoint(last_x,last_y), 10,CV_RGB(0, 255, 0), -1, CV_AA, 0);
			cvLine(drawing_frame, cvPoint(last_x, last_y), cvPoint(this_x, this_y), CV_RGB(0, 255, 0), 10, CV_AA, 0);
            // For overlaying (copying transparent images) in OpenCV
            // http://www.aishack.in/2010/07/transparent-image-overlays-in-opencv/
            for (int x = 0; x < frame->width; x++)
            {
                for (int y = 0; y < frame->height; y++)
                {
                    CvScalar source = cvGet2D(frame, y, x);
                    CvScalar over = cvGet2D(drawing_frame, y, x);

                    CvScalar merged;
                    CvScalar S = { 1,1,1,1 };
                    CvScalar D = { 1,1,1,1 };

                    for(int i = 0; i < 4; i++)
                        merged.val[i] = (S.val[i] * source.val[i] + D.val[i] * over.val[i]);

                    cvSet2D(frame, y, x, merged);
                }
            }
        }

        cvShowImage("mywindow", frame);

        int key = cvWaitKey(10);
        if (key  == 113) // q was pressed on the keyboard
            break;
    }

    cvReleaseImage(&frame);
    cvReleaseImage(&drawing_frame);
    cvReleaseCapture(&capture);
    cvDestroyWindow("mywindow");

    return 0;
}