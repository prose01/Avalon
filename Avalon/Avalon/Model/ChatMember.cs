﻿using MongoDB.Bson;

namespace Avalon.Model
{
    public class ChatMember
    {
        public string ProfileId { get; set; }

        public bool Blocked { get; set; }
    }
}
