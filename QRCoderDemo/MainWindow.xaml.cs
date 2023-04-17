using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.System;

namespace QRCoderDemo
{
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            this.InitializeComponent();
            GetDatas();
        }
        public ObservableCollection<ImageFileInfo> Images { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        async void GetDatas()
        {
            Images = new ObservableCollection<ImageFileInfo>();
            using (var context = new SQLiteContext())
            {
                if (context.ImageDatas.Any())
                {
                    foreach (var image in context.ImageDatas)
                    {
                        var imageData = await GetImageFileAsync(image.Name);
                        Images.Add(imageData);
                    }
                }
            }
        }

        async Task<ImageFileInfo> GetImageFileAsync(string text)
        {
            ImageFileInfo imageFile = new ImageFileInfo();
            imageFile.Name = text;
            string level = "M";
            QRCodeGenerator.ECCLevel eccLevel = (QRCodeGenerator.ECCLevel)(level == "L" ? 0 : level == "M" ? 1 : level == "Q" ? 2 : 3);

            //Create raw qr code data
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, eccLevel);
            PngByteQRCode qrCodePng = new PngByteQRCode(qrCodeData);
            //byte[] qrCodeImagePng = qrCodePng.GetGraphic(20, new byte[] { 144, 201, 111 }, new byte[] { 118, 126, 152 });
            byte[] qrCodeImagePng = qrCodePng.GetGraphic(20);

            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(qrCodeImagePng);
                    await writer.StoreAsync();
                }
                var image = new BitmapImage();
                await image.SetSourceAsync(stream);

                imageFile.Source = image;
            }
            return imageFile;
        }

        private async void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (sender is TextBox tb && e.Key.Equals(VirtualKey.Enter))
            {
                if (!Images.Any(a => a.Name.Equals(tb.Text)))
                {
                    var image = await GetImageFileAsync(tb.Text);
                    if (image != null)
                    {
                        Images.Add(image);
                        tb.Text = String.Empty;
                        using (var context = new SQLiteContext())
                        {
                            await context.ImageDatas.AddAsync(new ImageData() { Name = image.Name });
                            await context.SaveChangesAsync();
                        }
                    }
                }
            }

        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Tag is ImageFileInfo image)
                {
                    this.Images.Remove(image);
                    using (var context = new SQLiteContext())
                    {
                        var imageFromRepo = context.ImageDatas.FirstOrDefault(x => x.Name == image.Name);
                        if (imageFromRepo is not null)
                        {
                            context.ImageDatas.Remove(imageFromRepo);
                            context.SaveChanges();
                        }
                    }
                }
            }
        }

    }
}