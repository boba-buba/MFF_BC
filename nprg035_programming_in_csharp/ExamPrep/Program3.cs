using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Drawing;
using System.Transactions;

namespace Exam3
{
    interface IImageProcessing
    {
        public void ChangeBrightness(float amount)
        {
            // Implementace změny světlosti
        }

        public void ChangeConstrast(float amount)
        {
            // Implementace změny kontrastu
        }

        public void Convert(string sourceFormat, string targetFormat);
        // dalsi algoritmy na zmeny

    }
    abstract class Image 
    {
        protected Color[,]? bitmap = null;
        public abstract void Load(string fileName);
        //public abstract void Save();
    }
    class JpegImage : Image, IImageProcessing
    {
        public override void Load(string fileName)
        {
            // Načítání JPEG obrázku do pole bitmap
        }
        public void ChangeBrightness(float amount)
        {
            // Implementace změny světlosti
        }

        public void ChangeConstrast(float amount)
        {
            // Implementace změny kontrastu
        }

        public void Convert(string sourceFormat, string targetFormat) { }

    }
    /*class BmpImage : Image
    {
        public override void Load(string fileName)
        {
            // Načítání BMP obrázku do pole bitmap
        }
        public override void Save()
        {
            // Ukládání BMP obrázku z pole bitmap
            // do souboru zapamatovaného jména
        }
    }*/
    /*class Prg10
    {
        public static void Main()
        {
            var image = new JpegImage();
            image.Load("blackboard.jpg");
            UpdateImageForProjector(image);
            image.Save();
        }
        public static void UpdateImageForProjector(
        Image image
        )
        {
            image.ChangeBrightness(1.75f);
            image.ChangeConstrast(1.6f);
        }
    }*/
}