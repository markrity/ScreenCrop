# Screen Crop
This tool allows you to easily crop and upload screen shots.

Download last stable version [here](http://s000.tinyupload.com/index.php?file_id=07971557725903747915)


![Screen Crop action gif](https://github.com/InviBear/ScreenCrop/blob/master/Readme/ScreenCropAction.gif)

## Features:

* Take a screenshot.
* Crop the screenshot before saving it (Click and drag Mouse 1 and press Enter to save it).
* Move cropping sqaue around to capture exactly what you want (Click and drag Mouse 3).
* Automaticly save the screenshot to a chosen location.
* Automaticly upload the screenshot to imgur.com.
* Place the link of the uploaded image to your clipboard (CRTL+V).
* Customize cropping square to your liking.
* View saved screenshots.
* Set titles and remove screen shots from "recents" history.

![Image of ScreenCrop settings panel](https://github.com/InviBear/ScreenCrop/blob/master/Readme/Settings.png)

![Image of ScreenCrop recents panel](https://github.com/InviBear/ScreenCrop/blob/master/Readme/Recents.png)

#### How to build:
1. Download project.
2. Freeze PyCropper\ScreenCropper.pyw (seems like pyinstaller works best in my case).
3. Make sure that ScreenCropper.exe (the previusly pyw file) and all other content that pyinstaller created is located at the build folder of ScreenCropGui.exe
4. Make sure that ScreenCropGui calls and runs ScreenCropper.exe from the right path
(ScreenCropGui\ScreenCrop\Program.cs\SysTrayApp -> void gkh_KeyDown() function)
5. Build and run ScreenCropGui.

#### TODO:

- [X] Add visualization to settings menu.
- [X] Log captured screenshots, save locations and links.
- [ ] ~~Investigate random crashes.~~ (GlobalKeyHook sometimes crashes, dont know why yet)
- [ ] Document gui counterpart.
- [ ] Figure out how to make cropper part load faster.
