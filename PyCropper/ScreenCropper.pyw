# Python version conformation
import sys
# Path and file naming
import os
# Module settings
import json
# String validation
import re
# GUI
try:
    from tkinter import *
except ImportError:
    from Tkinter import *
# Image scaling
from PIL import Image, ImageTk
# Screen capture
from PIL import ImageGrab
# File naming
from datetime import datetime
# Imgur.com api
import pyimgur

major, minor, micro, releaselevel, serial = sys.version_info
if (major, minor) < (2, 7):
    print("This module was not tested for any python version under 2.7.")
    sys.exit(2)


__author__ = "Michael Livs"
__version__ = "1.0"
__email__ = "livsMichael@gmail.com"

DEBUG = TRUE


class ScreenCrop(Frame):
    """
    ScreenCrop is a python module that delivers an easy and intuative way
    to capture, crop and upload screen shots.
    """

    #                               CLASS INITIALTION
    # ===========================================

    def __init__(self, parent, path, save_path, continuous_mode, imgur_upload,
                 rec_color, rec_width, image_format):
        Frame.__init__(self, parent)
        self.parent = parent

        # Initial variables
        self.trace = 0
        self.drawn = None
        self.uploaded_images = dict()

        # Init settings
        self.save_path = save_path
        self.continuous_mode = continuous_mode
        self.imgur_upload = imgur_upload
        self.rec_color = rec_color
        self.rec_width = rec_width
        self.image_format = image_format

        # Client id from imgur api
        self.CLIENT_ID = "be8b1a456528379"

        # Minimal frame sizes
        self.frame_width = self.parent.winfo_screenwidth() / 2
        self.frame_height = self.parent.winfo_screenheight() / 2

        # Set minimal size
        self.parent.minsize(int(self.frame_width), int(self.frame_height))

        # Initiate main frame
        self.main_frame = Frame(self)
        self.main_frame.pack(side="top", fill="both", expand=True)

        # initiate canvas
        self.display = Canvas(self.main_frame, highlightthickness=0)
        self.display.pack(side="top", fill="both", expand=True)

        self.original = Image.open(path)
        self.image = ImageTk.PhotoImage(self.original)

        self.display.create_image(0, 0, image=self.image, anchor="nw",
                                  tags="IMG")

        # Event bindings
        self.parent.bind("<Escape>", self.quit)
        self.parent.bind('<Return>', self.save)
        self.display.bind("<ButtonPress-1>", self.onStart)
        self.display.bind("<ButtonRelease-1>", self.onEnd)
        self.display.bind("<B1-Motion>", self.onGrow)
        self.display.bind("<B3-Motion>", self.onMove)
        self.display.bind('<Double-1>', self.onClear)

        self.pack(fill="both", expand=True)

    #                               CLASS METHODS
    # ===========================================

    #                       mouse and keyboard events
    # -------------------------------------------------------------------------

    # Mouse button 1 press event
    def onStart(self, event):
        self.shape = self.display.create_rectangle
        self.start = event
        self.drawn = None
        event.widget.delete('DRAWN')

    # Mouse button 1 release event
    def onEnd(self, event):
        self.end = event

    # Mouse button 1 motion event
    def onGrow(self, event):
        self.display = event.widget
        if self.drawn:
            self.display.delete(self.drawn)
        objectId = self.shape(self.start.x, self.start.y, event.x, event.y,
                              outline=self.rec_color, width=self.rec_width,
                              tags="DRAWN")
        self.drawn = objectId

    # Mouse button 1 double click event
    def onClear(self, event):
        event.widget.delete('DRAWN')

    # Mouse button 3 move event
    def onMove(self, event):
        if self.drawn:
            self.display = event.widget
            diffX, diffY = (event.x - self.start.x), (event.y - self.start.y)
            self.display.move(self.drawn, diffX, diffY)
            self.start = event

    # Escape key pressed event
    def quit(self, event):
        self.parent.quit()

    # Enter key pressed event
    def save(self, event):
        image_name = datetime.now().strftime(
            '%Y-%m-%d_%H-%M-%S-%f') + self.image_format
        image_path = self.save_path + '\\' + image_name

        if self.drawn:

            left = 0
            top = 0
            width = 0
            height = 0

            # Take care of the cases that the crop box is out of the screen
            # width and height.
            if self.start.x < self.end.x:
                left = self.start.x
                width = self.end.x - self.start.x
            else:
                left = self.end.x
                width = self.start.x - self.end.x

            if self.start.y < self.end.y:
                top = self.start.y
                height = self.end.y - self.start.y
            else:
                top = self.end.y
                height = self.start.y - self.end.y

            box = (left, top, left + width, top + height)

            # Crop the image
            cropped = self.original.crop(box)

            # Save the cropped image and name it with the current date and
            # time
            cropped.save(image_path)

        else:
            # If user did not select an area to crop, save full-screen
            # screenshot
            self.original.save(image_path)

        if (not self.continuous_mode and self.imgur_upload):
            # TODO: find out how to upload albums to imgur
            # and enable uploading in continous mode

            # Hide window
            self.parent.withdraw()

            # Connect to imgur using CLIENT_ID
            im = pyimgur.Imgur(self.CLIENT_ID)

            # Upload image image to imgur
            image_upload = im.upload_image(image_path, title=image_name)

            # Save link in a dictionary
            self.uploaded_images["name"] = image_name
            self.uploaded_images["link"] = image_upload.link

            # Save link in local json for future use
            self.save_links()
            # Exit module
            self.quit(event)

        elif(not self.continuous_mode and not self.imgur_upload):
            self.parent.withdraw()
            self.quit()

    def save_links(self):
        with open('links.json', 'w') as links_json:
            json.dump(self.uploaded_images, links_json)

