using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace iTunesMatchTagger
{
    [DataContractAttribute]
    public class iTunesResult
    {
        [DataMemberAttribute]
        public int resultCount;
        [DataMemberAttribute]
        public ID3Tags[] results;
    }

    public class ID3Tags
    {
        public string trackName;
        public string collectionName;
        public string artistName;
        public int trackCount;
        public int trackNumber;
        public string primaryGenreName;
        public int discCount;
        public int discNumber;

        public string collectionViewUrl;
        public string trackViewUrl;
        public string previewUrl;
        public string artworkUrl30;
        public string artworkUrl60;
        public string artworkUrl100;

        public string releaseDate;
        public int trackTimeMillis;
        public string country;
    }
}