using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Advanced;
using System.Threading.Tasks;

class Program
{
    //static string Grad = @"$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\|()1{}[]?-_+~<>i!lI;:,""^`',. ";
    static string Grad = @"@@%%#&*+=-::..";
    //static string Grad = @"█▓▒░ ";
    static int widthChar;
    static int heightChar;

    static bool withColor = false;
    static bool SimColorOptimizer = false;
    static bool LineOptimizer = true;
    static bool HalfSizePixels = false;

    static int incrementer = 1;

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: TermImageView <image-file>");
            Console.WriteLine("Try: TermImageView help");
            return;
        }
        string filePath = args[0];

        if (filePath == "help" || filePath == "h" || filePath == "settings")
        {
            Console.WriteLine($"this is not done yet.");
            return;
        }

        Console.WriteLine($"You passed: {filePath}");
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found.");
            return;
        }

        Image<Rgba32> image = Image.Load<Rgba32>(filePath);

        widthChar = Console.WindowWidth;
    
        foreach (string arg in args.Skip(1))
        {
            if (arg == "c" || arg == "-c")
            {
                Console.WriteLine("drawing with color");
                withColor = true;
            } else if (arg == "o" || arg == "-o")
            {
                Console.WriteLine("drawing with original size, may not wrap correctly!");
                widthChar = image.Width;
            }
            else if (arg == "e" || arg == "-e")
            {
                Console.WriteLine("color memory optimizer");
                SimColorOptimizer = true;
            }
            else if (arg == "s" || arg == "-s")
            {
                Console.WriteLine("single char write");
                LineOptimizer = false;
            }
            else if (arg == "h" || arg == "-h")
            {
                Console.WriteLine("using Half Size Pixel Write");
                HalfSizePixels = true;
            }
            else if (int.TryParse(arg, out int width))
            {
                if (width <= 0)
                {
                    Console.WriteLine("Width must be positive.");
                    return;
                }
                widthChar = width;
            }
            else if (arg == "-oForce")
            {
                //Console.WriteLine("single char write");
                //LineOptimizer = false;
            }
        }


        float aspectRatio = 0.5f; // tweak this per font/terminal
        if (HalfSizePixels) {
            aspectRatio*=2;
            incrementer=2;
        }

        heightChar = (int)(image.Height * widthChar / (float)image.Width * aspectRatio);
        //heightChar = image.Height * widthChar / image.Width;

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < heightChar; y+=incrementer)
            {
                // Get a Span for the current row
                Span<Rgba32> pixelRow = accessor.GetRowSpan((int)((y/(float)heightChar)*image.Height));
                Span<Rgba32> pixelRowUnder = pixelRow;
                if (HalfSizePixels)
                {
                    if ((int)(((y+1)/(float)heightChar)*image.Height) >= image.Height)
                    {
                        HalfSizePixels = false;
                    } else {
                        pixelRowUnder = accessor.GetRowSpan((int)(((y+1)/(float)heightChar)*image.Height));
                    }
                }

                string TMPlineString = "";
                for (int x = 0; x < widthChar; x++)
                {
                    // Access or modify pixels directly in the Span
                    ref Rgba32 pixel = ref pixelRow[(int)((x/(float)widthChar)*image.Width)];
                    ref Rgba32 pixelUnder = ref pixelRowUnder[(int)((x/(float)widthChar)*image.Width)];
                    //ref Rgba32 pixelN1 = ref pixelRow[(int)((x/(float)widthChar)*image.Width)+1];
                    //if (SimColorOptimizer)
                    //{
                    //    //ref Rgba32 pixelN2 = ref pixelRow[(int)((x/(float)widthChar)*image.Width)+2];
                    //}
                    //float bright = Math.Clamp((pixel.R+pixel.G+pixel.B)/3f,0,255);
                    if (withColor) /// change system, only one draw / one TMPlineString add, so differance is in line optimization and not in draw type? string that saves whatever the string should be?
                    {   
                        string thisPixelDrawString = "";
                        if (HalfSizePixels)
                        {
                            thisPixelDrawString = stringTextColorDuo("▀",pixel.R, pixel.G, pixel.B, pixelUnder.R, pixelUnder.G, pixelUnder.B);
                        } else
                        {
                            thisPixelDrawString = stringTextColor("█", pixel.R, pixel.G, pixel.B);
                        }
                        if(LineOptimizer)
                        {
                            //TMPlineString += stringTextColor("██", (pixel.R+pixelN1.R)/2, (pixel.G+pixelN1.G)/2, (pixel.B+pixelN1.B)/2);
                            //x++;
                            //TMPlineString += stringTextColor("▄", pixel.R, pixel.G, pixel.B);
                            TMPlineString += thisPixelDrawString;
                        } else
                        {
                            Console.Write(thisPixelDrawString);
                        }
                    } else {
                        float bright = 0.299f * pixel.R + 0.587f * pixel.G + 0.114f * pixel.B;
                        if(LineOptimizer)
                        {
                            TMPlineString+= Grad[(Grad.Length - 1)-(int)((Grad.Length - 1)*(bright/255f))]; 
                        }
                        else
                        {
                            Console.Write(Grad[(Grad.Length - 1)-(int)((Grad.Length - 1)*(bright/255f))]); 
                        }
                    }
                        //writeTextColor(Grad[(Grad.Length - 1)-(int)((Grad.Length - 1)*(bright/255f))], pixel.R, pixel.G, pixel.B);

                    //Console.Write(bright);
                    //Console.Write(" ");
                    //pixel = new Rgba32(255, 0, 0, 255); // Change to red
                }
                if (LineOptimizer)
                {
                    Console.WriteLine(TMPlineString);
                } else
                {
                    Console.WriteLine("");
                }
            }
            //Console.WriteLine(Grad.Length);
            //Console.WriteLine(Grad);
            //Console.WriteLine(heightChar);
        });
    }

    static void writeTextColor(string c, int R, int G, int B)
    {
        Console.Write($"\u001b[38;2;{R};{G};{B}m{c}\u001b[0m");
    }
    static string stringTextColor(string c, int R, int G, int B)
    {
        return $"\u001b[38;2;{R};{G};{B}m{c}\u001b[0m";
    }
    static string stringTextColorDuo(string c, int R1, int G1, int B1, int R2, int G2, int B2)
    {
        //return $"\u001b[38;2;{R};{G};{B}m{c}\u001b[0m";
        return $"\u001b[38;2;{R1};{G1};{B1}m\u001b[48;2;{R2};{G2};{B2}m{c}\u001b[0m";
    }
}