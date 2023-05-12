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
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Bitmap
{
    public partial class Form1 : Form
    {
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
            // 启动定时器，定时更新屏幕画面
            Timer timer = new Timer();
            timer.Interval = 1000/120; // 捕捉频率，以毫秒为单位
            timer.Tick += (sender1, e1) =>
            {
                var bitmap = DwmapiWrapper.CaptureScreen();
                bitmapQueue.Enqueue(bitmap);
            };
            timer.Start();
            ThreadPool.QueueUserWorkItem(ProcessBitmapQueue, pictureBox1);
        }


        static void ProcessBitmapQueue(object state)
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
                Thread.Sleep(5); // 以10毫秒为示例
            }
        }
    }
}
