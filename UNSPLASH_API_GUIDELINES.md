This app uses [Unsplash](https://unsplash.com) as its wallpaper provider.
All API operations adhere to the Unsplash API [Technical and Usage Guidelines](https://help.unsplash.com/en/articles/2511245-unsplash-api-guidelines).

The implementation of development and usage guidelines is demonstrated below:

## Technical Implementation

1. Use `photo.urls` to preview wallpapers. You can find this in [Channels.xaml](./Irvuewin/Views/Channels.xaml) at
lines 69-82 and 190-219. The source code is as follows:

    ```xaml
    <Image Grid.Row="0"
       Margin="10, 10, 20, 0"
       Width="200"
       Source="{Binding Urls.Small, Converter={StaticResource ImageUrlToBitmapConverter}, ConverterParameter=200}">
    <Image.Style>
        <Style TargetType="Image">
            <!---->
        </Style>
    </Image.Style>
    </Image>
    ```

2. Triggering the download callback when setting or downloading wallpaper. Using an event publisher, the download callback event
is published in [WallpaperUtil](./Irvuewin/Helpers/Utils/WallpaperUtil.cs), and the system sends a request to 
`photo.links.download_location`. Here is the code snippet:

    ```csharp
    //...
    await using var imageStream = await httpClient.GetStreamAsync(uriString);
    await using var fileStream = File.Create(localImagePath);
    await imageStream.CopyToAsync(fileStream);
    path = localImagePath;
    // unsplash api callback
    EventBus.PublishTriggerWallpaperDownLoad(photo.Links.DownloadLocation);
    ```
   After the event is published, [HttpClient](./Irvuewin/Helpers/HTTP/HttpService.cs) will send the request asynchronously.
   ```csharp
   // Trigger Download
    private async void DownloadCallback(Uri rawPath)
    {
        // https://api.unsplash.com/photos/_0MA2Kzgb5s/download?ixid=xx
        // Logger.Information(@"Download callback path {0}, query {1} ", rawPath.AbsolutePath, rawPath.Query);
        await GetAsync<object>(rawPath.AbsolutePath, rawPath.Query, true);
    }

    private void OnWallpaperDownloadCallback(Uri rawUrl)
    {
        Logger.Information("Handle wallpaper download callback.");
        DownloadCallback(rawUrl);
    }
   ```

3. Wallpapers and photographers are attributed in the app's related submenus.

    ```csharp
    private static void OnViewPhoto(UnsplashPhoto photo)
    {
        var attrUrl = string.Concat(photo.Links.Html.ToString(), Attribution);
        Logger.Information("View wallpaper on unsplash: {0}", attrUrl);
        ICommonCommands.OpenUrl(attrUrl);
    }

    private static void OnViewAuthor(UnsplashPhoto photo)
    {
        var attrUrl = string.Concat(photo.User.Links.Html.ToString(), Attribution);
        Logger.Information("View author: {0}", attrUrl);
        ICommonCommands.OpenUrl(attrUrl);
    }
    ```

4. A Cloudflare proxy is configured to keep the `AccessKey` confidential.

    ```csharp
    const string BaseApiUrl = "https://unsplash-api-proxy.wangy325.workers.dev";
    const string OriginImageUrl = "https://images.unsplash.com";
    const string ImageProxyUrl = "https://unsplash-image-proxy.wangy325.workers.dev";
    ```
