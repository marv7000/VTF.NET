namespace VTF.NET;

/// <summary>
/// Stores a <see cref="VtfImage"/>'s compression or image format.
/// </summary>
public enum VtfImageFormat
{
    None = -1,
	RGBA8888 = 0,
	ABGR8888 = 1,
	RGB888,
	BGR888,
	RGB565,
	I8,
	IA88,
	P8,
	A8,
	RGB888_BLUESCREEN,
	BGR888_BLUESCREEN,
	ARGB8888,
	BGRA8888,
	DXT1,
	DXT3,
	DXT5,
	BGRX8888,
	BGR565,
	BGRX5551,
	BGRA4444,
	DXT1_ONEBITALPHA,
	BGRA5551,
	UV88,
	UVWQ8888,
	RGBA16161616F,
	RGBA16161616,
	UVLX8888
}
