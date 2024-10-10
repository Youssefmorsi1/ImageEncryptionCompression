using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace ImageEncryptCompress
{
    public partial class MainForm : Form
    {
        TimeSpan totalTime;
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        List<bool> compressedImage = new List<bool>();
        Node redRoot;
        Node greenRoot;
        Node blueRoot;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
            //testing encryption
            ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
           
        }

        private void btnEncryptImage_Click(object sender, EventArgs e)
        {
            string seedstring = seed_box.Text;
            List<bool>seed= new List<bool>();
            foreach(char c in seedstring)
            {
                if(c=='1')
                    seed.Add(true);
                else if (c == '0')
                    seed.Add(false);
            }
            int tap = Convert.ToInt32(tap_pos_box.Text);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ImageMatrix = ImageOperations.ImageEncryption(ImageMatrix, seed, tap);
            (redRoot, greenRoot, blueRoot, compressedImage) = ImageOperations.ImageCompression(ImageMatrix);
            Console.WriteLine($"image size after compression is :{(compressedImage.Count) / 8} byte ");
            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            totalTime = elapsedTime;
            Console.WriteLine($"Elapsed time: {elapsedTime}");
            stopwatch.Reset();
            //Console.WriteLine("size of compressed image is : " + compressedImage.Count / 8+"byte");
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);


        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Save Changes To File?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                e.Cancel = false; // Cancel the form closing event
            }
            else
            {
                string seedstring = seed_box.Text;
                List<bool> seed = new List<bool>();
                foreach (char c in seedstring)
                {
                    if (c == '1')
                        seed.Add(true);
                    else if(c == '0')
                        seed.Add(false);
                }
                int tap = Convert.ToInt32(tap_pos_box.Text);
                string path = ImageOperations.BrowseFile();
                int height = ImageOperations.GetHeight(ImageMatrix);
                int width = ImageOperations.GetWidth(ImageMatrix);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                ImageOperations.WriteBitsToBinaryFile(seed, tap, height, width, redRoot, greenRoot, blueRoot, compressedImage, path);
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                totalTime += elapsedTime;
                Console.WriteLine($"total time: {totalTime}");
                stopwatch.Reset();
            }
        }

        private void DecryptImage_Click(object sender, EventArgs e)
        {
            string path = ImageOperations.BrowseFile();
            List<bool> returnedList = new List<bool>();
            List<bool> returnedSeed=new List<bool>();
            int returnedTap;
            int returnedHeight;
            int returnedWidth;
            Node red;
            Node green;
            Node blue;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            (returnedSeed, returnedTap, returnedHeight, returnedWidth, red, green, blue, returnedList) = ImageOperations.ReadBitsFromBinaryFile(path);
            ImageMatrix = ImageOperations.ImageDecompression( returnedHeight, returnedWidth, red, green, blue, returnedList);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            ImageMatrix = ImageOperations.ImageEncryption(ImageMatrix, returnedSeed, returnedTap);
            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            Console.WriteLine($"Elapsed time: {elapsedTime}");
            stopwatch.Reset();
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void Save_Image_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.Image.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
            }

        }
    }
}