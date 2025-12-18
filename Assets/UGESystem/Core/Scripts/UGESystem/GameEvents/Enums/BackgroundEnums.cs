namespace UGESystem
{
    /// <summary>
    /// Defines actions that can be performed on a background.
    /// Used in <see cref="BackgroundCommand"/>.
    /// </summary>
    public enum BackgroundAction
    {
        /// <summary>Display the background.</summary>
        Show,
        /// <summary>Hide the background.</summary>
        Hide
    }

    /// <summary>
    /// Defines the type of background asset.
    /// Used in <see cref="BackgroundCommand"/>.
    /// </summary>
    public enum BackgroundType
    {
        /// <summary>The background is an image.</summary>
        Image,
        /// <summary>The background is a video.</summary>
        Video
    }
}