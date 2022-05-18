﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Avalon.Model
{
    public class Feedback
    {
        [BsonId]
        internal ObjectId _id { get; set; }

        public string FeedbackId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DateSent { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? DateSeen { get; set; }

        public string FromProfileId { get; set; }

        public string FromName { get; set; }

        public string AdminProfileId { get; set; }

        public string AdminName { get; set; }

        [BsonRepresentation(BsonType.String)]
        public FeedbackType FeedbackType { get; set; }

        [StringLength(2000, ErrorMessage = "Message length cannot be more than 2000 characters long.")]
        public string Message { get; set; }

        public bool Open { get; set; }

        public string Countrycode { get; set; }

        public string Languagecode { get; set; }
    }
}