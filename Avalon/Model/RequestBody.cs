namespace Avalon.Model
{
    public class RequestBody
    {
        public ProfileFilter ProfileFilter { get; set; }
        public FeedbackFilter FeedbackFilter { get; set; }
        public string[] ProfileIds { get; set; }
    }
}
