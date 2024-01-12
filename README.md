# KarIO.CaptureDevice
Implements the [FlashCap](https://github.com/kekyo/FlashCap/tree/main) library as a procedural texture in [Resonite](https://resonite.com/).

Currently slow and extremely buggy, but functions. For instance, Capture Device names do not get shown correctly most of the time. DirectShow support does not work, so it uses Video For Windows, which requires desktop interaction. Sometimes the component just plain fails to initialize. Only runs at about 5fps, and defaults to 1080p for testing purposes. Likely to break on a non-MJPEG stream. Completely untested on Linux. There are also a few QOL todos, but this plugin is only a proof of concept at the moment, and will likely remain as such.

# Credits

## [Resonite](https://resonite.com/) by [YDMS](https://yellowdogman.com/)

## [FlashCap](https://github.com/kekyo/FlashCap/tree/main) by [Kouji Matsui](https://github.com/kekyo)

This library is used to grab data from capture devices.

## [libjpeg-turbo](https://libjpeg-turbo.org/) by Various Contributors

This plugin uses the standard huffman tables from [jstdhuff.c](https://github.com/libjpeg-turbo/libjpeg-turbo/blob/main/jstdhuff.c)
