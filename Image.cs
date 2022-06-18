using System.Drawing;

namespace VTF.NET;

/// <summary>
/// Defines Image data.
/// </summary>
public class Image
{
    public int Width;
    public int Height;
    public Color[] Colors;

    public Image(string path)
    {
        using (var image = new Bitmap(path))
        {
            Width = image.Width;
            Height = image.Height;
            Colors = new Color[Width * Height];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Colors[i + j * Width].R = image.GetPixel(i, j).R;
                    Colors[i + j * Width].G = image.GetPixel(i, j).G;
                    Colors[i + j * Width].B = image.GetPixel(i, j).B;
                    Colors[i + j * Width].A = image.GetPixel(i, j).A;
                }
            }
        }
        
    }

    public Image(int width, int height)
    {
        Width = width;
        Height = height;
        Colors = new Color[width * height];
    }

    public Image(int width, int height, Color[] colors)
    {
        Width = width;
        Height = height;
        if (width * height == colors.Length)
            Colors = colors;
        else
            throw new ArgumentException("More or less colors than available pixels provided.");
    }

    public Image(int width, int height, Stream input, VtfImageFormat format)
    {
        Width = width;
        Height = height;
        Colors = new Color[width * height];
        
        for (int i = 0; i < width*height; i++)
        {
            Colors[i] = new Color(input, format);
        }
    }

    /// <summary>
    /// Fills an entire image with a solid Color.
    /// </summary>
    public void Fill(byte r, byte g, byte b, byte a)
    {
        for (int i = 0; i < Colors.Length; i++)
        {
            Colors[i] = new Color(r, g, b, a);
        }
    }

    /// <summary>
    /// Fills an entire channel with a solid value.
    /// 
    public void FillChannel(byte channel, byte value)
    {
        for (int i = 0; i < Colors.Length; i++)
        {
            switch (channel)
            {
                case 0:
                    Colors[i].R = value;
                    break;
                case 1:
                    Colors[i].G = value;
                    break;
                case 2:
                    Colors[i].B = value;
                    break;
                case 3:
                    Colors[i].A = value;
                    break;
                default:
                    break;
            }
        }
    }

    public Image GenerateMipmap()
    {
        int width = Width >> 1;
        int height = Height >> 1;
        Color[] colors = new Color[width * height];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Colors[i * 2];
        }

        return new Image(width, height, colors);
    }

    public byte[] ToBytes(VtfImageFormat format)
    {
        using (var stream = new MemoryStream())
        { 
            using (var writer = new BinaryWriter(stream))
            {
                for (int i = 0; i < Colors.Length; i++)
                {
                    writer.Write(Colors[i].ToBytes(format));
                }
            }
            return stream.ToArray();
        }
    }
}

