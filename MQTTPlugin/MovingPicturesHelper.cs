using System;

using MediaPortal.Plugins.MovingPictures.Database;

namespace MQTTPlugin
{
  public static class MovingPicturesHelper
  {
    public static LatestMediaHandler.MQTTItem CheckDB(string SearchFile)
    {
      LatestMediaHandler.MQTTItem item = new LatestMediaHandler.MQTTItem();
      if (MQTTPlugin.DebugMode) Logger.Debug("Check to see if video is in MovingPictures database.");

      if (Utils.IsAssemblyAvailable("MovingPictures", new Version(1, 0, 6, 1116)))
      {
        if (MQTTPlugin.DebugMode) Logger.Debug("MovingPictures found, searching Database for: " + SearchFile);

        if (SearchFile.IndexOf(".MPLS") != -1) // Blu-Ray played with BDHandler
        {
          if (MQTTPlugin.DebugMode) Logger.Debug("Blu-Ray being played with BDHandler, converting filename.");
          int BDMVindex;
          string OldFile = SearchFile;
          BDMVindex = SearchFile.IndexOf("\\BDMV\\");
          if (BDMVindex != -1)
          {
            SearchFile = SearchFile.Substring(0, BDMVindex + 6) + "INDEX.BDMV";
          }
          if (MQTTPlugin.DebugMode) Logger.Debug("Filename converted from: " + OldFile + " to: " + SearchFile);
        }

        if (MQTTPlugin.DebugMode) Logger.Debug("Searching Database for: " + SearchFile);
        DBLocalMedia Matches = DBLocalMedia.Get(SearchFile);
        if (Matches.AttachedMovies.Count > 0)
        {
          if (MQTTPlugin.DebugMode) Logger.Debug("Found " + Matches.AttachedMovies.Count.ToString() + " matches.");
          DBMovieInfo moviematch = Matches.AttachedMovies[0];
          item.Id = moviematch.ImdbID;
          item.Title = moviematch.Title + " (" + moviematch.Year + ")";
          item.Filename = SearchFile;
          item.Genres = moviematch.Genres.ToString();
          item.GetArtwork("movie");
        }
      }
      return item;
    }
  }
}
