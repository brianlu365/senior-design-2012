
namespace Microsoft.Kinect.TrackingRobot
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System.Diagnostics;
    using System.Timers;

  
    public partial class MainWindow : Window
    {
       
        private const float RenderWidth = 640.0f;
        private const float RenderHeight = 480.0f;
        private const double JointThickness = 3;
        private const double BodyCenterThickness = 10;
        private const double ClipBoundsThickness = 10;
        private readonly Brush centerPointBrush = Brushes.Blue;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Green;
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);
        private readonly Pen robotPen = new Pen(Brushes.Blue, 1);
        private KinectSensor sensor;
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;
        //...
        //Timer
        private Timer timer = new Timer(100);  //0.1 sec per tick
        private int Timer_duration = 0;
        //variables for left hand detection
        private Point currentLeftHandPosition;
        private Point previousLeftHandPosition;
        private bool updatePath = false;
        private bool isFirstFrameDetected = false;
        // a linked list structure to hold the path of the hand
        private HandPath handPath = new HandPath();
        
        //test area
        public Robot robot = new Robot();

        public MainWindow()
        {
            timer.Elapsed += new ElapsedEventHandler(timerEventHandler);
            InitializeComponent();
        }
        //event handler for timer
        private void timerEventHandler(Object sender, ElapsedEventArgs e)
        {
            Timer_duration += 1;
            if (Timer_duration >= 100000000)
            {
                Timer_duration = 0;
            }
        }
        private void startTimer()
        {
            Timer_duration = 0;
            timer.Enabled = true;
            timer.Start();
        }
        private void stopTimer()
        {
            timer.Enabled = false;
            timer.Stop();
        }
       
      

        //Initialize
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
        }

        //close window
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }
        // the display function
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        //real thing starts here
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            //is it the first frame
                            if (isFirstFrameDetected == false)
                            {
                                isFirstFrameDetected = true;
                                //set inital lefthand position
                                previousLeftHandPosition = SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position);
                                startTimer();
                            }
                            //get current left hand position
                            currentLeftHandPosition = SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position);
                            
                            //for print out
                            string temp = "(" + previousLeftHandPosition.X+ " , " + previousLeftHandPosition.Y + ")";
                            textBox4.Text = temp;
                            temp = "(" + currentLeftHandPosition.X + " , " + currentLeftHandPosition.Y + ")";
                            textBox3.Text = temp;
                            
                            //calculate the distance left hand traveled
                            double leftHandDistanceTraveled = Math.Sqrt(Math.Pow(currentLeftHandPosition.X - previousLeftHandPosition.X, 2) + Math.Pow(currentLeftHandPosition.Y - previousLeftHandPosition.Y, 2));
                            
                            //for print out
                            temp = " " + leftHandDistanceTraveled + " ";
                            textBox2.Text = temp;

                            //decide whether to start draw line
                            //if the left hand distance traveled is greater than 150
                            if (!updatePath)
                            {
                                if (leftHandDistanceTraveled >= 150)
                                {
                                    //if time duration is smaller than 0.5 sec
                                    if (Timer_duration < 5)
                                    {
                                        updatePath = true;
                                        previousLeftHandPosition = SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position);
                                        handPath.deleteAll();
                                        startTimer();
                                        goto next;
                                    }

                                }
                                //if the left hand distance traveled is smaller than 150
                                else
                                {
                                    if (Timer_duration >= 5)
                                    {
                                        previousLeftHandPosition = SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position);
                                        startTimer();
                                    }
                                }
                            }

                            //decide whether to stop updating the line
                            if (updatePath)
                            {
                                if (leftHandDistanceTraveled >= 150)
                                {
                                    if (Timer_duration < 5)
                                    {
                                        updatePath = false;
                                        previousLeftHandPosition = SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position);
                                        startTimer();
                                    }
                                }
                                else
                                {
                                    if (Timer_duration >= 5)
                                    {
                                        previousLeftHandPosition = SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position);
                                        startTimer();
                                    }
                                }
                            }
                        next:
                            //update the path
                            if (updatePath)
                            {
                                captureRightHandPath(skel);
                            }
                            

                            //draw the path
                            drawRightHandPath(dc);
                            //draw current right hand position
                            drawCurrentHandPosition(dc, skel);
                            string word = "320,320,360,320,360,380,320,380,90";
                            robot.setRobot(word);
                            textBox1.Text = "" + robot.topLeft + " , " + robot.topRight + " , " + robot.bottomLeft + " , " + robot.bottomRight;
                            drawRobot(dc, robot);
                        }                    
                        //else if (skel.TrackingState == SkeletonTrackingState.PositionOnly){}
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                //dc.DrawLine(this.inferredBonePen, new Point(1, 1), new Point(100, 100));
            }

        }

      
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
          
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

       
        //...
        //capture and Hand movements and add to the path
        private void captureRightHandPath(Skeleton skel)
        {
            if (handPath.head == null)
            {
                handPath.addNode(SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position));
            }
            else
            {
                // if the distance between current point and last point exceeds 1, add a new node
                Point currentPoint = SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position);
                double distance = Math.Sqrt(Math.Pow(currentPoint.X - handPath.tailPoint().X, 2) + Math.Pow(currentPoint.Y - handPath.tailPoint().Y, 2));
                if (distance >= handPath.distance)
                {
                    handPath.addNode(currentPoint);
                }
            }
            if (handPath.length > 100)
            {
                handPath.deleteAll();
            }

        }
        //draw the path
        private void drawRightHandPath(DrawingContext drawingContext)
        {
            if (handPath.head != null && handPath.head.next != null)
            {
                for (Node x = handPath.head; x.next != null; x = x.next)
                {
                    drawingContext.DrawLine(this.inferredBonePen, x.myPoint, x.next.myPoint);
                }
            }
        }
        //draw current hand position
        private void drawCurrentHandPosition(DrawingContext drawingContext, Skeleton skel)
        {
            Point currentPoint = SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position);
            drawingContext.DrawEllipse(inferredJointBrush, inferredBonePen, currentPoint, 10, 10);
            Debug.Print("currentPoint = (%d , %d)\n", currentPoint.X, currentPoint.Y);
            String text = "(" + currentPoint.X + " , " + currentPoint.Y+ ")";
            textBox1.Text = text;
        }
        // draw the robot
        private void drawRobot(DrawingContext drawingContext, Robot robot)
        {
            drawingContext.DrawLine(robotPen, robot.topLeft, robot.topRight);
            drawingContext.DrawLine(robotPen, robot.topRight, robot.bottomRight);
            drawingContext.DrawLine(robotPen, robot.bottomRight, robot.bottomLeft);
            drawingContext.DrawLine(robotPen, robot.bottomLeft, robot.topLeft);
        }

      
    }
    //hard coded linked list
    public class Node
    {

        public Node()
        {
            pre = next = null;
            turnAngle = new Angle();
        }
        public Node(Point p)
        {
            myPoint.X = p.X;
            myPoint.Y = p.Y;
            pre =  next = null;
            turnAngle = new Angle();
        }

        public Point myPoint;
        public Node next;
        public Node pre;
        public Angle turnAngle;
    }
    public class LinkedList
    {
        public LinkedList()
        {
            head = null;
            tail = null;
            length = 0;
            distance = 20.0;
        }
        public LinkedList(Point point)
        {
            head = new Node(point);
            tail = head;
            length = 1;
            distance = 20.0;

        }
        public void addNode(Point point)
        {
            if (head == null)
            {
                head = new Node(point);
                tail = head;
                length += 1;
            }
            else if (head.next == null)
            {
                head.turnAngle.angle = calculateAngle(head.myPoint, point);
                head.next = new Node(point);
                tail = head.next;
                tail.pre = head;
                length += 1;
            }
            else
            {
                tail.turnAngle.angle = calculateAngle(tail.myPoint, point);
                tail.next = new Node(point);
                tail.next.pre = tail;
                tail = tail.next;
                length += 1;
            }
        }
        public void deleteAll()
        {
            head = null;
            tail = null;
            GC.Collect();
            length = 0;
        }
        //calculate the angle based on two points : the angle is defined in Cartesian Coordinates
        public double calculateAngle(Point n1, Point n2)
        {
            double angle = 0;
            //region I
            if (n2.X > n1.X && n2.Y < n1.Y)
            {
                angle = Math.Abs(Math.Asin((n1.Y - n2.Y)/distance)) * 180 / Math.PI;
            }
            //region II
            else if (n2.X < n1.X && n2.Y < n1.Y)
            {
                angle = Math.Abs(Math.Asin((n1.Y - n2.Y) / distance)) * 180 / Math.PI;
                angle = 180 - angle;
            }
            //region III
            else if (n2.X < n1.X && n2.Y > n1.Y)
            {
                angle = Math.Abs(Math.Asin((n1.Y - n2.Y) / distance)) * 180 / Math.PI;
                angle = 180 + angle;
            }
            //region IV
            else if (n2.X > n1.X && n2.Y > n1.Y)
            {
                angle = Math.Abs(Math.Asin((n1.Y - n2.Y) / distance)) * 180 / Math.PI;
                angle = 360 - angle;
            }
            return angle;
        }
        public double distance{get; set;}
        public int length;
        public Node head;
        public Node tail;
    }
    public class HandPath : LinkedList
    {
        public HandPath() : base(){ }
        public HandPath(Point point) : base(point) { }
        public Point tailPoint() { return tail.myPoint; }
    }

    //angle class
    public class Angle
    {
        private double PI = Math.PI;
        public double angle;
      

        public Angle()
        {
            angle = 0;
        }
        public Angle(double ang)
        {
            angle = ang;
        }
        public double DEGTORAD()
        {
            return angle * PI / 180;
        }
        
    }
    // Rectangle Class
    public class Rectangle
    {
        public Rectangle()
        {
            topLeft.X = topLeft.Y = bottomRight.X = bottomRight.Y = 0;
        }
        public Rectangle(Point tl, Point tr, Point bl, Point br)
        {
           
            topLeft.X = tl.X;
            topLeft.Y = tl.Y;
            topRight.X = tr.X;
            topRight.Y = tr.Y;
            bottomLeft.X = bl.X;
            bottomLeft.Y = bl.Y;
            bottomRight.X = br.X;
            bottomRight.Y = br.Y;
            double bottomCenter_X = (br.X + bl.X) / 2.0;
            double bottomCenter_Y = (br.Y + bl.Y) / 2.0;
            double topCenter_X = (tl.X + tr.X) / 2.0;
            double topCenter_Y = (tl.Y + tr.Y) / 2.0;
            center.X = (bottomCenter_X + topCenter_X) / 2.0;
            center.Y = (bottomCenter_Y + topCenter_Y) / 2.0;
            width = Math.Sqrt(Math.Pow(tl.X - tr.X, 2) + Math.Pow(tl.Y - tr.Y, 2));
            length = Math.Sqrt(Math.Pow(tr.X - br.X, 2) + Math.Pow(tr.Y - br.Y, 2));
            angle.angle = calculateAngle();
        }
        //calculate the angle that the rectangle tilted
        protected double calculateAngle()
        {
            double ang = 0.0;
            //region I
            if (bottomRight.X <= topRight.X && bottomRight.Y >= topRight.Y)
            {
                ang = Math.Abs(Math.Asin((topRight.Y - bottomRight.Y) / length)) * 180 / Math.PI;
            }
            //region II
            else if (bottomRight.X >= topRight.X && bottomRight.Y >= topRight.Y)
            {
                ang = Math.Abs(Math.Asin((topRight.Y - bottomRight.Y) / length)) * 180 / Math.PI;
                ang = 180 - ang;
            }
            //region III
            else if (bottomRight.X >= topRight.X && bottomRight.Y <= topRight.Y)
            {
                ang = Math.Abs(Math.Asin((topRight.Y - bottomRight.Y) / length)) * 180 / Math.PI;
                ang = 180 + ang;
            }
            //region IV
            else if (bottomRight.X <= topRight.X && bottomRight.Y <= topRight.Y)
            {
                ang = Math.Abs(Math.Asin((topRight.Y - bottomRight.Y) / length)) * 180 / Math.PI;
                ang = 360 - ang;
            }
            return ang;
        }
       
        public Point topLeft = new Point();
        public Point bottomRight = new Point();
        public Point topRight = new Point();
        public Point bottomLeft = new Point();
        public double width;
        public double length;
        public Point center = new Point();
        public Angle angle = new Angle();
    }
    // Robot class
    public class Robot : Rectangle
    {
        public Robot() : base() { }
        public Robot(Point tl, Point tr, Point bl, Point br) : base(tl, tr, bl, br) { }
        //accept string "tl.X, tl.Y, tr.X, tr.Y, br.X, br.Y, bl.X, bl.Y, angle"
        public Robot(string str)
        {
            setRobot(str);
        }
        public void setRobot(string str)
        {
            //split the string
            char[] delimiterChars = { ' ', ',', '\t', '(', ')' };
            string[] words = str.Split(delimiterChars);
            if (words.Length < 9)
            {
                MessageBox.Show("the string is not long enough");
            }
            Point tl = new Point(Double.Parse(words[0]), Double.Parse(words[1]));
            Point tr = new Point(Double.Parse(words[2]), Double.Parse(words[3]));
            Point br = new Point(Double.Parse(words[4]), Double.Parse(words[5]));
            Point bl = new Point(Double.Parse(words[6]), Double.Parse(words[7]));

          
            topLeft.X = tl.X;
            topLeft.Y = tl.Y;
            topRight.X = tr.X;
            topRight.Y = tr.Y;
            bottomLeft.X = bl.X;
            bottomLeft.Y = bl.Y;
            bottomRight.X = br.X;
            bottomRight.Y = br.Y;
            double bottomCenter_X = (br.X + bl.X) / 2.0;
            double bottomCenter_Y = (br.Y + bl.Y) / 2.0;
            double topCenter_X = (tl.X + tr.X) / 2.0;
            double topCenter_Y = (tl.Y + tr.Y) / 2.0;
            center.X = (bottomCenter_X + topCenter_X) / 2.0;
            center.Y = (bottomCenter_Y + topCenter_Y) / 2.0;
            width = Math.Sqrt(Math.Pow(tl.X - tr.X, 2) + Math.Pow(tl.Y - tr.Y, 2));
            length = Math.Sqrt(Math.Pow(tr.X - br.X, 2) + Math.Pow(tr.Y - br.Y, 2));
            angle.angle = calculateAngle();
        }
    }
}