using System;
using System.Runtime.InteropServices;

namespace LevelImposter.Core
{
    public enum FREE_IMAGE_FORMAT
    {
        FIF_UNKNOWN = -1,
        FIF_BMP = 0,
        FIF_ICO = 1,
        FIF_JPEG = 2,
        FIF_JNG = 3,
        FIF_KOALA = 4,
        FIF_LBM = 5,
        FIF_IFF = 5,
        FIF_MNG = 6,
        FIF_PBM = 7,
        FIF_PBMRAW = 8,
        FIF_PCD = 9,
        FIF_PCX = 10,
        FIF_PGM = 11,
        FIF_PGMRAW = 12,
        FIF_PNG = 13,
        FIF_PPM = 14,
        FIF_PPMRAW = 15,
        FIF_RAS = 16,
        FIF_TARGA = 17,
        FIF_TIFF = 18,
        FIF_WBMP = 19,
        FIF_PSD = 20,
        FIF_CUT = 21,
        FIF_XBM = 22,
        FIF_XPM = 23,
        FIF_DDS = 24,
        FIF_GIF = 25,
        FIF_HDR = 26,
        FIF_FAXG3 = 27,
        FIF_SGI = 28,
        FIF_EXR = 29,
        FIF_J2K = 30,
        FIF_JP2 = 31,
        FIF_PFM = 32,
        FIF_PICT = 33,
        FIF_RAW = 34,
        FIF_WEBP = 35,
        FIF_JXR = 36
    }

    public enum FREE_IMAGE_LOAD_FLAGS
    {
        DEFAULT = 0,
        GIF_LOAD256 = 1,
        GIF_PLAYBACK = 2,
        ICO_MAKEALPHA = 1,
        JPEG_FAST = 1,
        JPEG_ACCURATE = 2,
        JPEG_CMYK = 4,
        JPEG_EXIFROTATE = 8,
        PCD_BASE = 1,
        PCD_BASEDIV4 = 2,
        PCD_BASEDIV16 = 3,
        PNG_IGNOREGAMMA = 1,
        TARGA_LOAD_RGB888 = 1,
        TIFF_CMYK = 1,
        RAW_PREVIEW = 1,
        RAW_DISPLAY = 2,
        RAW_HALFSIZE = 4,
        RAW_UNPROCESSED = 8,
        FIF_LOAD_NOPIXELS = 0x8000
    }

    public enum FREE_IMAGE_MDMODEL
    {
        FIMD_NODATA = -1,
        FIMD_COMMENTS = 0,
        FIMD_EXIF_MAIN = 1,
        FIMD_EXIF_EXIF = 2,
        FIMD_EXIF_GPS = 3,
        FIMD_EXIF_MAKERNOTE = 4,
        FIMD_EXIF_INTEROP = 5,
        FIMD_IPTC = 6,
        FIMD_XMP = 7,
        FIMD_GEOTIFF = 8,
        FIMD_ANIMATION = 9,
        FIMD_CUSTOM = 10
    }

    public class FreeImage
    {
        private const string FreeImageLibrary = "FreeImage";

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_Load")]
        public static extern IntPtr FreeImage_Load(FREE_IMAGE_FORMAT format, string filename, int flags);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_GetFileTypeFromMemory")]
        public static extern FREE_IMAGE_FORMAT FreeImage_GetFileTypeFromMemory(IntPtr stream, int size_in_bytes);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_OpenMemory")]
        public static extern IntPtr FreeImage_OpenMemory(IntPtr data, uint size_in_bytes);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_CloseMemory")]
        public static extern IntPtr FreeImage_CloseMemory(IntPtr data);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_AcquireMemory")]
        public static extern bool FreeImage_AcquireMemory(IntPtr stream, ref IntPtr data, ref uint size_in_bytes);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_LoadFromMemory")]
        public static extern IntPtr FreeImage_LoadFromMemory(FREE_IMAGE_FORMAT format, IntPtr stream, int flags);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_LoadMultiBitmapFromMemory")]
        public static extern IntPtr FreeImage_LoadMultiBitmapFromMemory(FREE_IMAGE_FORMAT format, IntPtr stream, int flags);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_LockPage")]
        public static extern IntPtr FreeImage_LockPage(IntPtr multiHandle, int page);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_UnlockPage")]
        public static extern void FreeImage_UnlockPage(IntPtr multiHandle, IntPtr handle, bool isChanged);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_Unload")]
        public static extern void FreeImage_Unload(IntPtr dib);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_CloseMultiBitmap")]
        public static extern bool FreeImage_CloseMultiBitmap(IntPtr handle, int flags);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_Save")]
        public static extern bool FreeImage_Save(FREE_IMAGE_FORMAT format, IntPtr handle, string filename, int flags);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_SaveToMemory")]
        public static extern bool FreeImage_SaveToMemory(FREE_IMAGE_FORMAT format, IntPtr dib, IntPtr stream, int flags);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_ConvertToRawBits")]
        public static extern void FreeImage_ConvertToRawBits(IntPtr bits, IntPtr dib, int pitch, uint bpp, uint red_mask, uint green_mask, uint blue_mask, bool topdown);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_ConvertToRawBits")]
        public static extern void FreeImage_ConvertToRawBits(byte[] bits, IntPtr dib, int pitch, uint bpp, uint red_mask, uint green_mask, uint blue_mask, bool topdown);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_ConvertTo32Bits")]
        public static extern IntPtr FreeImage_ConvertTo32Bits(IntPtr handle);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_GetWidth")]
        public static extern uint FreeImage_GetWidth(IntPtr handle);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_GetHeight")]
        public static extern uint FreeImage_GetHeight(IntPtr handle);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_GetPageCount")]
        public static extern int FreeImage_GetPageCount(IntPtr handle);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_GetMetadata")]
        public static extern bool FreeImage_GetMetadata(FREE_IMAGE_MDMODEL model, IntPtr handle, string key, out IntPtr tag);
    
        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_GetTagValue")]
        public static extern IntPtr FreeImage_GetTagValue(IntPtr tag);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_GetVersion")]
        public static extern IntPtr FreeImage_GetVersion();
    }
}
