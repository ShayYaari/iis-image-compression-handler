# Image Compression HTTP Handler for IIS

This repository contains an implementation of an HTTP Handler for Internet Information Services (IIS) that supports image compression using [ImageMagick](http://imagemagick.org/). With this handler, you can dynamically compress images served by your web server, reducing their quality to optimize bandwidth usage and improve load times.

## Features

- **Dynamic Compression**: Compress images on-the-fly as they are served by the web server.
- **Customizable Quality**: Adjust the quality of compression to suit your requirements.
- **Supports Common Image Formats**: Works with popular image formats such as JPEG, PNG, GIF, etc.
- **Easy Integration**: Simple setup and integration with your existing IIS server.

## Prerequisites

Before using this handler, ensure you have the following:

- [ImageMagick](http://imagemagick.org/) installed on your server.
- Internet Information Services (IIS) configured and running.

## Installation

1. Download the release DLL from the "Releases" section of this repository.
2. Put the downloaded DLL (IIImageCompressionHandler.dll) under the `bin` folder in the directory of your IIS-hosted application.

## Configuration

To configure the Image Compression HTTP Handler for your IIS server:

1. Open Internet Information Services (IIS) Manager.
2. Select your website or application.
3. Double-click on "Handler Mappings".
4. Click "Add Managed Handler...".
5. Enter the following details:
   - Request Path: `*.jpg` (or your desired image extension)
   - Type: `IISImageCompressionHandler.IISImageCompressionHandler`
   - Name: `IISImageCompressionHandler`
6. Click "OK" to save the configuration.

## Usage

Once installed and configured, the Image Compression HTTP Handler will automatically compress images with the specified file extension as they are requested by clients.

To adjust the compression quality for a specific request, you can include an HTTP header named "Image-Quality" with the desired percentage of quality. For example:

```
GET /example.jpg HTTP/1.1
Host: yourwebsite.com
Image-Quality: 50
```

This will instruct the handler to compress the image with 50% quality. Adjust the quality percentage as needed to find the optimal balance between image size and visual fidelity.


## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue if you encounter any problems or have suggestions for improvements.

## Acknowledgments

Special thanks to the developers of ImageMagick for providing a powerful and versatile image processing library.
