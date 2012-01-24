using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTunesLib;
using System.Configuration;
using System.Net;
using System.IO;

namespace iTunesMatchTagger
{
    public class iTunesHelper
    {
        #region UpdateOptions
        public bool UpdateTrackName = true;
        public bool UpdateCollectionName = true;
        public bool UpdateArtistName = true;
        public bool UpdateTrackCount = true;
        public bool UpdateTrackNumber = true;
        public bool UpdatePrimaryGenreName = true;
        public bool UpdateDiscCount = true;
        public bool UpdateDiscNumber = true;
        /*
        public string collectionViewUrl;
        public string trackViewUrl;
        public string previewUrl;
        public string artworkUrl30;
        public string artworkUrl60;
        public string artworkUrl100;

        public DateTime releaseDate;
        public int trackTimeMillis;
        public string country;
            */
        #endregion

        public iTunesApp iTunes = new iTunesApp();
        private WebClient _web_client = new WebClient();
        public static List<string> AllCountries = Settings.countries.Split(' ').ToList<string>();
        private static string MatchedKindAsString = Settings.matchedKindAsString;
        public List<string> SelectedCountries = new List<string>();

        /// <summary>
        /// check if any update flag is set to true
        /// </summary>
        /// <returns></returns>
        public bool IsUpdateOptionSet()
        {
            return UpdateTrackName || UpdateCollectionName || UpdateArtistName ||
                   UpdateTrackCount || UpdateTrackNumber || UpdatePrimaryGenreName ||
                   UpdateDiscCount || UpdateDiscNumber;
        }

        /// <summary>
        /// Opens Track-file and find iTunes-TrackId
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public static long GetTrackId(IITTrack track)
        {
            try
            {
                string filename = ((dynamic)track).Location;

                //WriteLog("Get TrackId for " + filename, true, true);

                // only matched files
                if (!MatchedKindAsString.Contains(track.KindAsString)) return 0;

                byte[] byte_buffer = new byte[1024];
                FileStream f = new FileStream(filename, FileMode.Open);
                f.Read(byte_buffer, 0, 1024);
                f.Close();
                string file_string = ASCIIEncoding.ASCII.GetString(byte_buffer);
                int pos = file_string.IndexOf("song");
                string hex = DecToHex(byte_buffer[pos + 4]) + DecToHex(byte_buffer[pos + 5]) +
                             DecToHex(byte_buffer[pos + 6]) + DecToHex(byte_buffer[pos + 7]);
                long id = HexToDec(hex);

                //WriteLog("iTunes-TrackId found: " + id, true, true);

                return id;
            }
            catch(Exception ex)
            {
                //WriteLog("GetTrackId() failed: " + ex.Message, true, true);
                return 0;
            }
        }

        #region hex helper
        public static Int32 HexToDec(string hexValue)
        {
            return Int32.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
        }

        public static string DecToHex(int decValue)
        {
            return string.Format("{0:x}", decValue);
        }
        #endregion

        /// <summary>
        /// updates iTunes-Tags
        /// </summary>
        /// <param name="track"></param>
        /// <param name="tags"></param>
        public void UpdateTrack(IITTrack track, ID3Tags tags)
        {
            WriteLog("Update Track " + ((dynamic)track).Location, true, true);

            if (UpdateTrackName) track.Name = tags.trackName;
            if (UpdateCollectionName) track.Album = tags.collectionName;
            if (UpdateArtistName) track.Artist = tags.artistName;
            if (UpdateTrackCount) track.TrackCount = tags.trackCount;
            if (UpdateTrackNumber) track.TrackNumber = tags.trackNumber;
            if (UpdatePrimaryGenreName) track.Genre = tags.primaryGenreName;
            if (UpdateDiscCount) track.DiscCount = tags.discCount;
            if (UpdateDiscNumber) track.DiscNumber = tags.discNumber;
        }

        /// <summary>
        /// Search track on iTunes
        /// </summary>
        /// <param name="tracks"></param>
        /// <returns></returns>
        public List<Track> FindTags(IITTrackCollection tracks)
        {
            List<Track> result = new List<Track>();
            List<Track> track_list = new List<Track>();

            foreach (IITTrack track in tracks)
            {
                Track t = new Track(track);
                if (t.IsValid())
                {
                    WriteLog("TrackId: " + t.iTunesTrackId + " Filename: " + ((dynamic)track).Location, true, true);
                    track_list.Add(t);
                }
            }

            foreach(string country in SelectedCountries)
            {
                if (track_list.Count == 0) break;
                result.AddRange(FindTags(track_list, country));
            }

            return result;
        }

        /// <summary>
        /// Search track on iTunes
        /// </summary>
        /// <param name="tracks"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        public List<Track> FindTags(List<Track> tracks, string country)
        {
            WriteLog("Querying " + country + " store", true);

            List<Track> result = new List<Track>();

            foreach (Track track in tracks)
            {
                iTunesLookup(track.iTunesTrack, country);
                if (track.Id3Tag != null) continue;

                try
                {
                    // search on itunes
                    string json = _web_client.DownloadString("http://itunes.apple.com/lookup?id=" + track.iTunesTrackId + "&country=" + country);
                    var search_result = JsonHelper.Deserialise<iTunesResult>(json);
                    if (search_result.resultCount == 1)
                    {
                        track.Id3Tag = search_result.results[0];
                        track.StoreCountry = country;
                        result.Add(track);
                    }
                    else if (search_result.resultCount > 1)
                    {
                        throw new Exception(search_result.resultCount + " results found for " + ((dynamic)track).Location);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("FindTags() Exception: " + ex.Message, true, true);
                }
            }

            WriteLog(" - " + result.Count + " tracks found", false);

            return result;
        }

        #region logging
        public delegate void WriteLogEventHandler(string text, bool new_line = true, bool debug = false);

        public event WriteLogEventHandler OnWriteLog;

        protected void WriteLog(string text, bool new_line = true, bool debug = false)
        {
            if (OnWriteLog != null) OnWriteLog(text, new_line, debug);
        }
        #endregion

        #region lookup event
        public delegate void iTunesLookupEventHandler(IITTrack track, string country);

        public event iTunesLookupEventHandler OniTunesLookup;

        protected void iTunesLookup(IITTrack track, string country)
        {
            if (OniTunesLookup != null) OniTunesLookup(track, country);
        }
        #endregion
    }
}