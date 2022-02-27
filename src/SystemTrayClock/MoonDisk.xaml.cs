using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace SystemTrayClock
{
    // source: Converted from Visual Basic, demo by Charles Petzold https://docs.microsoft.com/en-us/archive/msdn-magazine/2009/june/customizing-the-new-wpf-calendar-controls
    //(Original moonmap4k.jpg from from http://planetpixelemporium.com/planets.html, resized to 25% and converted to PNG.) 
    public partial class MoonDisk : Viewport3D
    {
        private string _strMoonImage;
        private Uri _uriMoonImage;
        private BitmapImage _imgMoon;
        private ImageBrush _brush;
        private DiffuseMaterial _matMoon;
        private MeshGeometry3D _sphere;
        private GeometryModel3D _geomod;

        //Create shareable model for lighting
        private AmbientLight _ambiLight;
        private DirectionalLight _dirLight;
        private Model3DGroup _modgrp;

        //Create shareable camera
        private OrthographicCamera _cam;

        public MoonDisk()
        {
            InitializeComponent();
            Loaded += MoonDisk_Loaded;
        }

        private void MoonDisk_Loaded(object sender, RoutedEventArgs e)
        {
            InitViewport3D();
        }

        public DateTime CurrentDate
        {
            get { return (DateTime)GetValue(CurrentDateProperty); }
            set { SetValue(CurrentDateProperty, value); }
        }
        public static readonly DependencyProperty CurrentDateProperty =
            DependencyProperty.Register("CurrentDate", typeof(DateTime), typeof(MoonDisk), new PropertyMetadata(OnCurrentDateChanged));

        private static void OnCurrentDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = d as MoonDisk;
            if (e.NewValue is DateTime newDate && e.OldValue is DateTime oldDate && !newDate.Date.Equals(oldDate.Date))
            {
                owner.rotate.Angle = owner.PhaseAngle((DateTime)e.NewValue);

            }
        }

        private double PhaseAngle(DateTime currentDate)
        {
            DateTime dt = currentDate;
            if (currentDate.Kind == DateTimeKind.Local)
                dt = currentDate.ToUniversalTime();

            //7.1: Julian Day
            int yr = dt.Year;
            int mon = dt.Month;
            double day = dt.Day + (dt.Hour + (dt.Minute + (dt.Second + dt.Millisecond / 1000.0) / 60.0) / 60.0) / 24.0;

            if (mon == 1 || mon == 2)
            {
                yr -= 1;
                mon += 12;
            }
            int A = yr / 100;
            int B = 2 - A + A / 4;
            double JD = Math.Floor(365.25 * (yr + 4716)) + Math.Floor(30.6001 * (mon + 1)) + day + B - 1524.5;

            //Julian Emphemeris Day (approximate -- see chapter 9)
            double JDE = JD;

            //21.1: Time measured in Julian centures from Epoch J2000.0
            double tau = (JDE - 2451545) / 36525;

            //45.2: Mean elongation of moon
            double D = 297.8501921 + 445267.1114034 * tau - 0.0018819 * tau * tau + tau * tau * tau / 545868 - tau * tau * tau * tau / 113065000;
            D = Normalize(D);

            //45.3: Sun's mean anomaly
            double M = 357.5291092 + 35999.0502909 * tau - 0.0001536 * tau * tau + tau * tau * tau / 24490000;
            M = Normalize(M);

            //45.4: Moon's mean anomaly
            double Mprime = 134.9633964 + 477198.8675055 * tau + 0.0087414 * tau * tau + tau * tau * tau / 69699 - tau * tau * tau * tau / 14712000;
            Mprime = Normalize(Mprime);

            //46.4: Phase angle (simplified calculation)
            double i = 180 - D - 6.289 * Sine(Mprime) + 2.100 * Sine(M) - 1.274 * Sine(2 * D - Mprime) - 0.658 * Sine(2 * D) - 0.214 * Sine(2 * Mprime) - 0.110 * Sine(D);

            //46.1: Illuminated fraction (not used)
            double k = (1 + Cosine(i)) / 2;
            return i;
        }

        private double Normalize(double angle)
        {
            angle = angle - 360 * (int)(Fix(angle / 360));
            if (angle < 0)
                angle += 360;
            return angle;
        }

        public double Fix(double Number)
        {
            if (Number >= 0.0)
            {
                return Math.Floor(Number);
            }
            return 0.0 - Math.Floor(0.0 - Number);
        }

        private double Sine(double angle)
        {
            return Math.Sin(Math.PI * angle / 180);
        }

        private double Cosine(double angle)
        {
            return Math.Cos(Math.PI * angle / 180);
        }

        private void InitViewport3D()
        {
            _strMoonImage = "pack://application:,,,/System Tray Clock;Component/moonmap.png";
            _uriMoonImage = new Uri(_strMoonImage);
            _imgMoon = new BitmapImage(_uriMoonImage);
            _brush = new ImageBrush(_imgMoon);
            _matMoon = new DiffuseMaterial(_brush);
            _sphere = GenerateSphere(new Point3D(0, 0, 0), 1, 360, 180);
            _geomod = new GeometryModel3D(_sphere, _matMoon);

            //Create shareable model for lighting
            _ambiLight = new AmbientLight(Color.FromRgb(16, 16, 16));
            _dirLight = new DirectionalLight(Colors.White, new Vector3D(0, 0, -1));
            _modgrp = new Model3DGroup();

            //Create shareable camera
            _cam = new OrthographicCamera(new Point3D(0, 0, 2), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 2.1);
            _cam = new OrthographicCamera(new Point3D(0, 0, 2), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 2.1);

            //Put together the Viewport3D
            viewport3d.Camera = _cam;
            modvisGeometry.Content = _geomod;
            modvisLight.Content = _modgrp;
            _geomod.Freeze();
            _modgrp.Children.Add(_ambiLight);
            for (int i = 0; i < 3; i++)
            {
                _modgrp.Children.Add(_dirLight);
            }
            _modgrp.Freeze();
        }

        private MeshGeometry3D GenerateSphere(Point3D center, double radius, int slices, int stacks)
        {
            //Create the MeshGeometry3D
            var mesh = new MeshGeometry3D();

            //Fill the Position, Normals, and TextureCoordinates collections
            for (int stack = 0; stack < stacks; stack++)
            {
                double phi = Math.PI / 2 - stack * Math.PI / stacks;
                double y = radius * Math.Sin(phi);
                double scale = -radius * Math.Cos(phi);
                for (int slice = 0; slice < slices; slice++)
                {
                    double theta = slice * 2 * Math.PI / slices;
                    double x = scale * Math.Sin(theta);
                    double z = scale * Math.Cos(theta);
                    var normal = new Vector3D(x, y, z);
                    mesh.Normals.Add(normal);
                    mesh.Positions.Add(normal + center);
                    mesh.TextureCoordinates.Add(new Point(Convert.ToDouble(slice) / slices, Convert.ToDouble(stack) / stacks));
                }
            }

            //Fill the TriangleIndices collection
            for (int stack = 0; stack < stacks - 1; stack++)
            {
                for (int slice = 0; slice < slices - 1; slice++)
                {
                    int n = slices + 1; //keep the line length down
                    if (stack != 0)
                    {
                        mesh.TriangleIndices.Add((stack + 0) * n + slice);
                        mesh.TriangleIndices.Add((stack + 1) * n + slice);
                        mesh.TriangleIndices.Add((stack + 0) * n + slice + 1);
                    }
                    if (stack != stacks - 1)
                    {
                        mesh.TriangleIndices.Add((stack + 0) * n + slice + 1);
                        mesh.TriangleIndices.Add((stack + 1) * n + slice);
                        mesh.TriangleIndices.Add((stack + 1) * n + slice + 1);
                    }
                }
            }
            return mesh;
        }
    }
}
