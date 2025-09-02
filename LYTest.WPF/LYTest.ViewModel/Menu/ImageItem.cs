using System.Windows.Media.Imaging;

namespace LYTest.ViewModel.Menu
{
    public class ImageItem : ViewModelBase
    {
        private string imageName;

        public string ImageName
        {
            get { return imageName; }
            set { SetPropertyValue(value, ref imageName, "ImageName"); }
        }
        private BitmapImage imageControl;

        public BitmapImage ImageControl
        {
            get { return imageControl; }
            set { SetPropertyValue(value, ref imageControl, "ImageControl"); }
        }

    }
}
