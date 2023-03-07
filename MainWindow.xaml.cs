using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace playersclubb_
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool PlayingMusic;
        public string[] SortedMediaFiles;
        public string CurrentMediaName;
        public int currentIndex;
        public MainWindow()
        {
            InitializeComponent();
            CurrentMusic.Volume = 0.7;
            SoundSlider.Maximum = 1;
            SoundSlider.Value = (double)CurrentMusic.Volume;
            Thread ThreadForSlider = new Thread(_ =>
            {
                while (true)
                {

                    Dispatcher.Invoke(() =>
                    {
                            AudioSlider.Value = CurrentMusic.Position.Ticks;
                       
                    });
                    Thread.Sleep(1000);
                }
            });
            ThreadForSlider.Start();
        }

        private void DirectoryPickerButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog() { IsFolderPicker = true };
            CommonFileDialogResult result = fileDialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                string[] mediaFiles = Directory.GetFiles(fileDialog.FileName);
                SortedMediaFiles = mediaFiles.Where(x => Path.GetExtension(x) == ".mp3" || Path.GetExtension(x) == ".m4a" || Path.GetExtension(x) == ".wav").ToArray();
                foreach (string mediaFile in SortedMediaFiles)
                {
                    MediaFilesBox.Items.Add(mediaFile);
                    Method4PlayingMusic(0);
                }
            }
        }

        private void MediaFilesBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentMediaName = MediaFilesBox.SelectedValue.ToString();
            CurrentMusic.Source = new Uri($"{CurrentMediaName}");
            AudioSlider.Value = 0;
            CurrentMusic.Play();
            PlayingMusic = true;
            currentIndex = MediaFilesBox.SelectedIndex;
        }

        private void CurrentMusic_MediaOpened(object sender, RoutedEventArgs e)
        {
            AudioSlider.Maximum = CurrentMusic.NaturalDuration.TimeSpan.Ticks;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrentMusic.Position = new TimeSpan(Convert.ToInt64(AudioSlider.Value));
            if (PlayingMusic == true)
            {

                var timeElapsed = CurrentMusic.Position;
                var TimeFromStart = string.Format("{0:D2}:{1:D2}:{2:D2}", timeElapsed.Hours, timeElapsed.Minutes, timeElapsed.Seconds);
                PassedTime.Text = TimeFromStart;
                var TotalTime = new TimeSpan(CurrentMusic.NaturalDuration.TimeSpan.Ticks);
                TotalTime = TotalTime - timeElapsed;
                var TotalTimeOut = string.Format("{0:D2}:{1:D2}:{2:D2}", TotalTime.Hours, TotalTime.Minutes, TotalTime.Seconds);
                LeftTime.Text = TotalTimeOut;
            }


            if (AudioSlider.Value == AudioSlider.Maximum)
            {
                EnablePosition();
                Method4PlayingMusic(currentIndex++);
                if (currentIndex == SortedMediaFiles.Length - 1)
                {
                    Method4PlayingMusic(0); // Опять начало с первого трека
                }
            }

        }
        public void Method4PlayingMusic(int Index)
        {
            CurrentMediaName = MediaFilesBox.Items[Index].ToString();
            CurrentMusic.Source = new Uri($"{CurrentMediaName}");
            AudioSlider.Value = 0;
            CurrentMusic.Play();
            PlayingMusic = true;
        }
        private void PlayOrStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayingMusic == true)
            {
                CurrentMusic.Pause();
                PlayingMusic = false;
            }
            else if (PlayingMusic == false)
            {
                CurrentMusic.Play();
                PlayingMusic = true;
            }
        }
        public void EnablePosition()
        {
            if (currentIndex <= -1)
            {
                currentIndex = 0;
            }
            if (currentIndex >= SortedMediaFiles.Length)
            {
                currentIndex = SortedMediaFiles.Length - 1;
            }
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            EnablePosition();
            Method4PlayingMusic(currentIndex++);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            EnablePosition();
            Method4PlayingMusic(currentIndex--);
        }

        private void SoundSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrentMusic.Volume = (double)SoundSlider.Value;

        }
        private void AgainButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMusic.Stop();
        }

        private void MixButton_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            int RandomIndex = rnd.Next(0, SortedMediaFiles.Length - 1);
            if (RandomIndex == currentIndex) //Для того, чтобы трек при перемешивании не мог идти 2 раза подряд
            {
                RandomIndex = rnd.Next(0, SortedMediaFiles.Length - 1);
            }
            if (RandomIndex != currentIndex)
            {
                currentIndex = RandomIndex;
                Method4PlayingMusic(RandomIndex);
            }
        }


    }
}
