#!python34


# Path and file naming
import os
# Module settings
import json
# GUI
from tkinter import *
# Image scaling
from PIL import Image, ImageTk
# Screen capture
from PIL import ImageGrab
# File naming
from datetime import datetime
# Imgur.com api
import pyimgur
# Copy to clipboard
from pyperclip import copy

__author__ = "Michael Livs"
__version__ = "1.0"
__email__ = "livsMichael@gmail.com"

root = None


class ScreenCrop(Frame):
    """
    ScreenCrop is a python module that delivers an easy and intuative way
    to capture, crop and upload screen shots.
    """

    #                               CLASS INITIALTION
    # ===========================================

    def __init__(self, parent, path, save_path, continuous_mode, rec_color,
                 rec_width, image_format):
        Frame.__init__(self, parent)
        self.parent = parent

        # Initial variables
        self.trace = 0
        self.drawn = None

        # Settings
        self.rec_color = rec_color
        self.rec_width = rec_width
        self.continuous_mode = continuous_mode
        self.save_path = save_path
        self.image_format = image_format

        # Client id from imgur api
        self.CLIENT_ID = "be8b1a456528379"

        # Minimal frame sizes
        self.frame_width = root.winfo_screenwidth() / 2
        self.frame_height = root.winfo_screenheight() / 2

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
        self.parent.destroy()

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
            self.original.save(image_path)

        if (not self.continuous_mode):
            self.parent.withdraw()
            im = pyimgur.Imgur(self.CLIENT_ID)
            uploaded_image = im.upload_image(image_path, title=image_name)
            print(uploaded_image.title)
            print(uploaded_image.link)
            copy(uploaded_image.link)
            self.quit(None)


def main():
    """ Main entry point to module """
    # Path to temporary directory that will be created in the location
    # of the module
    tempPath = os.path.dirname(
        os.path.realpath(__file__)) + '\\temporary'

    # Path to the settings file
    settingsJson = os.path.dirname(
        os.path.realpath(__file__)) + '\\settings.json'

    # Path to where screen shots directory will be saved
    defualtPath = os.path.dirname(
        os.path.realpath(__file__)) + '\\Screen Shots'

    # Path to temporary screen shot
    tempCap = tempPath + '\\screencap.png'

    # Open settings file
    with open(settingsJson, 'r+') as file:
        settings = json.load(file)

    if (settings['save_location'] is None):
        settings['save_location'] = defualtPath

    # If doesn't exist, create temporary directory
    if not os.path.exists(tempPath):
        os.makedirs(tempPath)

    # If doesn't exist, create screens shots save directory
    if not os.path.exists(settings['save_location']):
        os.makedirs(settings['save_location'])

    # Take screen shot and save it in the temporary directory
    screen = ImageGrab.grab()
    screen.save(tempCap)

    # Create instance of UI
    global root
    root = Tk()
    root.attributes("-fullscreen", True)
    root.title("Screen Crop")
    root.wm_attributes("-topmost", 1)
    root.focus_force()

    app = ScreenCrop(root,
                     tempCap,
                     settings['save_location'],
                     settings['continuous_mode'],
                     settings['rec_color'],
                     settings['rec_width'],
                     settings['image_format'])

    app.mainloop()
