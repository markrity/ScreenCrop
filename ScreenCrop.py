
import os
import json
from tkinter import *
from PIL import Image, ImageTk
from PIL import ImageGrab
from datetime import datetime


class ScreenCrop(Frame):
    """docstring for ClassName"""

    def __init__(self, parent, path, save_path, continuous_mode, rec_color,
                        rec_width, image_format):
        Frame.__init__(self, parent)
        self.parent = parent

        # Initial variables
        self.trace = 0
        self.drawn = None
        self.rec_color = rec_color
        self.rec_width = rec_width
        self.continuous_mode = continuous_mode
        self.save_path = save_path
        self.image_format = image_format

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
        self.display.bind('<Double-1>', self.onClear)
        self.display.bind('<ButtonPress-3>', self.onMove)

        self.pack(fill="both", expand=True)

    def onStart(self, event):
        self.shape = self.display.create_rectangle
        self.start = event
        self.drawn = None
        event.widget.delete('DRAWN')

    def onEnd(self, event):
        self.end = event

    def onGrow(self, event):
        self.display = event.widget
        if self.drawn:
            self.display.delete(self.drawn)
        objectId = self.shape(self.start.x, self.start.y, event.x, event.y,
                              outline=self.rec_color, width=self.rec_width,
                              tags="DRAWN")
        if self.trace:
            print(objectId)
        self.drawn = objectId

    def onClear(self, event):
        event.widget.delete('DRAWN')

    def onMove(self, event):
        if self.drawn:
            if self.trace:
                print(self.drawn)
            self.display = event.widget
            diffX, diffY = (event.x - self.start.x), (event.y - self.start.y)
            self.display.move(self.drawn, diffX, diffY)
            self.start = event

    def quit(self, event):
        self.parent.destroy()

    def save(self, event):
        if self.drawn:
            print(self.start.x, self.start.y)
            print(self.end.x, self.end.y)
            cropped = self.original.crop((self.start.x, self.start.y,
                                          self.end.x, self.end.y))

            cropped.save(self.save_path + '\\' +
                                   datetime.now().strftime('%Y-%m-%d_%H-%M-%S-%f') +
                                   self.image_format)

            if (self.continuous_mode == False):
             self.quit(None)

        else:
            self.original.save(self.save_path + '\\' +
                                   datetime.now().strftime('%Y-%m-%d_%H-%M-%S-%f') +
                                   self.image_format)
            self.quit(None)


    def browse(self):
        print("Browse")

if __name__ == '__main__':
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

    if (settings['save_location'] == None):
        settings['save_location']  = defualtPath

    print(settings['save_location'])
    print(settings['continuous_mode'])
    print(settings['rec_color'])

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
    root = Tk()
    root.attributes("-fullscreen", True)
    root.title("Screen Crop")

    app = ScreenCrop(root,
                                   tempCap,
                                   settings['save_location'],
                                   settings['continuous_mode'],
                                   settings['rec_color'],
                                   settings['rec_width'],
                                   settings['image_format'])

    app.mainloop()
