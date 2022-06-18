using System.Numerics;
namespace VTF.NET;

/// <summary>
/// Contains VTF image Mipmaps.
/// </summary>
public class VtfImage
{
    public VtfHeader Header;
    public VtfResourceEntryInfo[] Resources;
    public Image Body;
    public Image Thumbnail;

    /// <summary>
    /// Creates a new <see cref="VtfImage"/> instance from a VTF file saved at the specified path.
    /// </summary>
    /// <param name="filePath">The path to the VTF file.</param>
    public VtfImage(string filePath)
    {
        byte[] bytes = File.ReadAllBytes(filePath);
        using (var stream = new MemoryStream(bytes))
        {
            using (var reader = new BinaryReader(stream))
            {
                Header = new VtfHeader(stream);

                Thumbnail = ReadThumbnail(stream);
                reader.BaseStream.Position += GetMipmapBlockSize(Header.Width); // Skip the header and mipmaps to get straight to the pixel Mipmaps.

                Resources = VtfResourceEntryInfo.ReadResources(stream);
                
                ReadImage(stream, Header.Width, Header.Height);
            }
        }
    }
    
    public VtfImage(int width, int height)
    {
        Header = new VtfHeader(width, height);
        Resources = new VtfResourceEntryInfo[0];
        Body = new Image(width, height);
        Thumbnail = new Image(16, 16);
    }

    private Image ReadThumbnail(Stream input)
    {
        using (var reader = new BinaryReader(input))
        {
            return ReadImage(input, 16, 16);
        }
    }

    /// <summary>
    /// Reads a <see cref="VtfImage"/> body from a data stream.
    /// </summary>
    /// <param name="stream"></param>
    private Image ReadImage(Stream stream, int width, int height)
    {
        try
        {
            return new Image(width, height, stream, Header.HighResImageFormat);
        }
        catch (Exception e)
        {
            throw new Exception("Failed to read image data!", e);
        }
    }

    /// <summary>
    /// Saves the image to a VTF file at the specified path.
    /// </summary>
    /// <param name="filePath">File path of the image.</param>
    public void Save(string filePath)
    {
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Header.ToBytes()); // Write the header.
                for (int i = 0; i < Resources.Length; i++)
                {
                    writer.Write(Resources[i].ToBytes()); // Write every resource (usually empty).
                }

                writer.Write(Thumbnail.ToBytes(Header.LowResImageFormat)); // Writer the thumbnail image.
                writer.Write(Body.ToBytes(Header.HighResImageFormat)); // Writer the body image.
            }
        }
    }

    /// <summary>
    /// Calculates the amount of mipmaps necessary to fit the image.
    /// </summary>
    /// <param name="size">Maximum size in pixels.</param>
    /// <returns></returns>
    public static int EnumerateMipmaps(int size)
    {
        int mipmapCount = 0;
        while (size > 1)
        {
            size /= 2;
            mipmapCount++;
        }
        return mipmapCount;
    }

    /// <summary>
    /// Calculates the size of all image mipmaps in bytes given an input resolution.
    /// </summary>
    /// <param name="size">The image width/height in pixels.</param>
    /// <returns>Size off all bytes making up a mipmap block.</returns>
    public static int GetMipmapBlockSize(int size)
    {
        int result = 0;

        for (int i = 0; i <= EnumerateMipmaps(size); i++)
        {
            result += (int)Math.Pow(2, i) * (int)Math.Pow(2, i);
        }
        return result - 1;
    }
}

