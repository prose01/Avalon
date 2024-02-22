using System.Collections.Generic;
using System;

namespace Avalon.Model
{
    public class GroupMember
    {
        public string ProfileId { get; set; }

        public string Name { get; set; }

        public AvatarModel Avatar { get; set; }

        public bool Blocked { get; set; }

        public Dictionary<string, DateTime> Complains { internal get; set; }
    }
}
