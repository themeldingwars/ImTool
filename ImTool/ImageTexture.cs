using System;
using System.IO;
using StbImageSharp;
using Veldrid;

namespace ImTool;

public class ImageTexture
{
    /// <summary>
    /// The RGBA pixels of the largest mip level, 4 bytes per pixel.
    /// </summary>
    public byte[] Pixels { get; }

    public uint Width { get; }
    public uint Height { get; }
    public PixelFormat Format { get; }

    /// <summary>
    /// The number of mip levels the device texture gets, 1 if mipmapping is disabled.
    /// Levels below the first are generated on the GPU.
    /// </summary>
    public uint MipLevels { get; }

    public ImageTexture(string path, bool mipmap = true, bool srgb = false)
        : this(File.ReadAllBytes(path), mipmap, srgb) { }

    public ImageTexture(Stream stream, bool mipmap = true, bool srgb = false)
        : this(ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha), mipmap, srgb) { }

    public ImageTexture(byte[] encoded, bool mipmap = true, bool srgb = false)
        : this(ImageResult.FromMemory(encoded, ColorComponents.RedGreenBlueAlpha), mipmap, srgb) { }

    public ImageTexture(byte[] rgba, uint width, uint height, bool mipmap = true, bool srgb = false)
    {
        if (rgba.Length != width * height * 4)
            throw new ArgumentException($"Expected {width * height * 4} bytes of RGBA data, got {rgba.Length}.", nameof(rgba));

        Pixels = rgba;
        Width = width;
        Height = height;
        Format = srgb ? PixelFormat.R8_G8_B8_A8_UNorm_SRgb : PixelFormat.R8_G8_B8_A8_UNorm;
        MipLevels = mipmap ? 1 + (uint)Math.Floor(Math.Log2(Math.Max(width, height))) : 1;
    }

    private ImageTexture(ImageResult image, bool mipmap, bool srgb)
        : this(image.Data, (uint)image.Width, (uint)image.Height, mipmap, srgb) { }

    public Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory factory)
    {
        var usage = TextureUsage.Sampled;
        if (MipLevels > 1)
            usage |= TextureUsage.GenerateMipmaps;

        Texture texture = factory.CreateTexture(TextureDescription.Texture2D(Width, Height, MipLevels, 1, Format, usage));
        gd.UpdateTexture(texture, Pixels, 0, 0, 0, Width, Height, 1, 0, 0);

        if (MipLevels > 1)
        {
            using CommandList cl = factory.CreateCommandList();
            cl.Begin();
            cl.GenerateMipmaps(texture);
            cl.End();
            gd.SubmitCommands(cl);
        }

        return texture;
    }
}