public class VtfHeader
{
    public uint[] Version = new uint[2];       // version[0].version[1] (currently 7.2).
    public uint HeaderSize;                    // Size of the header struct  (16 byte aligned; currently 80 bytes) + size of the resources dictionary (7.3+).
    public ushort Width;                       // Width of the largest mipmap in pixels. Must be a power of 2.
    public ushort Height;                      // Height of the largest mipmap in pixels. Must be a power of 2.
    public uint Flags;                         // VTF flags.
    public ushort Frames;                      // Number of frames, if animated (1 for no animation).
    public ushort FirstFrame;                  // First frame in animation (0 based). Can be -1 in environment maps older than 7.5, meaning there are 7 faces, not 6.
    public Vector3 Reflectivity;               // Reflectivity vector.
    public float BumpmapScale;                 // Bumpmap scale.
    public VtfImageFormat HighResImageFormat;  // High resolution image format.
    public byte MipmapCount;                   // Number of mipmaps.
    public VtfImageFormat LowResImageFormat;   // Low resolution image format (always DXT1).
    public byte LowResImageWidth;              // Low resolution image width.
    public byte LowResImageHeight;             // Low resolution image height.
    // 7.2+
    public ushort Depth;                       // Depth of the largest mipmap in pixels. Must be a power of 2. Is 1 for a 2D texture.
    // 7.3+
    public uint NumResources;                  // Number of resources this vtf has. The max appears to be 32.

    /// <summary>
    /// Creates an empty header instance.
    /// </summary>
    public VtfHeader(int width, int height)
    {
        Version[0] = 7U;
        Version[1] = 2U;
        HeaderSize = 80U;
        Width = (ushort)width;
        Height = (ushort)height;
        Flags = 0U;
        Frames = (ushort)1U;
        FirstFrame = (ushort)0U;
        Reflectivity = new Vector3(1.0f, 0.0f, 0.0f);
        BumpmapScale = 1.0f;
        HighResImageFormat = VtfImageFormat.RGBA8888;
        MipmapCount = (byte)VtfImage.EnumerateMipmaps(Width);
        LowResImageFormat = VtfImageFormat.DXT1;
        LowResImageWidth = (byte)16U;
        LowResImageHeight = (byte)16U;
        Depth = (ushort)1U;
        NumResources = 0U;
    }


    /// <summary>
    /// Creates an empty header instance.
    /// </summary>
    public VtfHeader(VtfImage image)
    {
        Version[0] = 7U;
        Version[1] = 2U;
        HeaderSize = 80U;
        Width = (ushort)image.Body.Width;
        Height = (ushort)image.Body.Height;
        Flags = 0U;
        Frames = (ushort)1U;
        FirstFrame = (ushort)0U;
        Reflectivity = new Vector3(1.0f, 0.0f, 0.0f);
        BumpmapScale = 1.0f;
        HighResImageFormat = VtfImageFormat.RGBA8888;
        MipmapCount = (byte)VtfImage.EnumerateMipmaps(Width);
        LowResImageFormat = VtfImageFormat.DXT1;
        LowResImageWidth = (byte)16U;
        LowResImageHeight = (byte)16U;
        Depth = (ushort)1U;
        NumResources = 0U;
    }

    /// <summary>
    /// Reads a new <see cref="VtfHeader"/> from a binary stream.
    /// </summary>
    /// <param name="Mipmaps">Input stream.</param>
    public VtfHeader(Stream stream)
    {
        using (var reader = new BinaryReader(stream))
        {
            var b = reader.ReadBytes(4);
            if (!b.SequenceEqual(new byte[] { 86, 84, 70, 0 }))
                throw new Exception("Invalid VTF file.");
            Version[0] = reader.ReadUInt32();
            Version[1] = reader.ReadUInt32();
            HeaderSize = reader.ReadUInt32();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            Flags = reader.ReadUInt32();
            Frames = reader.ReadUInt16();
            FirstFrame = reader.ReadUInt16();
            reader.ReadBytes(4);
            Reflectivity = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            reader.ReadBytes(4);
            BumpmapScale = reader.ReadSingle();
            HighResImageFormat = (VtfImageFormat)reader.ReadUInt32();
            MipmapCount = reader.ReadByte();
            LowResImageFormat = (VtfImageFormat)reader.ReadUInt32();
            LowResImageWidth = reader.ReadByte();
            LowResImageHeight = reader.ReadByte();
            Depth = reader.ReadUInt16();
            reader.ReadBytes(3);
            NumResources = reader.ReadUInt32();
            reader.ReadBytes(8);
        }
    }

