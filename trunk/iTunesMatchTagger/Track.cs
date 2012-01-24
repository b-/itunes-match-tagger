using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTunesLib;

namespace iTunesMatchTagger
{
    public class Track
    {
        public IITTrack iTunesTrack;
        public long iTunesTrackId;
        public ID3Tags Id3Tag;
        public string StoreCountry;

        public Track(IITTrack track)
        {
            iTunesTrack = track;
            iTunesTrackId = iTunesHelper.GetTrackId(track);
        }

        public bool IsValid()
        {
            return (iTunesTrackId > 0);
        }
    }
}