public struct Color
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public Color(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(byte r, byte g, byte b, byte a, VtfImageFormat format)
    {
        switch (format)
        {
            case VtfImageFormat.RGBA8888:
                R = r; G = g; B = b; A = a;
                break;
            case VtfImageFormat.BGRA8888:
                B = r; G = g; R = b; A = a;
                break;
            case VtfImageFormat.ABGR8888:
                A = r; B = g; G = b; R = a;
                break;
            case VtfImageFormat.ARGB8888:
                A = r; R = g; G = b; B = a;
                break;
            case VtfImageFormat.RGB888:
                R = r; G = g; B = b; A = 255;
                break;
            case VtfImageFormat.BGR888:
                B = r; G = g; R = b; A = 255;
                break;
            case VtfImageFormat.RGB888_BLUESCREEN:
                R = r; G = g; B = b; A = 255;
                break;
            case VtfImageFormat.BGR888_BLUESCREEN:
                B = r; G = g; R = b; A = 255;
                break;
            case VtfImageFormat.RGB565:
            case VtfImageFormat.I8:
            case VtfImageFormat.IA88:
            case VtfImageFormat.P8:
                throw new NotSupportedException("Paletted images are not supported by VTF.NET!"); // According to Valve spec.
            case VtfImageFormat.A8:
            case VtfImageFormat.DXT1:
            case VtfImageFormat.DXT3:
            case VtfImageFormat.DXT5:
            case VtfImageFormat.BGRX8888:
            case VtfImageFormat.BGR565:
            case VtfImageFormat.BGRX5551:
            case VtfImageFormat.BGRA4444:
            case VtfImageFormat.DXT1_ONEBITALPHA:
            case VtfImageFormat.BGRA5551:
            case VtfImageFormat.UV88:
            case VtfImageFormat.UVWQ8888:
            case VtfImageFormat.RGBA16161616F:
            case VtfImageFormat.RGBA16161616:
            case VtfImageFormat.UVLX8888:
            default:
                throw new ArgumentException();
        }
    }

    public Color(Stream input, VtfImageFormat format)
    {
        using (var reader = new BinaryReader(input))
        {
            switch (format)
            {
                case VtfImageFormat.RGBA8888:
                    R = reader.ReadByte();
                    G = reader.ReadByte();
                    B = reader.ReadByte();
                    A = reader.ReadByte();
                    break;
                case VtfImageFormat.BGRA8888:
                    B = reader.ReadByte();
                    G = reader.ReadByte();
                    R = reader.ReadByte();
                    A = reader.ReadByte();
                    break;
                case VtfImageFormat.ABGR8888:
                    A = reader.ReadByte();
                    B = reader.ReadByte();
                    G = reader.ReadByte();
                    R = reader.ReadByte();
                    break;
                case VtfImageFormat.ARGB8888:
                    A = reader.ReadByte();
                    R = reader.ReadByte();
                    G = reader.ReadByte();
                    B = reader.ReadByte();
                    break;
                case VtfImageFormat.RGB888:
                    R = reader.ReadByte();
                    G = reader.ReadByte();
                    B = reader.ReadByte();
                    A = 255;
                    break;
                case VtfImageFormat.BGR888:
                    B = reader.ReadByte();
                    G = reader.ReadByte();
                    R = reader.ReadByte();
                    A = 255;
                    break;
                case VtfImageFormat.RGB888_BLUESCREEN:
                    R = reader.ReadByte();
                    G = reader.ReadByte();
                    B = reader.ReadByte();
                    A = 255;
                    break;
                case VtfImageFormat.BGR888_BLUESCREEN:
                    B = reader.ReadByte();
                    G = reader.ReadByte();
                    R = reader.ReadByte();
                    A = 255;
                    break;
                case VtfImageFormat.RGB565:
                case VtfImageFormat.I8:
                case VtfImageFormat.IA88:
                case VtfImageFormat.P8:
                    throw new NotSupportedException("Paletted images are not supported by VTF.NET!"); // According to Valve spec.
                case VtfImageFormat.A8:
                case VtfImageFormat.DXT1:
                case VtfImageFormat.DXT3:
                case VtfImageFormat.DXT5:
                case VtfImageFormat.BGRX8888:
                case VtfImageFormat.BGR565:
                case VtfImageFormat.BGRX5551:
                case VtfImageFormat.BGRA4444:
                case VtfImageFormat.DXT1_ONEBITALPHA:
                case VtfImageFormat.BGRA5551:
                case VtfImageFormat.UV88:
                case VtfImageFormat.UVWQ8888:
                case VtfImageFormat.RGBA16161616F:
                case VtfImageFormat.RGBA16161616:
                case VtfImageFormat.UVLX8888:
                default:
                    throw new ArgumentException();
            }
        }
    }
    public byte[] ToBytes(VtfImageFormat format)
    {
        using (var stream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(stream))
            {
                switch (format)
                {
                    case VtfImageFormat.RGBA8888:
                        writer.Write(R);
                        writer.Write(G);
                        writer.Write(B);
                        writer.Write(A);
                        break;
                    case VtfImageFormat.BGRA8888:
                        writer.Write(B);
                        writer.Write(G);
                        writer.Write(R);
                        writer.Write(A);
                        break;
                    case VtfImageFormat.ABGR8888:
                        writer.Write(A);
                        writer.Write(B);
                        writer.Write(G);
                        writer.Write(R);
                        break;
                    case VtfImageFormat.ARGB8888:
                        writer.Write(A);
                        writer.Write(R);
                        writer.Write(G);
                        writer.Write(B);
                        break;
                    case VtfImageFormat.RGB888:
                        writer.Write(R);
                        writer.Write(G);
                        writer.Write(B);
                        break;
                    case VtfImageFormat.BGR888:
                        writer.Write(B);
                        writer.Write(G);
                        writer.Write(R);
                        break;
                    case VtfImageFormat.RGB888_BLUESCREEN:
                        writer.Write(R);
                        writer.Write(G);
                        writer.Write(B);
                        break;
                    case VtfImageFormat.BGR888_BLUESCREEN:
                        writer.Write(B);
                        writer.Write(G);
                        writer.Write(R);
                        break;
                    case VtfImageFormat.RGB565:
                    case VtfImageFormat.I8:
                    case VtfImageFormat.IA88:
                    case VtfImageFormat.P8:
                        throw new NotSupportedException("Paletted images are not supported by VTF.NET!"); // According to Valve spec.
                    case VtfImageFormat.A8:
                    case VtfImageFormat.DXT1:
                    case VtfImageFormat.DXT3:
                    case VtfImageFormat.DXT5:
                    case VtfImageFormat.BGRX8888:
                    case VtfImageFormat.BGR565:
                    case VtfImageFormat.BGRX5551:
                    case VtfImageFormat.BGRA4444:
                    case VtfImageFormat.DXT1_ONEBITALPHA:
                    case VtfImageFormat.BGRA5551:
                    case VtfImageFormat.UV88:
                    case VtfImageFormat.UVWQ8888:
                    case VtfImageFormat.RGBA16161616F:
                    case VtfImageFormat.RGBA16161616:
                    case VtfImageFormat.UVLX8888:
                    default:
                        throw new ArgumentException();
                }
            }
            return stream.ToArray();
        }
    }
}
