using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Baanto.ShadowSense.Services;

namespace TouchDetection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        // index we're asking for.
        private const int SM_DIGITIZER = 94;
        private const int SM_MAXIMUMTOUCHES = 95;


        // Masks used to check results from SM_DIGITIZER check

        // The input digitizer is ready for input. If this value is unset, it may mean that the tablet service is stopped, the digitizer is not supported, or digitizer drivers have not been installed.
        private const int NID_READY = 0x80;
        
        // An input digitizer with support for multiple inputs is used for input.
        private const int NID_MULTI_INPUT = 0x40;
        
        // An integrated touch digitizer is used for input.
        private const int NID_INTEGRATED_TOUCH = 0x01;

        // An integrated pen digitizer is used for input.
        private const int NID_INTEGRATED_PEN = 0x04;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            bool hasPen = false;
            bool hasTouch = false;

            //get windows input tablet devices
            var devices = GetTouchDevices();

            //new instance of ShadowSense service
            var ss = new ShadowSenseService();

            //find ShadowSense devices
            var info =  ss.GetDeviceInfo().ToList();

            StringBuilder sb = new StringBuilder();


            //display nummber of tablet devices
            sb.AppendFormat("{0} Tablet devices detected", devices);
            sb.AppendLine();

            sb.AppendLine();

            sb.AppendFormat("{0} ShadowSense devices detected", info.Count);
            sb.AppendLine();

            foreach (var item in info)
            {
                sb.AppendFormat("Device: {0}", item.Name);
                sb.AppendLine();
            }

            //get touchscreen flags
            int digitizer = GetSystemMetrics(SM_DIGITIZER);

            //check for pen support
            if ((digitizer & NID_INTEGRATED_PEN) == NID_INTEGRATED_PEN)
                hasPen = true;

            //check for touch support
            if ((digitizer & NID_INTEGRATED_TOUCH) == NID_INTEGRATED_TOUCH)
                hasTouch = true;

            sb.AppendLine();

            if (!hasTouch && !hasPen)
                sb.Append("No Pen or Touch available");

            if (hasPen)
                sb.Append("Pen");

            if (hasTouch && hasPen)
                sb.Append(" and ");

            if (hasTouch)
                sb.AppendFormat("Touch with {0} touch points", GetMaxTouches());

            TB.Text = sb.ToString();
        }

        private int GetTouchDevices()
        {
            // Get a collection of the tablet devices for this window.  
            var devices = Tablet.TabletDevices;

            return devices != null ? devices.Count : 0;
        }
        private int GetMaxTouches()
        {
            //get touchscreen flags
            int digitizer = GetSystemMetrics(SM_DIGITIZER);

            if ((digitizer & (NID_READY + NID_MULTI_INPUT)) == NID_READY + NID_MULTI_INPUT)
            {
                int numTouchPoints = GetSystemMetrics(SM_MAXIMUMTOUCHES);
                return numTouchPoints;
            }
            return 0;
        }
    }
}