if __name__ == '__main__':

    """ Main entry point to module """
    # Path to temporary directory that will be created in the location
    # of the module
    tempPath = os.path.dirname(
        os.path.realpath('__file__')) + '\\temporary'

    # Path to the settings file
    settingsJson = os.path.dirname(
        os.path.realpath('__file__')) + '\\settings.json'

    # Path to where screen shots directory will be saved
    defualtPath = os.path.dirname(
        os.path.realpath('__file__')) + '\\Screen Shots'

    # Path to temporary screen shot
    tempCap = tempPath + '\\screencap.png'

    # try opening settings file
    try:
        with open(settingsJson, 'r+') as file:
            settings = json.load(file)
    except (IOError, ValueError) as e:
        # Give default alternative if settings file is not found
        if DEBUG:
            print(str(e))
        settings = dict()
        settings['save_location'] = defualtPath
        settings['continuous_mode'] = False
        settings['imgur_upload'] = True
        settings['rec_color'] = "#ff3535"
        settings['rec_width'] = 1.4
        settings['image_format'] = '.png'

    if DEBUG:
        print(settings)

    # If doesn't exist, create temporary directory.
    if not os.path.exists(tempPath):
        if DEBUG:
            print(tempPath + " doesn't exist, creatings directoy")
        os.makedirs(tempPath)

    # Check If the path is a valid absolute path adress.
    if (settings['save_location'] is None or
            not os.path.isabs(settings['save_location'])):
        if DEBUG:
            print("save_location is not a valid absolute path, " +
                  "using defualt path: " +
                  defualtPath)
        settings['save_location'] = defualtPath

    # If save location doesn't exist, create it.
    if not os.path.exists(settings['save_location']):
        if DEBUG:
            print("save_location doesn't exist, creating directory")
        os.makedirs(settings['save_location'])

    # Make sure that user input is valid.
    if (not isinstance(settings['rec_width'], int) and
            not isinstance(settings['rec_width'], float)):
        if DEBUG:
            print("rec_width is " +
                  str(type(settings['rec_width'])) +
                  ", using defualt value (1.4)")
        settings['rec_width'] = 1.4

    if (not isinstance(settings['continuous_mode'], bool)):
        if DEBUG:
            print("continuous_mode is " +
                  InstanceType(settings['continuous_mode']) +
                  ", using defualt value (False)")
        settings['continuous_mode'] = False

    if (not isinstance(settings['imgur_upload'], bool)):
        if DEBUG:
            print("imgur_upload is " +
                  InstanceType(settings['imgur_upload']) +
                  ", using defualt value (True)")
        settings['imgur_upload'] = True

    # Regular expresion to check if HEX is valid
    match = re.search(r'^#(?:[0-9a-fA-F]{3}){1,2}$', settings['rec_color'])
    if not match:
        if DEBUG:
            print("rec_color is not a valid HEX type, " +
                  "using defualt value (#ff3535)")
        settings['rec_color'] = "#ff3535"

    # Make sure that the image format is a valid one
    validImageFormats = ['BMP', 'GIF', 'JPEG', 'JPG', 'PNG']
    for imFormat in validImageFormats:
        if (imFormat.upper() == str(settings['image_format'])[1:].upper()):
            valid = True
            break
        else:
            valid = False

    if (not valid):
        if DEBUG:
            print(" image_format has an unsoported value: " +
                  settings['image_format'] +
                  ", using defualt value (png)")
        settings['image_format'] = '.png'

    # Take screen shot and save it in the temporary directory
    screen = ImageGrab.grab()
    screen.save(tempCap)

    # Create instance of UI
    root = Tk()
    root.attributes("-fullscreen", True)
    root.title("Screen Crop")
    root.wm_attributes("-topmost", 1)
    root.focus_force()

    app = ScreenCrop(root,
                     tempCap,
                     settings['save_location'],
                     settings['continuous_mode'],
                     settings['imgur_upload'],
                     settings['rec_color'],
                     settings['rec_width'],
                     settings['image_format'])

    app.mainloop()
