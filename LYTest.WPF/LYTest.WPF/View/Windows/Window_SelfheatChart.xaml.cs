using LYTest.Core;
using LYTest.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LYTest.WPF.View.Windows
{
    /// <summary>
    /// Window_SelfheatChart.xaml 的交互逻辑
    /// </summary>
    public partial class Window_SelfheatChart : Window
    {
        static string LogFile;
        public static void Log(string message)
        {
            System.IO.File.AppendAllText(LogFile, message + Environment.NewLine);
        }

        Thread task;
        public Window_SelfheatChart()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            string path = System.IO.Directory.GetCurrentDirectory() + @"\Log\系统日志";
            System.IO.Directory.CreateDirectory(path);
            LogFile = $@"{path}\{DateTime.Now:yyyy-MM-dd}_chart.txt";
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            task = new Thread(new ThreadStart(RunSynch));
            task.Start();
        }

        public DynamicViewModel Data { get; set; }


        private void RunSynch()
        {
            while (true)
            {
                cv.Dispatcher.Invoke(DrawChart1);
                Task.Delay(3000).Wait();
            }
        }

        private void DrawChart1()
        {
            //EquipmentData.CheckResults.CheckNodeCurrent.CheckResults;
            cv.Children.Clear();
            float maxP = -999f; // 最大点
            float minP = 999f;  // 最小点
            float sum = 0;

            string data10 = Data.GetProperty("误差集合").ToString();
            txt.Text = data10;
            string[] arr10 = data10.TrimEnd(',').Split(',');

            List<float> list10 = new List<float>();
            foreach (string s in arr10)
            {
                if (float.TryParse(s, out float f))
                {
                    list10.Add(f);
                    if (f > maxP)
                        maxP = f;
                    if (f < minP)
                        minP = f;
                    sum += f;
                }
            }
            if (list10.Count <= 0) return;
            try
            {
                DrawChart(list10,maxP,minP,sum);

            }
            catch (Exception ex) { 
                string msg = ex.StackTrace;
                Log(msg);
            }
         }


        private void DrawChart(List<float> list10,float maxP,float minP,float sum)
        {
            //EquipmentData.CheckResults.CheckNodeCurrent.CheckResults;
            cv.Children.Clear();
            //float maxP = -999f; // 最大点
            //float minP = 999f;  // 最小点
            //float sum = 0;


            if (list10.Count <= 0) return;


            float avgP = sum / list10.Count; //  平均值
            float err = maxP - minP;
            float halfErr = err / 2f;

            // 每份的高度=  高度的80%，平均值的2倍分
            double pH = cv.ActualHeight * 0.8f / err;

            // 圆点
            Point p0 = new Point(40, cv.ActualHeight / 2f);

            // 横线
            Line l = new Line
            {
                X1 = p0.X,
                X2 = cv.ActualWidth - 20,
                Y1 = p0.Y,
                Y2 = p0.Y,
                StrokeThickness = 1f,
                Stroke = Brushes.White,
            };
            cv.Children.Add(l);

            // 竖线
            Line l2 = new Line
            {
                X1 = p0.X,
                X2 = p0.X,
                Y1 = 0,
                Y2 = cv.ActualHeight,
                StrokeThickness = 1f,
                Stroke = Brushes.White
            };
            cv.Children.Add(l2);

            // 顶点小线
            Line l1 = new Line
            {
                X1 = p0.X,
                X2 = p0.X + 10,
                Y1 = p0.Y - pH * halfErr,
                Y2 = p0.Y - pH * halfErr,
                StrokeThickness = 1f,
                Stroke = Brushes.White,
            };
            cv.Children.Add(l1);
            // 底点小线
            Line l3 = new Line
            {
                X1 = p0.X,
                X2 = p0.X + 10,
                Y1 = p0.Y + pH * halfErr,
                Y2 = p0.Y + pH * halfErr,
                StrokeThickness = 1f,
                Stroke = Brushes.White,
            };
            cv.Children.Add(l3);


            // 圆点标签
            TextBlock centText = new TextBlock
            {
                Text = $"{avgP:F3}",
                Foreground = Brushes.White,
            };
            Canvas.SetLeft(centText, 0f);
            Canvas.SetTop(centText, p0.Y - 7);
            cv.Children.Add(centText);

            // 顶点标签
            TextBlock topText = new TextBlock
            {
                Text = $"{avgP + halfErr:F3}",
                Foreground = Brushes.White,
            };
            Canvas.SetLeft(topText, 0f);
            Canvas.SetTop(topText, p0.Y - pH * halfErr - 7);
            cv.Children.Add(topText);

            // 底点标签
            TextBlock buttonText = new TextBlock
            {
                Text = $"{avgP - halfErr:F3}",
                Foreground = Brushes.White,
            };
            Canvas.SetLeft(buttonText, 0f);
            Canvas.SetTop(buttonText, p0.Y + pH * halfErr - 7);
            cv.Children.Add(buttonText);

            double pW = 10;

            if (cv.ActualWidth < pW * list10.Count + 100)
                cv.Width = pW * list10.Count + 100;

            int i = 0;
            Point lastP = new Point();


            foreach (float p in list10)
            {
                Point px = new Point(i * pW + p0.X, p0.Y + (avgP - p) * pH);

                if (i == 0) // 第1个点
                {
                    Ellipse pc = new Ellipse
                    {

                        Height = 3,
                        Width = 3,
                        Fill = Brushes.Red,
                        Stroke = new SolidColorBrush(Colors.Red)
                    };
                    Canvas.SetLeft(pc, px.X);
                    Canvas.SetTop(pc, px.Y);
                    cv.Children.Add(pc);
                    lastP = px;

                }
                else
                {
                    Line le = new Line()
                    {
                        X1 = lastP.X,
                        Y1 = lastP.Y,
                        X2 = px.X,
                        Y2 = px.Y,
                        StrokeThickness = 3f,
                        Stroke = Brushes.Red,

                    };
                    cv.Children.Add(le);
                    lastP = px;
                }


                i++;
            }

        }
    }
}
