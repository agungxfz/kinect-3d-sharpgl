using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Kinect3DTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor sense;
        Skeleton[] skeletonBtn;
        Skeleton skeletonku;
        Skeleton[] skeletons = new Skeleton[6];

        public MainWindow()
        {
            InitializeComponent();
            pewaktuBersih();
        }
        private void Memuat_Aplikasi(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.sense = KinectSensor.KinectSensors.Where(item => item.Status == KinectStatus.Connected).FirstOrDefault();
                this.skeletonBtn = new Skeleton[sense.SkeletonStream.FrameSkeletonArrayLength];
                this.sense.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                this.sense.SkeletonStream.Enable();
                this.sense.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.sense.SkeletonFrameReady += DeteksiLangkahTangan;
                this.sense.ColorFrameReady += GambarRGB;
                this.sense.Start();
            }
            else
            {
                MessageBoxResult res = MessageBox.Show("Kinect tidak terhubung", "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
                this.sense.ForceInfraredEmitterOff = true;
                this.sense.Stop();
                Application.Current.Shutdown();
            }
        }
        private void Menutup_Aplikasi(object sender, EventArgs e)
        {
            if (sense != null)
            {
                this.sense.ForceInfraredEmitterOff = true;
                this.sense.Stop();
                Application.Current.Shutdown();
            }
        }
        private void GambarRGB(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frameGambar = e.OpenColorImageFrame())
            {
                if (frameGambar == null)
                {
                    return;
                }

                byte[] dataPixel = new byte[frameGambar.PixelDataLength];
                frameGambar.CopyPixelDataTo(dataPixel);
                this.RGBView.Source = BitmapSource.Create(
                    frameGambar.Width,
                    frameGambar.Height,
                    96, 96,
                    PixelFormats.Bgr32,
                    null, dataPixel, frameGambar.Width * 4);
            }
        }
        private void DeteksiLangkahTangan(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skelframe = e.OpenSkeletonFrame())
            {
                if (skelframe != null && skeletonBtn != null)
                {
                    skelframe.CopySkeletonDataTo(skeletonBtn);
                }
            }
            foreach (Skeleton skeletonas in skeletonBtn)
            {
                Joint tgnkanan = skeletonas.Joints[JointType.HandRight];
                Joint tgnkiri = skeletonas.Joints[JointType.HandLeft];

                skeletonku = skeletonas;
                if (skeletonas.TrackingState == SkeletonTrackingState.Tracked & skeletons != null )
                {
                    GambarKubus(skel_vec3d(tgnkanan.Position));
                    GambarKubus(skel_vec3d(tgnkiri.Position));
                }
            }
        }
        private Vector3D skel_vec3d(SkeletonPoint skelpoint)
        {
            Vector3D vec3d = new Vector3D(skelpoint.X, skelpoint.Y, skelpoint.Z);
            return vec3d;
        }
        private Model3DGroup BuatModelBentuk(Point3D p0, Point3D p1, Point3D p2)
        {
            MeshGeometry3D meshku = new MeshGeometry3D();
            meshku.Positions.Add(p0);
            meshku.Positions.Add(p1);
            meshku.Positions.Add(p2);
            meshku.TriangleIndices.Add(0);
            meshku.TriangleIndices.Add(1);
            meshku.TriangleIndices.Add(2);

            Vector3D normalvec = HitungPosisi(p0, p1, p2);
            meshku.Normals.Add(normalvec);
            meshku.Normals.Add(normalvec);
            meshku.Normals.Add(normalvec);

            Material materi = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
            GeometryModel3D modelku = new GeometryModel3D(meshku, materi);
            Model3DGroup grup = new Model3DGroup();
            grup.Children.Add(modelku);
            return grup;
        }
        private Vector3D HitungPosisi(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D vec1 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D vec2 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(vec1, vec2);
        }
        private void GambarKubus(Vector3D vec3d)
        {
            Model3DGroup kubus = new Model3DGroup();
            float delta = 0.05f;
            Point3D p0 = new Point3D(-vec3d.X, vec3d.Y, vec3d.Z);
            Point3D p1 = new Point3D(-vec3d.X + delta, vec3d.Y, vec3d.Z);
            Point3D p2 = new Point3D(-vec3d.X + delta, vec3d.Y, vec3d.Z + delta);
            Point3D p3 = new Point3D(-vec3d.X, vec3d.Y, vec3d.Z + delta);
            Point3D p4 = new Point3D(-vec3d.X, vec3d.Y + delta, vec3d.Z);
            Point3D p5 = new Point3D(-vec3d.X + delta, vec3d.Y + delta, vec3d.Z);
            Point3D p6 = new Point3D(-vec3d.X + delta, vec3d.Y + delta, vec3d.Z + delta);
            Point3D p7 = new Point3D(-vec3d.X, vec3d.Y + delta, vec3d.Z + delta);

            kubus.Children.Add(BuatModelBentuk(p3, p2, p6));
            kubus.Children.Add(BuatModelBentuk(p3, p6, p7));

            kubus.Children.Add(BuatModelBentuk(p2, p1, p5));
            kubus.Children.Add(BuatModelBentuk(p2, p5, p6));

            kubus.Children.Add(BuatModelBentuk(p1, p0, p4));
            kubus.Children.Add(BuatModelBentuk(p1, p4, p5));

            kubus.Children.Add(BuatModelBentuk(p0, p3, p7));
            kubus.Children.Add(BuatModelBentuk(p0, p7, p4));

            kubus.Children.Add(BuatModelBentuk(p7, p6, p5));
            kubus.Children.Add(BuatModelBentuk(p7, p5, p4));

            kubus.Children.Add(BuatModelBentuk(p2, p3, p0));
            kubus.Children.Add(BuatModelBentuk(p2, p0, p1));

            ModelVisual3D model3d = new ModelVisual3D();
            model3d.Content = kubus;
            this.ViewPortUtama.Children.Add(model3d);
        }
        private void BersihkanViewPort()
        {
            ModelVisual3D m3d;
            for (int i = this.ViewPortUtama.Children.Count - 1; i >= 0; i--)
            {
                m3d = (ModelVisual3D)ViewPortUtama.Children[i];
                if (m3d.Content is DirectionalLight == false)
                    this.ViewPortUtama.Children.Remove(m3d);
            }
        }
        private void pewaktuBersih()
        {
            DispatcherTimer dispatcherTim = new DispatcherTimer();
            dispatcherTim.Tick += new EventHandler(dispatcherTim_Tick);
            dispatcherTim.Interval = new TimeSpan(0, 0, 1);
            dispatcherTim.Start();
        }
        private void dispatcherTim_Tick(object sender, EventArgs e)
        {
            BersihkanViewPort();
        }
    }
}
