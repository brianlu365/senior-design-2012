
namespace Microsoft.Kinect.TrackingRobot
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;


  
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
        private KinectSensor sensor;
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;
        //...

        // a linked list structor to hold the path of the hand
        private HandPath handPath = new HandPath();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

      
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

            if (null == this.sensor)
            {
                
            }
        }

        
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

       
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
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            
                            captureRightHandPath(skel);
                            drawRightHandPath(dc);
                            drawCurrentHandPosition(dc, skel);

                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                      
                            captureRightHandPath(skel);
                            drawRightHandPath(dc);
                            drawCurrentHandPosition(dc, skel);
                        }
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
                if (distance >= 1.0)
                {
                    handPath.addNode(currentPoint);
                }
            }
            if (handPath.length > 200)
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
        private void drawCurrentHandPosition(DrawingContext drawingContext, Skeleton skel)
        {
            Point currentPoint = SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position);
            drawingContext.DrawEllipse(inferredJointBrush, inferredBonePen, currentPoint, 10, 10);
        }
    }
    //hard coded linked list
    public class Node
    {

        public Node()
        {
            next = null;
        }
        public Node(Point p)
        {
            myPoint.X = p.X;
            myPoint.Y = p.Y;
            next = null;
        }
        public Point myPoint;
        public Node next;
    }
    public class LinkedList
    {
        public LinkedList()
        {
            head = null;
            tail = null;
            length = 0;
        }
        public LinkedList(Point point)
        {
            head = new Node(point);
            tail = head;
            length = 1;

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
                head.next = new Node(point);
                tail = head.next;
                length += 1;
            }
            else
            {
                tail.next = new Node(point);
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
}