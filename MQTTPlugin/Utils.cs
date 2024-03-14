using MediaPortal.GUI.Library;
using MediaPortal.Util;

using Newtonsoft.Json.Serialization;

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;

namespace MQTTPlugin
{
  public static class Utils
  {
    internal static Hashtable TVIDCache;
    internal static Hashtable ImageCache;

    public static string Language;
    public static string[] PipesArray = new string[1] { "|" };

    internal static bool IsAssemblyAvailable(string name, Version ver)
    {
      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (Assembly a in assemblies)
      {
        try
        {
          if (a.GetName().Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
          {
            return (ver == null || a.GetName().Version >= ver);
          }
        }
        catch { }
      }
      return false;
    }

    public static string GetLang()
    {
      string lang = string.Empty;
      try
      {
        lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
      }
      catch (Exception)
      {
        lang = CultureInfo.CurrentUICulture.Name;
      }
      if (string.IsNullOrEmpty(lang))
      {
        lang = "EN";
      }
      return lang;
    }

    /// <summary>
    /// Loads an Image from a File by invoking GDI Plus instead of using build-in .NET methods, or falls back to Image.FromFile
    /// Can perform up to 10x faster
    /// </summary>
    /// <param name="filename">The filename to load</param>
    /// <returns>A .NET Image object</returns>
    public static Image LoadImageFastFromFile(string filename)
    {
      filename = Path.GetFullPath(filename);
      if (!File.Exists(filename))
      {
        return null;
      }

      Image imageFile = null;
      try
      {
        try
        {
          imageFile = ImageFast.FromFile(filename);
        }
        catch (Exception)
        {
          Logger.Debug("LoadImageFastFromFile: Reverting to slow ImageLoading for: " + filename);
          imageFile = Image.FromFile(filename);
        }
      }
      catch (FileNotFoundException fe)
      {
        Logger.Debug("LoadImageFastFromFile: Image does not exist: " + filename + " - " + fe.Message);
        return null;
      }
      catch (Exception e)
      {
        // this probably means the image is bad
        Logger.Debug("LoadImageFastFromFile: Unable to load Imagefile (corrupt?): " + filename + " - " + e.Message);
        return null;
      }
      return imageFile;
    }

    /// <summary>
    /// Resizes an image
    /// </summary>
    /// <param name="originalImage">The image to resize</param>
    /// <param name="newWidth">The new width in pixels</param>
    /// <param name="newHeight">The new height in pixels</param>
    /// <returns>A resized version of the original image</returns>
    public static Image Resize(this Image originalImage, int newWidth, int newHeight)
    {
      Image smallVersion = new Bitmap(newWidth, newHeight);
      using (Graphics g = Graphics.FromImage(smallVersion))
      {
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.DrawImage(originalImage, 0, 0, newWidth, newHeight);
      }
      return smallVersion;
    }

    internal static string ImageToDataImage(string Filename, int width, int height)
    {
      string filename = GetThemedSkinFile(Filename, @"\Media\");
      if (File.Exists(filename))
      {
        string ext = Path.GetExtension(filename);
        if (!string.IsNullOrEmpty(ext))
        {
          string tempFilename = Path.GetTempFileName();

          Image img = LoadImageFastFromFile(filename);
          img = img.Resize(width, height);
          img.Save(tempFilename);
          img.Dispose();

          string dataImg = @"data:image/" + ext.Substring(1).ToLowerInvariant() + ";base64," + Convert.ToBase64String(File.ReadAllBytes(tempFilename));

          MediaPortal.Util.Utils.FileDelete(tempFilename);
          return dataImg;
        }
      }
      return string.Empty;
    }

    /// <summary>
    /// Return a themed version of the requested skin filename, or default skin filename, otherwise return the filename.  Use a path to media to get images.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    internal static string GetThemedSkinFile(string filename, string folder)
    {
      if (File.Exists(filename)) // sometimes filename is full path, don't know why
      {
        return filename;
      }
      else
      {
        return File.Exists(GUIGraphicsContext.Theme + folder + filename) ?
                 GUIGraphicsContext.Theme + folder + filename :
                 File.Exists(GUIGraphicsContext.Skin + folder + filename) ?
                   GUIGraphicsContext.Skin + folder + filename :
                   filename;
      }
    }

    internal static string DownloadJson(string URL)
    {
      try
      {
        WebClient wc = new WebClient();
        // .NET 4.0: Use TLS v1.2. Many download sources no longer support the older and now insecure TLS v1.0/1.1 and SSL v3.
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xc00;
        return wc.DownloadString(URL);
      }
      catch (WebException we)
      {
        Logger.Debug("DownloadJson: " + we);
      }
      return string.Empty;
    }

    public class LowercaseContractResolver : DefaultContractResolver
    {
      protected override string ResolvePropertyName(string propertyName)
      {
        return propertyName.ToLower();
      }
    }
  }
}
