using System;

namespace Avalon.Model
{
    public class Bookmark
    {
        public string ProfileId { get; set; }

        public string Name { get; set; }
        public AvatarModel Avatar { get; set; }
        public DateTime? LastMessagesSeen { get; set; }

        public bool Blocked { get; set; }

        public bool IsBookmarked { get; set; }
    }
}