    /// <summary>
    /// Sets a VTF flag on the current <see cref="VtfHeader"/> instance.
    /// </summary>
    /// <param name="flag">The flag parameter.</param>
    /// <param name="value">State of the flag.</param>
    public void SetFlag(VtfImageFlag flag, bool value)
    {
        // VTF flags are compiled using bitwise operations.
        // If you wanted to enable every option, you'd end up with the flag value \xAAAAAAAA.
        if (value)
            Flags |= (uint)flag;
        else
            Flags &= ~(uint)flag;
    }

    /// <summary>
    /// Gets a VTF flag from the current <see cref="VtfHeader"/> instance.
    /// </summary>
    /// <param name="flag">The flag state to look up.</param>
    /// <returns>The value of the flag as referenced by the VTF header.</returns>
    public bool GetFlag(VtfImageFlag flag)
    {
        return (Flags & (uint)flag) != 0;
    }

    /// <summary>
    /// Converts the current <see cref="VtfHeader"/> instance to a byte array.
    /// </summary>
    /// <returns></returns>
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(new byte[] { 0x56, 0x54, 0x46, 0x00 });
                writer.Write(Version[0]);
                writer.Write(Version[1]);
                writer.Write(HeaderSize);
                writer.Write(Width);
                writer.Write(Height);
                writer.Write(Flags);
                writer.Write(Frames);
                writer.Write(FirstFrame);
                writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 });
                writer.Write(Reflectivity.X);
                writer.Write(Reflectivity.Y);
                writer.Write(Reflectivity.Z);
                writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 });
                writer.Write(BumpmapScale);
                writer.Write((uint)HighResImageFormat);
                writer.Write(MipmapCount);
                writer.Write((uint)LowResImageFormat);
                writer.Write(LowResImageWidth);
                writer.Write(LowResImageHeight);
                writer.Write(Depth);
                writer.Write(new byte[] { 0x00, 0x00, 0x00 });
                writer.Write(NumResources);
                writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                return stream.ToArray();
            }
        }
    }
}

public class VtfResourceEntryInfo
{
    byte[] Tag = new byte[3];   // A three-byte "tag" that identifies what this resource is.
    byte Flags;                 // Resource entry flags. The only known flag is 0x2, which indicates that no Mipmaps chunk corresponds to this resource.
    uint Offset;                // The offset of this resource's Mipmaps in the file. 

    /// <summary>
    /// Reads a new <see cref="VtfResourceEntryInfo"/> from a byte array.
    /// </summary>
    /// <param name="Mipmaps"></param>
    public VtfResourceEntryInfo(byte[] tag, byte flags, uint offset)
    {
        Tag = tag;
        Flags = flags;
        Offset = offset;
    }

    public static VtfResourceEntryInfo[] ReadResources(Stream stream)
    {
        using (var reader = new BinaryReader(stream))
        {
            var header = new VtfHeader(stream);
            var list = new List<VtfResourceEntryInfo>();
            reader.BaseStream.Position += header.HeaderSize;
            for (int i = 0; i < header.NumResources; i++)
            {
                var a = reader.ReadBytes(3);
                var b = reader.ReadByte();
                var c = reader.ReadUInt32();

                var info = new VtfResourceEntryInfo(a, b, c);
                list.Add(info);
            }
            return list.ToArray();
        }
    }

    /// <summary>
    /// Converts the current <see cref="VtfResourceEntryInfo"/> instance to a byte array.
    /// </summary>
    /// <returns></returns>
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Tag);
                writer.Write(Flags);
                writer.Write(Offset);
                return stream.ToArray();
            }
        }
    }
};
