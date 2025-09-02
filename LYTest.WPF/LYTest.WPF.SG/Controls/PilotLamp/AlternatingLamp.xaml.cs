using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LYTest.WPF.SG.Controls.PilotLamp
{
    /// <summary>
    /// AlternatingLamp.xaml 的交互逻辑
    /// </summary>
    public partial class AlternatingLamp : UserControl
    {
        public AlternatingLamp()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 绿色
        /// </summary>
        public Color LightColor
        {
            get { return (Color)GetValue(LightColorProperty); }
            set { SetValue(LightColorProperty, value); }
        }

        public static readonly DependencyProperty LightColorProperty =
            DependencyProperty.Register("LightColor", typeof(Color), typeof(AlternatingLamp), new PropertyMetadata(Colors.Red));

        /// <summary>
        /// 是否闪烁
        /// </summary>
        public bool Flash
        {
            get { return (bool)GetValue(FlashProperty); }
            set { SetValue(FlashProperty, value); }
        }
        public static readonly DependencyProperty FlashProperty =
            DependencyProperty.Register("Flash", typeof(bool), typeof(AlternatingLamp), new PropertyMetadata(false));



        /// 点亮灯
        /// <summary>
        /// 点亮灯
        /// </summary>
        public bool Shine
        {
            get { return (bool)GetValue(ShineProperty); }
            set
            {
                SetValue(ShineProperty, value);
            }
        }
        public static readonly DependencyProperty ShineProperty =
            DependencyProperty.Register("Shine", typeof(bool), typeof(AlternatingLamp), new PropertyMetadata(false));



        /// <summary>
        /// 属性改变触发事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {


            if (e.Property.Name == "Shine")
            {
                if (Shine)
                {
                    brush.Color = LightColor;
                    brush2.Color = LightColor;
                    brush3.Color = LightColor;

                }
                else
                {
                    brush.Color = Colors.Gray;
                    brush2.Color = Colors.Gray;
                    brush3.Color = Colors.Gray;
                    Storyboard storyBoard = Resources["storyBoard"] as Storyboard;
                    if (storyBoard != null)
                    {
                        storyBoard.Stop();
                    }



                }
            }
            if (e.Property.Name == "Flash")
            {
                if (Flash && Shine)
                {
                    Storyboard storyBoard = Resources["storyBoard"] as Storyboard;
                    if (storyBoard != null)
                    {
                        storyBoard.Begin();
                    }
                }
                Flash = false;
            }



            base.OnPropertyChanged(e);
        }



    }
}
