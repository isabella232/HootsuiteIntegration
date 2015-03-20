using System;
using System.Collections.Generic;

namespace ININ.Alliances.HootsuiteClientAddin.model
{
    public class HootsuiteData
    {
        public string Version { get; set; }
        public HootsuitePost Post { get; set; }

        public HootsuiteData()
        {
            Post = new HootsuitePost();
        }
    }

    public class HootsuitePost
    {
        public string Href { get; set; }
        public string Id { get; set; }
        public DateTime Datetime { get; set; }
        public string Source { get; set; }
        public string Network { get; set; }
        public HootsuitePostContent Content { get; set; }
        public HootsuitePostUser User { get; set; }
        public List<HootsuitePostConversation> Conversation { get; set; }
        public List<HootsuitePostAttachment> Attachments { get; set; }

        public HootsuitePost()
        {
            Datetime = DateTime.MinValue;
            Content = new HootsuitePostContent();
            User = new HootsuitePostUser();
            Conversation = new List<HootsuitePostConversation>();
            Attachments = new List<HootsuitePostAttachment>();
        }
    }

    public class HootsuitePostContent
    {
        public string Body { get; set; }
        public string BodyHtml { get; set; }

        public HootsuitePostContent()
        {
            // An error message of sorts
            Body = "Blank JSON data!!!";
            BodyHtml = "Blank JSON data!!!";
        }
    }

    public class HootsuitePostUser
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
    }

    public class HootsuitePostConversation
    {
        public string UserId { get; set; }
        public DateTime Datetime { get; set; }
        public string Likes { get; set; }
        public string Name { get; set; }
        public string ProfileUrl { get; set; }
        public string Uid { get; set; }
        public string Text { get; set; }

        public HootsuitePostConversation()
        {
            Datetime = DateTime.MinValue;
        }
    }

    public class HootsuitePostAttachment
    {
        // No idea what goes in here because Hootsuite doesn't populate this
    }
}

// IDEA!!! Json document to C# converter
