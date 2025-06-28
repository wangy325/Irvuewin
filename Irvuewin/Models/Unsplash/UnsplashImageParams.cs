namespace Irvuewin.Models.Unsplash;


public class UnsplashImageParams(int width)
{
    /*
     * w, h: for adjusting the width and height of a photo
        crop: for applying cropping to the photo
        fm: for converting image format
        auto=format: for automatically choosing the optimal image format depending on user browser
        q: for changing the compression quality when using lossy file formats
        fit: for changing the fit of the image within the specified dimensions
        dpr: for adjusting the device pixel ratio of the image
     */

    public int Width { get; set; } = width;
    public int Height { get; set; }
    public int Quality { get; set; } = 85;
    public string? Crop { get; set; } = "entropy";
    public string? Fm { get; set; } = "jpg";
    public string? Fit { get; set; } = null;
    public int Dpr { get; set; } = 2;

    public UnsplashImageParams(int width, int dpr) : this(width)
    {
        Width = width;
        Dpr = dpr;
    }
}