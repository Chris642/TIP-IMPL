using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FragLabs.Audio.Codecs;
using System.ComponentModel;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using System.Windows.Threading;

namespace TIPimpl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VoiceHandling voicehandler;
        static public int volumein = 0;
        static public int volumeout = 0;

        DispatcherTimer _dispatcherTimer = null;
        static public int lastmax = 100;
        private void Dt_Tick(object sender, object e)
        {
            if(volumein > lastmax)
            {
                lastmax = volumein;
            }
            INPROG.Maximum = lastmax;
            INPROG.Value = volumein;
            OUTPROG.Maximum = lastmax;
            OUTPROG.Value = volumeout;
        }
        public MainWindow()
        {
            
            InitializeComponent();
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _dispatcherTimer.Tick += Dt_Tick;
            _dispatcherTimer.Start();
            //SplashScreen splash = new SplashScreen("Resources/su35b.gif");
            //splash.Show(true);
            //Populate device LIST BINCH
            voicehandler = new VoiceHandling();
            iNList.ItemsSource = voicehandler.populateIN_devices();
            if (WaveIn.DeviceCount > 0)
                iNList.SelectedIndex = 0;


            oUTList.ItemsSource = voicehandler.populateOUT_devices();
            if (WaveOut.DeviceCount > 0)
                oUTList.SelectedIndex = 0;
        }



        private void Send_click(object sender, RoutedEventArgs e)
        {          
            voicehandler.Record_OPUS(iNList.SelectedIndex, ipBOX.Text);
   
        }

        
        private void StopLclick(object sender, RoutedEventArgs e)
        {
            voicehandler.Stop_recording();
        }

        private void Receive_Click(object sender, RoutedEventArgs e)
        {
            voicehandler.Play_OPUS(oUTList.SelectedIndex, ipBOX.Text);
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            voicehandler.Stop_it();
        }

        private void StopR_Click(object sender, RoutedEventArgs e)
        {
            voicehandler.Stop_playing();
        }

        private void Call_Click(object sender, RoutedEventArgs e)
        {
            voicehandler.Record_OPUS(iNList.SelectedIndex, ipBOX.Text);
            voicehandler.Play_OPUS(oUTList.SelectedIndex, ipBOX.Text);
        }

        private void End_call_Click(object sender, RoutedEventArgs e)
        {
            voicehandler.Stop_it();
            volumein = 0;
            volumeout = 0;
        }
    }
}
