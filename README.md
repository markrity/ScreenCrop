# Screen Crop
This tool allows you to easily crop and upload screen shots.

Download last stable version [here](http://www.megafileupload.com/ap5e/Install.rar)

![Screen Crop action gif](https://github.com/InviBear/ScreenCrop/blob/master/Readme/ScreenCropAction.gif)

## Features:

* Take a screenshot.
* Crop the screenshot before saving it (Click and drag Mouse 1).
* Move cropping sqaue around to capture exactly what you want (Click and drag Mouse 3).
* Automaticly save screenshot to a chosen location.
* Automaticly upload screenshot to imgur.com.
* Place the link of the uploaded image to your clipboard (CRTL+V).
* Customize cropping square to your liking.

![Image of ScreenCrop settings panel](https://github.com/InviBear/ScreenCrop/blob/master/Readme/Settings.png)


#### How to build:
1. Download project.
2. Freeze PyCropper\ScreenCropper.pyw (seems like pyinstaller works best in my case).
3. Make sure that ScreenCropper.exe (the previusly pyw file) and all other content that pyinstaller created is located at the build folder of ScreenCropGui.exe
4. Make sure that ScreenCropGui calls and runs ScreenCropper.exe from the right path
(ScreenCropGui\ScreenCrop\Program.cs\SysTrayApp -> void gkh_KeyDown() function)
5. Build and run ScreenCropGui.

#### TODO:

- [X] Add visualization to settings menu.
- [ ] Log captured screenshots, save locations and links.
- [ ] Investigate random crashes.
- [ ] Document gui counterpart.
- [ ] Figure out make cropper part load faster.
