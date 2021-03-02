using MediaPortal.Video.Database;

using System;

namespace MQTTPlugin
{
  public static class MyVideoHelper
  {
    public static LatestMediaHandler.MQTTItem CheckDB(string SearchFile)
    {
      LatestMediaHandler.MQTTItem item = new LatestMediaHandler.MQTTItem();
      if (MQTTPlugin.DebugMode) Logger.Debug("Check to see if video is in MyVideos database.");

      if (MQTTPlugin.DebugMode) Logger.Debug("MyVideo found, searching Database for: " + SearchFile);
      try
      {
        IMDBMovie movie = new IMDBMovie();
        int movieID = VideoDatabase.GetMovieId(SearchFile);
        VideoDatabase.GetMovieInfoById(movieID, ref movie);
        if (movie.ID > 0)
        {
          if (MQTTPlugin.DebugMode) Logger.Debug("Video is in MyVideos database.");
          item.Id = movie.IMDBNumber;
          item.Title = movie.Title + " (" + movie.Year + ")";
          item.Filename = SearchFile;
          item.Genres = movie.Genre;
          item.GetArtwork("movie");
        }
      }
      catch (Exception e)
      {
        Logger.Error("Error getting info from MyVideo Database: " + e.Message);
      }

      return item;
    }
  }
}
