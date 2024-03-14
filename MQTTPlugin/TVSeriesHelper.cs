using System;
using System.Collections.Generic;

using WindowPlugins.GUITVSeries;

namespace MQTTPlugin
{
  public static class TVSeriesHelper
  {
    public static LatestMediaHandler.MQTTItem CheckDB(string SearchFile)
    {
      LatestMediaHandler.MQTTItem item = new LatestMediaHandler.MQTTItem();
      if (MQTTPlugin.DebugMode) Logger.Debug("Check to see if video is in MP-TVSeries database.");

      if (Utils.IsAssemblyAvailable("MP-TVSeries", new Version(2, 6, 3, 1242)))
      {
        if (MQTTPlugin.DebugMode) Logger.Debug("MP-TVSeries found, searching Database for: " + SearchFile);
        try
        {
          SQLCondition query = new SQLCondition(new DBEpisode(), DBEpisode.cFilename, SearchFile, SQLConditionType.Equal);
          List<DBEpisode> episodes = DBEpisode.Get(query);
          if (MQTTPlugin.DebugMode) Logger.Debug("Found: " + episodes.Count.ToString() + " episodes.");
          if (episodes.Count > 0)
          {
            DBSeries s = Helper.getCorrespondingSeries(episodes[0].onlineEpisode[DBOnlineEpisode.cSeriesID]);
            if (MQTTPlugin.DebugMode) Logger.Debug("Video is in MP-TVSeries database.");

            item.Id = episodes[0][DBEpisode.cSeriesID];
            item.Title = s.ToString() + " - " + episodes[0][DBEpisode.cEpisodeName];
            item.Filename = SearchFile;
            item.Genres = s[DBOnlineSeries.cGenre];
            item.Episode = "S" + episodes[0][DBEpisode.cSeasonIndex] + "E" + episodes[0][DBEpisode.cEpisodeIndex];
            item.Poster= ImageAllocator.GetSeriesPosterAsFilename(s);
            item.Fanart = Fanart.getFanart(episodes[0][DBEpisode.cSeriesID]).FanartFilename;
            item.GetArtwork("tv");
          }
        }
        catch (Exception e)
        {
          Logger.Error("Error getting info from TVSeries Database: " + e.Message);
        }
      }
      return item;
    }
  }
}
