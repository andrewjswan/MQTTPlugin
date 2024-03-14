using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MQTTPlugin
{
  public static class LatestsMediaHandlerHelper
  {
    public static List<string> LatestsMedia()
    {
      List<string> _latests = new List<string>();
      if (Utils.IsAssemblyAvailable("LatestMediaHandler", new Version(2, 4, 0, 0)))
      {
        if (MQTTPlugin.DebugMode) Logger.Debug("LatestMediaHandler found, searching for Latests media");
        try
        {
          foreach (LatestsCategory category in (LatestsCategory[])Enum.GetValues(typeof(LatestsCategory)))
          {
            List<LatestMediaHandler.MQTTItem> latest = LatestMediaHandler.ExternalAccess.GetMQTTLatests(category.ToString());

            if (latest != null && latest.Count > 0)
            {
              HALatests header = new HALatests();
              header.title_default = "$title";
              header.line1_default = "$episode";
              header.line2_default = "$release";
              header.line3_default = "$rating - $runtime";
              header.line4_default = "$number - $studio";
              header.icon = "none";

              switch (category)
              {
                case LatestsCategory.Music:
                case LatestsCategory.MvCentral:
                  break;
                case LatestsCategory.Movies:
                case LatestsCategory.MovingPictures:
                case LatestsCategory.MyFilms:
                  header.line2_default = "$genres";
                  latest.GetArtwork("movie");
                  break;
                case LatestsCategory.TVSeries:
                  header.line2_default = "$genres";
                  latest.GetArtwork("tv");
                  break;
                case LatestsCategory.Pictures:
                case LatestsCategory.TV:
                  break;
              }

              string headerline = JsonConvert.SerializeObject(header, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new Utils.LowercaseContractResolver() });
              string items = JsonConvert.SerializeObject(latest, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new Utils.LowercaseContractResolver() });
              string line = "{\"count\": " + latest.Count.ToString() + ", \"data\": " + "[" + headerline + ", " + items.Substring(1) + "}";

              _latests.Add(category.ToString() + ":" + line);

              if (MQTTPlugin.DebugMode) Logger.Debug("LatestMediaHandler: " + category.ToString() + ": " + latest.Count.ToString());
            }
          }
        }
        catch (Exception e)
        {
          Logger.Error("Error getting info from LatestMediaHandler: " + e.Message);
        }
      }
      return _latests;
    }

    internal static List<LatestMediaHandler.MQTTItem> GetArtwork(this List<LatestMediaHandler.MQTTItem> self, string mode)
    {
      List<LatestMediaHandler.MQTTItem> items = new List<LatestMediaHandler.MQTTItem>();

      foreach (LatestMediaHandler.MQTTItem item in self)
      {
        items.Add(item.GetArtwork(mode));
      }

      return items;
    }

    internal static LatestMediaHandler.MQTTItem GetArtwork(this LatestMediaHandler.MQTTItem self, string mode)
    {
      string id = mode == "tv" ? GetTVID(self.Id) : self.Id;

      if (!string.IsNullOrEmpty(id))
      {
        if (Utils.ImageCache == null)
        {
          Utils.ImageCache = new Hashtable();
        }
        string key = "#" + mode + "#" + id;

        string json = Utils.ImageCache.Contains(key) ? (string)Utils.ImageCache[key] : Utils.DownloadJson(ApiURLTheMovieDB + mode + "/" + id + "/images?api_key=" + ApiKeyTheMovieDB);
        if (!string.IsNullOrEmpty(json))
        {
          try
          {
            JsonRoot images = JsonConvert.DeserializeObject<JsonRoot>(json);

            // Poster
            var poster = images.posters.Where(l => l.iso_639_1 == Utils.Language).OrderByDescending(x => x.vote_average).ThenByDescending(y => y.width).FirstOrDefault();
            string image = poster == null ? string.Empty : poster.file_path;
            if (!string.IsNullOrEmpty(image))
            {
              self.Poster = ApiImageTheMovieDB + image;
            }
            else if (Utils.Language != "en")
            {
              poster = images.posters.Where(l => l.iso_639_1 == "en").OrderByDescending(x => x.vote_average).ThenByDescending(y => y.width).FirstOrDefault();
              image = poster == null ? string.Empty : poster.file_path;
              if (!string.IsNullOrEmpty(image))
              {
                self.Poster = ApiImageTheMovieDB + image;
              }
            }

            // Background
            var background = images.backdrops.OrderByDescending(x => x.vote_average).ThenByDescending(y => y.width).FirstOrDefault();
            image = background == null ? string.Empty : background.file_path;
            if (!string.IsNullOrEmpty(image))
            {
              self.Fanart = ApiImageTheMovieDB + image;
            }

            // Cache
            if (!Utils.ImageCache.Contains(key))
            {
              Utils.ImageCache.Add(key, json);
            }
          }
          catch
          { }
        }

        // Poster
        if (!self.Poster.StartsWith("http"))
        {
          self.Poster = Utils.ImageToDataImage(self.Poster, 133, 200);
        }

        // Background
        if (!self.Fanart.StartsWith("http"))
        {
          self.Fanart = Utils.ImageToDataImage(self.Fanart, 444, 250);
        }
      }

      return self;
    }

    internal static string GetTVID(string id)
    {
      if (!string.IsNullOrEmpty(id))
      {
        if (Utils.TVIDCache == null)
        {
          Utils.TVIDCache = new Hashtable();
        }
        string key = "#" + id;
        if (Utils.TVIDCache.Contains(key))
        {
          return (string)Utils.TVIDCache[key];
        }

        string json = Utils.DownloadJson(ApiURLTheMovieDB + "find/" + id + "?external_source=tvdb_id&api_key=" + ApiKeyTheMovieDB);
        Regex rxtvid = new Regex(@"tv_results.:[^}]+?id.:(?<tvid>\d+)");
        Match matchtvid = rxtvid.Match(json);
        if (matchtvid.Success)
        {
          string value = matchtvid.Groups["tvid"].Value;
          Utils.TVIDCache.Add(key, value);
          return value;
        }
      }
      return id;
    }

    public enum LatestsCategory
    {
      Music,
      MvCentral,
      Movies,
      MovingPictures,
      TVSeries,
      MyFilms,
      Pictures,
      TV,
    }

    internal class HALatests
    {
      public string title_default { get; set; }
      public string line1_default { get; set; }
      public string line2_default { get; set; }
      public string line3_default { get; set; }
      public string line4_default { get; set; }
      public string icon { get; set; }
    }

    internal class Backdrop
    {
      public double aspect_ratio { get; set; }
      public string file_path { get; set; }
      public int height { get; set; }
      public string iso_639_1 { get; set; }
      public double vote_average { get; set; }
      public int vote_count { get; set; }
      public int width { get; set; }
    }

    internal class Poster
    {
      public double aspect_ratio { get; set; }
      public string file_path { get; set; }
      public int height { get; set; }
      public string iso_639_1 { get; set; }
      public double vote_average { get; set; }
      public int vote_count { get; set; }
      public int width { get; set; }
    }

    internal class JsonRoot
    {
      public int id { get; set; }
      public List<Backdrop> backdrops { get; set; }
      public List<Poster> posters { get; set; }
    }

    private static string ApiURLTheMovieDB = "https://api.tmdb.org/3/";
    private static string ApiImageTheMovieDB = "https://image.tmdb.org/t/p/w500";
    private static string ApiKeyTheMovieDB = "e224fe4f3fec5f7b5570641f7cd3df3a";
  }
}
