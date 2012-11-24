
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
        //Robot Timer
        private Timer robotTimer = new Timer(200);
        //variables for left hand detection
        private Point currentLeftHandPosition;
        private Point previousLeftHandPosition;
        private bool updatePath = false;
        private bool isFirstFrameDetected = false;
        // a linked list structure to hold the path of the hand
        private HandPath handPath = new HandPath();
        //Robot
        public Robot robot = new Robot();
        //test area
        string word = "320,320,330,320,330,340,320,340,90";
        public MainWindow()
        {
            timer.Elapsed += new ElapsedEventHandler(timerEventHandler);
            robotTimer.Elapsed += new ElapsedEventHandler(robotTimerEventHandler);
            robot.setRobot(word);
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
        private void robotTimerEventHandler(Object sender, ElapsedEventArgs e)
        {
            if (handPath.iterator.next != null)
            {
                double dist = Math.Sqrt(Math.Pow(robot.center.X - handPath.iterator.myPoint.X, 2) + Math.Pow(robot.center.Y - handPath.iterator.myPoint.Y, 2));
                //double next_dist = Math.Sqrt(Math.Pow(robot.center.X - handPath.iterator.next.myPoint.X, 2) + Math.Pow(robot.center.Y - handPath.iterator.next.myPoint.Y, 2));
                // distance between two points
                double d = Math.Sqrt(Math.Pow(handPath.iterator.myPoint.X - handPath.iterator.next.myPoint.X, 2) + Math.Pow(handPath.iterator.myPoint.Y - handPath.iterator.next.myPoint.Y, 2));
                Console.WriteLine(" " + dist + " ");
                if (dist >= d)
                {
                    handPath.iterator = handPath.iterator.next;
                    if (handPath.iterator != null)
                    {
                        robot.determineInitialRotationDirection(handPath.iterator.turnAngle);
                    }
                }
                if (handPath.iterator != null)
                {
                    robot.determineRotationDirection(handPath.iterator.turnAngle);
                    if (robot.rotationDirection == Robot.LEFT && robot.initialRotationDirection == Robot.LEFT)
                    {
                        robot.rotateRobot(handPath.iterator.turnAngle);
                    }
                    else if (robot.rotationDirection == Robot.RIGHT && robot.initialRotationDirection == Robot.RIGHT)
                    {
                        robot.rotateRobot(handPath.iterator.turnAngle);
                    }
                    else
                    {
                        
                        robot.correctAngle(handPath.iterator.turnAngle);
                        robot.moveRobot();
                       
                    }
                }
            }             
            else
            {
                robotTimer.Stop();
            }
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
                    foreach(Skeleton skel in skeletons)
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
                            //string temp = "(" + previousLeftHandPosition.X+ " , " + previousLeftHandPosition.Y + ")";
                            //textBox4.Text = temp;
                            //temp = "(" + currentLeftHandPosition.X + " , " + currentLeftHandPosition.Y + ")";
                            //textBox3.Text = temp;
                            
                            //calculate the distance left hand traveled
                            double leftHandDistanceTraveled = Math.Sqrt(Math.Pow(currentLeftHandPosition.X - previousLeftHandPosition.X, 2) + Math.Pow(currentLeftHandPosition.Y - previousLeftHandPosition.Y, 2));
                            
                            //for print out
                            //temp = " " + leftHandDistanceTraveled + " ";
                            //textBox2.Text = temp;

                            //decide whether to start draw line
                            //if the left hand distance traveled is greater than 100
                            if (!updatePath)
                            {
                                if (leftHandDistanceTraveled >= 120)
                                {
                                    //if time duration is smaller than 0.5 sec
                                    if (Timer_duration < 5)
                                    {
                                        if (isHandWithinRadius(robot, skel))
                                        {
                                            updatePath = true;
                                            previousLeftHandPosition = SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position);
                                            handPath.deleteAll();

                                            startTimer();
                                            goto next;
                                        }
                                        else
                                        {
                                            previousLeftHandPosition = SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position);
                                            startTimer();
                                        }
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
                                if (leftHandDistanceTraveled >= 120)
                                {
                                    if (Timer_duration < 5)
                                    {
                                        updatePath = false;
                                        previousLeftHandPosition = SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position);
                                        startTimer();
                                        //move robot
                                        handPath.iterator = handPath.head;
                                        robot.determineInitialRotationDirection(handPath.iterator.turnAngle);
                                        robotTimer.Enabled = true;
                                        robotTimer.Start();
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
                            
                            
                            //textBox1.Text = "" + robot.topLeft + " , " + robot.topRight + " , " + robot.bottomLeft + " , " + robot.bottomRight;
                            textBox2.Text = "" + robot.angle.angle + "";
                            drawRobot(dc, robot);
                            textBox4.Text = "( " + robot.center.X + " , " + robot.center.Y + " )";
                            if (handPath.iterator != null)
                            {
                               double dist = Math.Sqrt(Math.Pow(robot.center.X - handPath.iterator.myPoint.X, 2) + Math.Pow(robot.center.Y - handPath.iterator.myPoint.Y, 2));
                               textBox1.Text = "" + dist + "";
                            }
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
                // if the distance between current point and last point exceeds 40, add a new node
                Point currentPoint = SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position);
                double distance = Math.Sqrt(Math.Pow(currentPoint.X - handPath.tailPoint().X, 2) + Math.Pow(currentPoint.Y - handPath.tailPoint().Y, 2));
                if (distance >= handPath.distance)
                {
                    handPath.addNode(currentPoint);
                    if (handPath.tail.pre != null)
                        textBox3.Text = "" + handPath.tail.pre.turnAngle.angle;
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
            drawingContext.DrawLine(robotPen, robot.topLeft, robot.center);
            drawingContext.DrawLine(robotPen, robot.topRight, robot.center);
            drawingContext.DrawLine(robotPen, robot.bottomLeft, robot.center);
            drawingContext.DrawLine(robotPen, robot.bottomRight, robot.center);
        }

        // detect whether hand is within the radius of robot
        private bool isHandWithinRadius(Robot robot, Skeleton skel)
        {
            Point currentPoint = SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position);
            double distance = Math.Sqrt(Math.Pow(currentPoint.X - robot.center.X, 2) + Math.Pow(currentPoint.Y - robot.center.Y, 2));
            if (distance <= robot.radious)
            {
                return true;
            }
            else
                return false;
        }
    }
    //hard coded linked list
    public class Node
    {

        public Node()
        {
            pre = next = null;
        }
        public Node(Point p)
        {
            myPoint.X = p.X;
            myPoint.Y = p.Y;
            pre = next = null;
        }

        public Point myPoint;
        public Node next;
        public Node pre;
        public Angle turnAngle = new Angle();
    }
    public class LinkedList
    {
        public LinkedList()
        {
            head = null;
            tail = null;
            length = 0;
            distance = 40.0;
            iterator = head;
        }
        public LinkedList(Point point)
        {
            head = new Node(point);
            tail = head;
            length = 1;
            distance = 40.0;
            iterator = head;

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
            double d = Math.Sqrt(Math.Pow(n2.X - n1.X, 2) + Math.Pow(n2.Y - n1.Y, 2));
            //region I
            if (n2.X > n1.X && n2.Y < n1.Y)
            {
                angle = Math.Abs(Math.Asin((n1.Y - n2.Y) / d)) * 180 / Math.PI;
            }
            //region II
            else if (n2.X < n1.X && n2.Y < n1.Y)
            {
                angle = Math.Abs(Math.Asin((n1.Y - n2.Y) / d)) * 180 / Math.PI;
                angle = 180 - angle;
            }
            //region III
            else if (n2.X < n1.X && n2.Y > n1.Y)
            {
                angle = Math.Abs(Math.Asin((n1.Y - n2.Y) / d)) * 180 / Math.PI;
                angle = 180 + angle;
            }
            //region IV
            else if (n2.X > n1.X && n2.Y > n1.Y)
            {
                angle = Math.Abs(Math.Asin((n1.Y - n2.Y) / d)) * 180 / Math.PI;
                angle = 360 - angle;
            }
            return angle;
        }
        public double distance{get; set;}
        public int length;
        public Node head;
        public Node tail;
        public Node iterator;
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
        public void moveAngle(double w)
        {
            angle += w;
            if (angle < 0)
            {
                angle = angle + 360;
            }
            else if (angle >= 360)
            {
                angle = angle - 360;
            }
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
        public Robot(Point tl, Point tr, Point bl, Point br) : base(tl, tr, bl, br) 
        {
            calculateDxDy();
            centerDistance = Math.Sqrt(Math.Pow(topLeft.X - center.X, 2) + Math.Pow(topLeft.Y - center.Y, 2));
        }
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
            centerDistance = Math.Sqrt(Math.Pow(topLeft.X - center.X, 2) + Math.Pow(topLeft.Y - center.Y, 2));
            centerAngle = Math.Asin(0.5 * width / centerDistance) * 180 / Math.PI;
            angle.angle = calculateAngle();
            calculateDxDy();
        }
        private void calculateDxDy()
        {
            dx = dl * Math.Cos(angle.DEGTORAD());
            dy = -dl * Math.Sin(angle.DEGTORAD());
        }
        //determine rotation direction : may not be used
        public void determineRotationDirection(Angle _angle)
        {
            if (angle.angle < _angle.angle)
            {
                double d_angle = _angle.angle - angle.angle;
                double neg_d_angle = 360 - d_angle;
                if (d_angle <= neg_d_angle)
                {
                    rotationDirection = LEFT;
                }
                else
                {
                    rotationDirection = RIGHT;
                }

            }
            else if (angle.angle > _angle.angle)
            {
                double d_angle = angle.angle - _angle.angle;
                double neg_d_angle = 360 - d_angle;
                if (d_angle <= neg_d_angle)
                {
                    rotationDirection = RIGHT;
                }
                else
                {
                    rotationDirection = LEFT;
                }
            }
            else
            {
                rotationDirection = STRAIGHT;
            }
        }
        //determine initial Rotation Direction
        public void determineInitialRotationDirection(Angle _angle)
        {
            if (angle.angle < _angle.angle)
            {
                double d_angle = _angle.angle - angle.angle;
                double neg_d_angle = 360 - d_angle;
                if (d_angle <= neg_d_angle)
                {
                    initialRotationDirection = LEFT;
                }
                else
                {
                    initialRotationDirection = RIGHT;
                }

            }
            else if (angle.angle > _angle.angle)
            {
                double d_angle = angle.angle - _angle.angle;
                double neg_d_angle = 360 - d_angle;
                if (d_angle <= neg_d_angle)
                {
                    initialRotationDirection = RIGHT;
                }
                else
                {
                    initialRotationDirection = LEFT;
                }
            }
            else
            {
                initialRotationDirection = STRAIGHT;
            }
        }
        // rotate animation
        public void rotateRobot(Angle _angle)
        {
            if (angle.angle < _angle.angle)
            {
                double d_angle = _angle.angle - angle.angle;
                double neg_d_angle = 360 - d_angle;
                if (d_angle <= neg_d_angle)
                {
                    angle.moveAngle(dw);
                }
                else
                {
                    angle.moveAngle(-dw);
                }

            }
            else if (angle.angle > _angle.angle)
            {
                double d_angle = angle.angle - _angle.angle;
                double neg_d_angle = 360 - d_angle;
                if (d_angle <= neg_d_angle)
                {
                    angle.moveAngle(-dw);
                }
                else
                {
                    angle.moveAngle(dw);
                }
            }
            else
            {
                rotationDirection = STRAIGHT;
            }
            //...
            double bottomLeftAngle = angle.angle - 180 - centerAngle;
            double bottomRightAngle = angle.angle - 180 + centerAngle;
            double topLeftAngle = angle.angle + centerAngle;
            double topRightAngle = angle.angle - centerAngle;

            bottomLeft.X = center.X + centerDistance * Math.Cos(bottomLeftAngle * Math.PI / 180);
            bottomLeft.Y = center.Y - centerDistance * Math.Sin(bottomLeftAngle * Math.PI / 180);
            bottomRight.X = center.X + centerDistance * Math.Cos(bottomRightAngle * Math.PI / 180);
            bottomRight.Y = center.Y - centerDistance * Math.Sin(bottomRightAngle * Math.PI / 180);
            topLeft.X = center.X + centerDistance * Math.Cos(topLeftAngle * Math.PI / 180);
            topLeft.Y = center.Y - centerDistance * Math.Sin(topLeftAngle * Math.PI / 180);
            topRight.X = center.X + centerDistance * Math.Cos(topRightAngle * Math.PI / 180);
            topRight.Y = center.Y - centerDistance * Math.Sin(topRightAngle * Math.PI / 180);
            calculateDxDy();
        }
        // correct angle after rotation
        public void correctAngle(Angle _angle)
        {
            double d_angle = Math.Abs(angle.angle - _angle.angle);
            if (angle.angle < _angle.angle)
            {
                angle.moveAngle(d_angle);
            }
            else if (angle.angle > _angle.angle)
            {
                angle.moveAngle(-d_angle);
            }
            if (angle.angle != _angle.angle)
            {
                double bottomLeftAngle = angle.angle - 180 - centerAngle;
                double bottomRightAngle = angle.angle - 180 + centerAngle;
                double topLeftAngle = angle.angle + centerAngle;
                double topRightAngle = angle.angle - centerAngle;

                bottomLeft.X = center.X + centerDistance * Math.Cos(bottomLeftAngle * Math.PI / 180);
                bottomLeft.Y = center.Y - centerDistance * Math.Sin(bottomLeftAngle * Math.PI / 180);
                bottomRight.X = center.X + centerDistance * Math.Cos(bottomRightAngle * Math.PI / 180);
                bottomRight.Y = center.Y - centerDistance * Math.Sin(bottomRightAngle * Math.PI / 180);
                topLeft.X = center.X + centerDistance * Math.Cos(topLeftAngle * Math.PI / 180);
                topLeft.Y = center.Y - centerDistance * Math.Sin(topLeftAngle * Math.PI / 180);
                topRight.X = center.X + centerDistance * Math.Cos(topRightAngle * Math.PI / 180);
                topRight.Y = center.Y - centerDistance * Math.Sin(topRightAngle * Math.PI / 180);
                calculateDxDy();
            }
        }
        public void moveRobot()
        {
            topLeft.X += dx;
            topLeft.Y += dy;
            topRight.X += dx;
            topRight.Y += dy;
            bottomLeft.X += dx;
            bottomLeft.Y += dy;
            bottomRight.X += dx;
            bottomRight.Y += dy;
            center.X += dx;
            center.Y += dy;
        }
        public double radious = 10;
        //the distance from center of rectangle to the corner
        private double centerDistance;
        //the angle of corner, center, witdth/2
        private double centerAngle;
        public double dx;
        public double dy;
        private const double dl = 5;
        //anglar speed
        public const double dw = 2;
        public const int LEFT = 0;
        public const int RIGHT = 1;
        public const int STRAIGHT = 3;
        public int rotationDirection = STRAIGHT;
        public int initialRotationDirection = STRAIGHT;
    }
}