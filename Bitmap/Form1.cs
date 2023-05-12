using Bitmap.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Timer = System.Windows.Forms.Timer;

namespace Bitmap
{
    public partial class Form1 : Form
    {
        private IntPtr _currentIntpr = IntPtr.Zero;
        private Timer timer;
        static ConcurrentQueue<System.Drawing.Bitmap> bitmapQueue = new ConcurrentQueue<System.Drawing.Bitmap>(); // 位图队列
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(); // 取消标记源

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateCurrentWindows();
        }

        private void StartGetCaptureWindow()
        {
            // 初始化定时器和取消令牌
            timer = new Timer();
            cancellationTokenSource = new CancellationTokenSource();
            timer.Interval = 1000 / 120; // 捕捉频率，以毫秒为单位
            timer.Tick += Timer_Tick;
            // 启动定时器和处理队列的线程
            timer.Start();
            ThreadPool.QueueUserWorkItem(ProcessBitmapQueue, pictureBox1);
        }

        private void StopGetCaptureWindow()
        {
            // 停止定时器和处理队列的线程
            timer.Stop();
            cancellationTokenSource.Cancel();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_currentIntpr != IntPtr.Zero)
            {
                var bitmap = DwmapiWrapper.CaptureWindowInBackgroundC(_currentIntpr);
                bitmapQueue.Enqueue(bitmap);
            }
        }

        private void UpdateCurrentWindows()
        {
            listBox1.Items.Clear();
            AllWindowsIntPtr.GetEnumWindows();
            foreach (string title in AllWindowsIntPtr.intPtrDictionary.Keys)
            {
                listBox1.Items.Add(title);
            }
        }

        private void ListBox_SelectionChanged(object sender, EventArgs e)
        {
            // 获取选中项的值
            string selectedItem = listBox1.SelectedItem as string;
            if (AllWindowsIntPtr.intPtrDictionary.ContainsKey(selectedItem))
            {
                _currentIntpr = AllWindowsIntPtr.intPtrDictionary[selectedItem];
            }
        }

         void ProcessBitmapQueue(object state)
        {
            PictureBox pictureBox = (PictureBox)state;

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (bitmapQueue.TryDequeue(out System.Drawing.Bitmap bitmap))
                {
                    // 在PictureBox控件中显示位图
                    pictureBox.Invoke(new Action(() =>
                    {
                        pictureBox.Image?.Dispose(); // 释放上一个位图
                        pictureBox.Image = bitmap.Clone() as Image;
                    }));

                    // 释放位图资源
                    bitmap.Dispose();
                }
                // 延时一段时间，以控制处理速度
                Thread.Sleep(2); // 以10毫秒为示例
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                StartGetCaptureWindow();
            }
            else
            {
                StopGetCaptureWindow();
                pictureBox1.Image?.Dispose(); // 释放上一个位图
                pictureBox1.Image = null;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
