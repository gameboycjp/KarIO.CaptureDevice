# KarIO.CaptureDevice
Implements the [FlashCap](https://github.com/kekyo/FlashCap/tree/main) library as a procedural texture in [Resonite](https://resonite.com/).

Will remain unmaintained until the games engine updates to .NET 8! Unitys mono seems to cause weird issues, even for the base game sometimes.

Currently slow and extremely buggy, but functions. Most of the following issues are things I cannot test, or issues so odd that I have no idea why they could be happening. Capture Device names do not get shown correctly most of the time. DirectShow support does not work, so it uses Video For Windows, which requires desktop interaction. The camera never gets closed. Sometimes the component just plain fails to initialize. Only manages to present at about 5fps at the default resolution, 1080p, chosen for development purposes. Resolution needs to be changed on the component manually. Likely to break on a non-MJPEG stream, or an MJPEG stream that has Huffman Tables. Completely untested on Linux. There are also a few QoL todos, but this plugin is only a proof of concept at the moment, and will likely remain as such.

# Credits

## [Resonite](https://resonite.com/) by [YDMS](https://yellowdogman.com/)

## [FlashCap](https://github.com/kekyo/FlashCap/tree/main) by [Kouji Matsui](https://github.com/kekyo)

This library is used to grab data from capture devices.

## [libjpeg-turbo](https://libjpeg-turbo.org/) by Various Contributors

This plugin uses the standard huffman tables from [jstdhuff.c](https://github.com/libjpeg-turbo/libjpeg-turbo/blob/main/jstdhuff.c)
