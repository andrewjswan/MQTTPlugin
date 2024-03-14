using MediaPortal.Music.Database;
using MediaPortal.Profile;
using MediaPortal.TagReader;

using System;
using System.Collections.Generic;

namespace MQTTPlugin
{
  public static class MyMusicHelper
  {
    private static MediaPortal.Playlists.PlayListPlayer playlistPlayer;
    private static bool _stripArtistPrefixes;

    public static LatestMediaHandler.MQTTItem CheckDB(string SearchFile)
    {
      LatestMediaHandler.MQTTItem item = new LatestMediaHandler.MQTTItem();

      try
      { 
        Song song = new Song();
        MusicDatabase musicDatabase = MusicDatabase.Instance;
        musicDatabase.GetSongByFileName(SearchFile, ref song);
        if (song != null)
        {
          if (MQTTPlugin.DebugMode) Logger.Debug("Music is in MyMusic database.");

          item.Id = song.Id.ToString();
          item.Title = song.Artist + " - " + song.Album + " - " + song.Title;
          item.Filename = SearchFile;
          item.Genres = song.Genre;
          // item.GetArtwork("music");
        }
      }
      catch (Exception e)
      {
        Logger.Error("Error getting info from MyMusic Database: " + e.Message);
      }

      return item;
    }

    public static void PlayMusic(string filename)
    {
      if (!string.IsNullOrEmpty(filename))
      {
        LoadSongsFromList(filename);
        StartPlayback();
      }
    }

    public static void PlayMusicAlbum(string albumname)
    {
      List<Song> songs = new List<Song>();
      MusicDatabase musicDatabase = MusicDatabase.Instance;
      musicDatabase.GetSongsByAlbumArtist(albumname, ref songs);

      if (songs.Count == 0)
      {
        return;
      }

      string SongsFiles = string.Empty;
      for (int i = 0; i < songs.Count; ++i)
      {
        if (!string.IsNullOrEmpty(songs[i].FileName))
        {
          SongsFiles = songs[i].FileName + "|";
        }
      }

      if (!string.IsNullOrEmpty(SongsFiles))
      {
        LoadSongsFromList(SongsFiles);
        StartPlayback();
      }
    }

    public static void PlayMusicArtistAlbum(string artist, string albumname)
    {
      List<Song> songs = new List<Song>();
      MusicDatabase musicDatabase = MusicDatabase.Instance;
      musicDatabase.GetSongsByAlbumArtistAlbum(artist, albumname, ref songs);

      if (songs.Count == 0)
      {
        return;
      }

      string SongsFiles = string.Empty;
      for (int i = 0; i < songs.Count; ++i)
      {
        if (!string.IsNullOrEmpty(songs[i].FileName))
        {
          SongsFiles = songs[i].FileName + "|";
        }
      }

      if (!string.IsNullOrEmpty(SongsFiles))
      {
        LoadSongsFromList(SongsFiles);
        StartPlayback();
      }
    }
    
    private static void LoadSongsFromList(string SongsFiles)
    {
      using (Settings xmlreader = new MPSettings())
      {
        _stripArtistPrefixes = xmlreader.GetValueAsBool("musicfiles", "stripartistprefixes", false);
      }

      // clear current playlist
      playlistPlayer = MediaPortal.Playlists.PlayListPlayer.SingletonPlayer;
      playlistPlayer.GetPlaylist(MediaPortal.Playlists.PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
      int numSongs = 0;
      try
      {
        string[] sSongsFiles = SongsFiles.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        foreach (string sSongsFile in sSongsFiles)
        {
          MediaPortal.Playlists.PlayListItem item = new MediaPortal.Playlists.PlayListItem();
          item.FileName = sSongsFile;
          item.Type = MediaPortal.Playlists.PlayListItem.PlayListItemType.Audio;
          MusicTag tag = TagReader.ReadTag(sSongsFile);
          if (tag != null)
          {
            tag.Artist = MediaPortal.Util.Utils.FormatMultiItemMusicStringTrim(tag.Artist, _stripArtistPrefixes);
            tag.AlbumArtist = MediaPortal.Util.Utils.FormatMultiItemMusicStringTrim(tag.AlbumArtist, _stripArtistPrefixes);
            tag.Genre = MediaPortal.Util.Utils.FormatMultiItemMusicStringTrim(tag.Genre, false);
            tag.Composer = MediaPortal.Util.Utils.FormatMultiItemMusicStringTrim(tag.Composer, _stripArtistPrefixes);

            item.Description = tag.Title;
            item.MusicTag = tag;
            item.Duration = tag.Duration;
          }
          playlistPlayer.GetPlaylist(MediaPortal.Playlists.PlayListType.PLAYLIST_MUSIC_TEMP).Add(item);
          numSongs++;
        }
        if (MQTTPlugin.DebugMode) Logger.Debug("LoadSongsFromList: Complete: " + numSongs);
      }
      catch (Exception ex)
      {
        Logger.Error("LoadSongsFromList: " + ex.ToString());
      }
    }

    private static void StartPlayback()
    {
      // if we got a playlist start playing it
      int Count = playlistPlayer.GetPlaylist(MediaPortal.Playlists.PlayListType.PLAYLIST_MUSIC_TEMP).Count;
      if (Count > 0)
      {
        if (MQTTPlugin.DebugMode) Logger.Debug("StartPlayback: " + Count);
        playlistPlayer.CurrentPlaylistType = MediaPortal.Playlists.PlayListType.PLAYLIST_MUSIC_TEMP;
        playlistPlayer.Reset();
        playlistPlayer.Play(0);
      }
    }

  }
}
