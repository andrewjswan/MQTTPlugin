using MediaPortal.GUI.Video;
using MediaPortal.Util;
using MediaPortal.Video.Database;

using System;
using System.Collections;

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
          item.Poster = FanartHandlerHelper.GetFanartTVForLatestMedia(movie.Title, movie.ID.ToString(), null, FanartHandlerHelper.FanartTV.MoviesPoster);
          item.Fanart = FanartHandlerHelper.GetFanartTVForLatestMedia(movie.Title, movie.ID.ToString(), null, FanartHandlerHelper.FanartTV.MoviesBackground);
          item.GetArtwork("movie");
        }
      }
      catch (Exception e)
      {
        Logger.Error("Error getting info from MyVideo Database: " + e.Message);
      }

      return item;
    }

    public static void PlayMovie(string filename)
    {
      GUIVideoFiles.Reset(); // reset pincode

      IMDBMovie movie = new IMDBMovie();
      int id = VideoDatabase.GetMovieInfo(filename, ref movie);
      if (id < 0)
      {
        return;
      }

      ArrayList files = new ArrayList();
      VideoDatabase.GetFilesForMovie(movie.ID, ref files);

      if (files.Count > 1)
      {
        GUIVideoFiles.StackedMovieFiles = files;
        GUIVideoFiles.IsStacked = true;
      }
      else
      {
        GUIVideoFiles.IsStacked = false;
      }

      GUIVideoFiles.MovieDuration(files, false);
      GUIVideoFiles.PlayMovie(movie.ID, false);
    }

  }
}
