using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TMapper
{
    public class ImageCutter
    {
        public const int DefaultCut = 100;

        public int CutWidth { get; set; }
        public int CutHeight { get; set; }

        public string Source { get; protected set; }
        public string OutputFolder { get; set; }

        protected Image ImgSource;
        
        public ImageCutter(string source)
        {
            CutWidth = DefaultCut;
            CutHeight = DefaultCut;

            OutputFolder = "cuts";

            ImgSource = Image.FromFile(source);
            
        }

        private void CreateFolder()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }
        }

        public void Cuts()
        {
            CreateFolder();

            int rows = ImgSource.Height / CutHeight;
            int cols = ImgSource.Width / CutWidth;

            Rectangle destRect = new Rectangle(0,0, CutWidth, CutHeight);

            for(int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Bitmap b = new Bitmap(CutWidth, CutHeight);

                    Rectangle srcRect = new Rectangle(c*CutWidth, r*CutHeight, CutWidth, CutHeight);

                    Graphics g = Graphics.FromImage(b);

                    g.DrawImage(ImgSource, destRect, srcRect, GraphicsUnit.Pixel);


                    b.Save(Path.Combine(OutputFolder, String.Format("{0}_{1}.png", c.ToString("00"), r.ToString("00"))), ImageFormat.Png);
                }
            }

        }
    }
}
