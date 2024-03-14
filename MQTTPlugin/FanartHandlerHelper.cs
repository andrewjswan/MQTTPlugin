using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace MQTTPlugin
{
  public static class FanartHandlerHelper
  {
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static string GetFanartTVForLatestMedia(string key1, string key2, string key3, FanartTV category)
    {
      if (Utils.IsAssemblyAvailable("FanartHandler", new Version(4, 0, 0, 0)))
      {
        return string.Empty;
      }

      try
      {
        return FanartHandler.ExternalAccess.GetFanartTVForLatestMedia(key1, key2, key3, category.ToString());
      }
      catch (FileNotFoundException) { }
      catch (MissingMethodException)
      {
        Logger.Error("GetFanartTVForLatestMedia: Update Fanart Handler plugin.");
      }
      catch (Exception ex)
      {
        Logger.Error("GetFanartTVForLatestMedia: Possible: Update Fanart Handler plugin.");
        Logger.Debug("GetFanartTVForLatestMedia: " + ex.ToString());
      }
      return string.Empty;
    }

    public enum FanartTV
    {
      MusicThumb,
      MusicBackground,
      MusicCover,
      MusicClearArt,
      MusicBanner,
      MusicCDArt,
      MusicLabel,
      MoviesPoster,
      MoviesBackground,
      MoviesClearArt,
      MoviesBanner,
      MoviesClearLogo,
      MoviesCDArt,
      MoviesCollectionPoster,
      MoviesCollectionBackground,
      MoviesCollectionClearArt,
      MoviesCollectionBanner,
      MoviesCollectionClearLogo,
      MoviesCollectionCDArt,
      SeriesPoster,
      SeriesThumb,
      SeriesBackground,
      SeriesBanner,
      SeriesClearArt,
      SeriesClearLogo,
      SeriesCDArt,
      SeriesSeasonPoster,
      SeriesSeasonThumb,
      SeriesSeasonBanner,
      SeriesSeasonCDArt,
      SeriesCharacter,
      None,
    }

  }
}
