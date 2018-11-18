namespace MovieProcessor
{
    public class MovieProcessorSettings
    {
        public string AccessToken { get; set; }
        public string BaseURL { get; set; }
        public string MovieListRelativeURL { get; set; }
        public string MovieListFallbackRelativeURL { get; set; }
        public string MovieDetailRelativeURL { get; set; }
        public string MovieDetailFallbackRelativeURL { get; set; }
    }
}
