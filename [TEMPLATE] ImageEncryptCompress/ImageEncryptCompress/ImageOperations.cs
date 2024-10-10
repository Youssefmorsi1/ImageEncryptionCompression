using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Collections;
using System.IO;

///Algorithms Project
///Intelligent Scissors
///

namespace ImageEncryptCompress
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }
    //el node: color ,freq, left , right , bit




    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>


    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }
        
        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


       /// <summary>
       /// Apply Gaussian smoothing filter to enhance the edge detection 
       /// </summary>
       /// <param name="ImageMatrix">Colored image matrix</param>
       /// <param name="filterSize">Gaussian mask size</param>
       /// <param name="sigma">Gaussian sigma</param>
       /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];

           
            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }



        public static byte convertToByte(List<bool> Key)
        {
            byte result = 0;
            for (int i = 0; i < c; i++)
            {
                if (Key[i])
                {
                    result |= (byte)(1 << (7 - i));
                }
            }
            return result;
        }
        const int c = 8;
        public static ( byte, List<bool>) LFSR(List<bool> seed, int tap)
        {
            //Initialization
            int Seed_Length = seed.Count;
            int Tap_Pos = Seed_Length - tap - 1;
            bool newbit;
            List<bool>key = new List<bool>();
            List<bool> ReturnedSeed = new List<bool>();


            for (int i = 0; i < c; i++)
            {
                newbit = seed[i] ^ seed[i+Tap_Pos];
                seed.Add(newbit);
                key.Add(newbit);

            }
            byte ReturnedKey=convertToByte(key);
            ReturnedSeed = seed.GetRange(c, Seed_Length);
            return (ReturnedKey, ReturnedSeed);
        }

        public static RGBPixel[,] ImageEncryption(RGBPixel[,] ImageMatrix, List<bool> seed, int tap)
        {
            //initialization
             byte[] RGB_Keys = new byte[3];
             int Image_Height = GetHeight(ImageMatrix);
             int Image_Width = GetWidth(ImageMatrix);

            // RGB_Keys: An array to store the generated random keys for each RGB component for each pixel.
            // Each RGB component of each pixel is XORed with its corresponding key to encrypt the pixel.
            for (int i = 0; i < Image_Height; i++)
            {
                for (int j = 0; j < Image_Width; j++)
                {
                    (RGB_Keys[0],seed) = LFSR(seed, tap);
                    (RGB_Keys[1], seed) = LFSR(seed, tap);
                    (RGB_Keys[2], seed) = LFSR(seed, tap);

                    ImageMatrix[i, j].red ^= RGB_Keys[0];
                    ImageMatrix[i, j].green ^= RGB_Keys[1];
                    ImageMatrix[i, j].blue ^= RGB_Keys[2];
                }
            }

            return ImageMatrix;
        }

        //compression
        public static (Node, Node, Node, List<bool>) ImageCompression(RGBPixel[,] ImageMatrix)
        {
            List<bool> compressedImage = new List<bool>();
            int Image_Height = GetHeight(ImageMatrix);
            int Image_Width = GetWidth(ImageMatrix);

            //calculate frequency for each color value
            int[] redFreq = new int[256];
            int[] greenFreq = new int[256];
            int[] blueFreq = new int[256];
            for (int i = 0; i < Image_Height; i++)
            {
                for (int j = 0; j < Image_Width; j++)
                {
                    redFreq[ImageMatrix[i, j].red]++;
                    greenFreq[ImageMatrix[i, j].green]++;
                    blueFreq[ImageMatrix[i, j].blue]++;
                }
            }

            //initialize priority queue for huffman
            PriorityQueue<Node> redQueue = new PriorityQueue<Node>();
            PriorityQueue<Node> greenQueue = new PriorityQueue<Node>();
            PriorityQueue<Node> blueQueue = new PriorityQueue<Node>();

            for(int i = 0; i < 256; i++)
            {
                if (redFreq[i] != 0)
                    redQueue.Enqueue(new Node((byte)i, redFreq[i]));
                if (greenFreq[i] != 0)
                    greenQueue.Enqueue(new Node((byte)i, greenFreq[i]));
                if (blueFreq[i] != 0)
                    blueQueue.Enqueue(new Node((byte)i, blueFreq[i]));
            }

            //construct tree and get root of tree
            Node redRoot = ConstructHuffman(redQueue);
            Node greenRoot = ConstructHuffman(greenQueue);
            Node blueRoot = ConstructHuffman(blueQueue);

            //traverse the trees to get binary code for each value
            List<bool>[] redCode = EncodeBFS(redRoot);
            List<bool>[] greenCode = EncodeBFS(greenRoot);
            List<bool>[] blueCode = EncodeBFS(blueRoot);


            //compress image into string
            for (int i = 0; i < Image_Height; i++)
            {
                for (int j = 0; j < Image_Width; j++)
                {
                    compressedImage.AddRange(redCode[ImageMatrix[i, j].red]);
                    compressedImage.AddRange(greenCode[ImageMatrix[i, j].green]);
                    compressedImage.AddRange(blueCode[ImageMatrix[i, j].blue]);
                }
            }

            return (redRoot, greenRoot, blueRoot, compressedImage);
        }



        static Node ConstructHuffman(PriorityQueue<Node> queue)
        {
            while(queue.Count > 1)
            {
                Node x = queue.Dequeue();
                Node y = queue.Dequeue();

                Node parent = new Node(0, x.Frequency + y.Frequency, x, y);
                queue.Enqueue(parent);
            }

            return queue.Dequeue();
        }

        static List<bool> [] EncodeBFS(Node root)
        {
            List<bool>[] binaryCode = new List<bool>[256];
            Queue<Tuple<Node, List<bool>>> queue = new Queue<Tuple<Node, List<bool>>>();
            Tuple<Node, List<bool>> start = new Tuple<Node, List<bool>>(root, new List<bool>());
            queue.Enqueue(start);

            while(queue.Count > 0)
            {
                var tmp = queue.Dequeue();
                Node node = tmp.Item1;
                List<bool> code = tmp.Item2;

                if(node.Left != null)
                {
                    List<bool> nxt = new List<bool>();
                    nxt.AddRange(code);
                    nxt.Add(false);

                    Tuple<Node, List<bool>> child = new Tuple<Node, List<bool>>(node.Left, nxt);
                    queue.Enqueue(child);
                }

                if(node.Right != null) {
                    List<bool> nxt = new List<bool>();
                    nxt.AddRange(code);
                    nxt.Add(true);

                    Tuple<Node, List<bool>> child = new Tuple<Node, List<bool>>(node.Right, nxt);
                    queue.Enqueue(child);
                }

                if(node.Left == null && node.Right == null) {
                    binaryCode[node.color] = code;
                }
            }
            

            return binaryCode;
        }

        static void DFSWrite(Node node, BinaryWriter writer)
        {
            //write color - false -> not leaf, true -> leaf
            bool leaf = (node.Left == null && node.Right == null);
            writer.Write(node.color);
            writer.Write(leaf);
            if (leaf)
                return;

            DFSWrite(node.Left, writer);
            DFSWrite(node.Right, writer);
        }
        public static void WriteBitsToBinaryFile(List<bool> initialSeed, int tapPosition, int imageHeight, int imageWidth, Node redRoot, Node greenRoot, Node blueRoot, List<bool> bitList, string filePath)
        {
            //order of write
            //initial seed - tap position - height - width - tree - bitlist
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    writer.Write(initialSeed.Count);
                    foreach (var c in initialSeed)
                        writer.Write(c);

                    writer.Write(tapPosition);
                    writer.Write(imageHeight);
                    writer.Write(imageWidth);

                    //write trees in dfs order
                    DFSWrite(redRoot, writer);
                    DFSWrite(greenRoot, writer);
                    DFSWrite(blueRoot, writer);

                    //write bitlist
                    byte currentByte = 0;
                    int byteSize = 0;
                    writer.Write(bitList.Count);

                    foreach (bool bit in bitList)
                    {
                        if (bit == true)
                            currentByte |= (byte)(1 << byteSize);
                        byteSize++;

                        if(byteSize == 8)
                        {
                            writer.Write((byte)currentByte);
                            byteSize = 0;
                            currentByte = 0;
                        }
                    }

                    //faka
                    if(byteSize != 0)
                        writer.Write(currentByte);
                }
            }
        }

        static Node DFSRead(BinaryReader reader)
        {
            Node node = new Node();
            node.color = reader.ReadByte();
            bool leaf = reader.ReadBoolean();
            if(leaf)
                return node;

            node.Left = DFSRead(reader);
            node.Right = DFSRead(reader);
            return node;
        }

       public static (List<bool>, int, int, int, Node, Node, Node, List<bool>) ReadBitsFromBinaryFile(string filePath)
        {
            List<bool> initialSeed = new List<bool>();
            int tapPosition = 0;
            int imageHeight = 0;
            int imageWidth = 0;
            Node redRoot;
            Node greenRoot;
            Node blueRoot;

            List<bool> bitList = new List<bool>();


            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    int seedLength = reader.ReadInt32();
                    for (int i = 0; i < seedLength; i++)
                    {
                        initialSeed.Add(reader.ReadBoolean());
                    }

                    tapPosition = reader.ReadInt32();
                    imageHeight = reader.ReadInt32();
                    imageWidth = reader.ReadInt32();

                    //read trees
                    redRoot = DFSRead(reader);
                    greenRoot = DFSRead(reader);
                    blueRoot = DFSRead(reader);

                    //read bitlist
                    int listSize = reader.ReadInt32();

                    byte[] readBytes = reader.ReadBytes((listSize + 7) / 8);
                    foreach(byte b in readBytes)
                    {
                        for(int i = 0; i < 8; i++)
                        {
                            if(listSize == 0) break;

                            if((b & (byte)(1 << i)) > 0)
                                bitList.Add(true);
                            else
                                bitList.Add(false);
                            listSize--;
                        }
                    }
                }
            }

            return (initialSeed, tapPosition, imageHeight, imageWidth, redRoot, greenRoot, blueRoot, bitList);
        }


        public static string BrowseFile()
        {
            string filePath = "";

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                }
            }

            return filePath;
        }

        //image decompression
        //notes: change arraylist to list.
        public static RGBPixel[,] ImageDecompression(int imageHeight, int ImageWidth, Node redRoot, Node greenRoot, Node blueRoot, List<bool> bitList)
        {
            //output
            RGBPixel[,] ImageMatrix = new RGBPixel[imageHeight, ImageWidth];

            //get val for each pixel's color
            int curIndex = 0;
            for(int i = 0; i < imageHeight; i++)
            {
                for(int j = 0; j < ImageWidth; j++)
                {
                    ImageMatrix[i, j].red = TraverseDecode(bitList, ref curIndex, redRoot);
                    ImageMatrix[i, j].green = TraverseDecode(bitList, ref curIndex, greenRoot);
                    ImageMatrix[i, j].blue = TraverseDecode(bitList, ref curIndex, blueRoot);
                }
            }

            return ImageMatrix;
        }

        static byte TraverseDecode(List<bool> bitlist, ref int index, Node node)
        {
            while(true) { 
                if(node.Left == null && node.Right == null)
                    return node.color;

                if (bitlist[index] == false)
                {
                    node = node.Left;
                    index++;
                }
                else
                {
                    node = node.Right;
                    index++;
                }
            }
        }

    }
}
