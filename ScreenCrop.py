
import os
from tkinter import *
from PIL import Image, ImageTk
from PIL import ImageGrab


class ResizingCanvas(Canvas):
    def __init__(self, parent, **kwargs):
        Canvas.__init__(self, parent, **kwargs)
        self.bind("<Configure>", self.on_resize)
        self.height = self.winfo_reqheight()
        self.width = self.winfo_reqwidth()

    def on_resize(self, event):
        # determine the ratio of old width/height to new width/height
        scaleWidth = float(event.width) / self.width
        scaleHeight = float(event.height) / self.height

        minScale = min(scaleWidth, scaleHeight)

        self.width = event.width * minScale
        self.height = event.height * minScale
        # resize the canvas
        self.config(width=self.width, height=self.height)
        # rescale all the objects tagged with the "ALL" tag
        self.scale("ALL", 0, 0, minScale, minScale)


class ScreenCap(Frame):
    """docstring for ClassName"""

    def __init__(self, parent, path):
        Frame.__init__(self, parent)
        self.parent = parent

        self.frame_width = root.winfo_screenwidth() / 2
        self.frame_height = root.winfo_screenheight() / 2
        self.parent.minsize(int(self.frame_width), int(self.frame_height))

        # Main grid and container configuration
        self.parent.grid_rowconfigure(0, weight=1)
        self.parent.grid_columnconfigure(0, weight=1)
        self.main_container = Frame(self.parent)
        self.main_container.grid(row=0, column=0, sticky="nsew")
        self.main_container.grid_rowconfigure(0, weight=1)
        self.main_container.grid_columnconfigure(0, weight=1)

        # Deviding the main container to two seperated parts
        self.top_frame = Frame(self.main_container, relief="groove",
                               borderwidth=3, padx=2, pady=5)
        self.bottom_frame = Frame(self.main_container)

        # Positioning top_frame and bottom_frame in grid
        self.top_frame.grid(row=0, column=0, sticky="nsew")
        self.bottom_frame.grid(row=1, column=0, sticky="nsew")

        # Row and column configuration to allow resizing
        self.top_frame.grid_rowconfigure(0, weight=1)
        self.top_frame.grid_columnconfigure(0, weight=1)

        """
        self.bottom_frame.grid_rowconfigure(0, weight=1)
        self.bottom_frame.grid_columnconfigure(1, weight=1)
        """

        # A frame to hold the buttons and seperate them from the rest of the
        # GUI
        self.button_frame = Frame(self.bottom_frame)
        self.button_frame.grid(row=1, column=0, sticky="nsew", columnspan=3,
                               padx=5, pady=5)
        """
        self.button_frame.grid_rowconfigure(1, weight=1)
        self.button_frame.grid_columnconfigure(1, weight=1)
        """

        # Button objects initiation
        self.buttonSave = Button(self.button_frame, text='Save',
                                 command=self.save)
        self.buttonCancel = Button(self.button_frame, text='Cancel',
                                   command=self.quit)
        self.ButtonBrowse = Button(self.button_frame, text='Browse',
                                   command=self.browse)

        # Button positioning in grid
        self.buttonSave.grid(row=0, column=0, sticky="w")
        self.ButtonBrowse.grid(row=0, column=1, sticky="n")
        self.buttonCancel.grid(row=0, column=2, sticky="e")

        """ Set screen capture in canvas """
        self.original = Image.open(path)
        self.image = ImageTk.PhotoImage(self.original)
        self.display = ResizingCanvas(self.top_frame, highlightthickness=0)
        self.display.grid(row=0, column=0, sticky="nsew")
        self.display.grid_rowconfigure(0, weight=1)
        self.display.grid_columnconfigure(0, weight=1)

        self.display.create_image(0, 0, image=self.image, anchor="nw",
                                  tags="IMG")

        # Event bindings
        self.main_container.bind("<Configure>", self.resize)
        self.display.bind('<Motion>', self.motion)

    def motion(self, event):
        x, y = event.x, event.y
        print('{}, {}'.format(x, y))

    def resize(self, event):

        # Calculate scale ratio
        scaleWidth = float(event.width) / self.display.width
        scaleHeight = float(event.height) / self.display.height

        # Get minimum scaling ratio
        scale = min(scaleWidth, scaleHeight)

        # Use minimal scaling ratio to scale image
        size = int(event.width * scale), int(event.height * scale)

        # Delete previus image and create a new scale image
        resized = self.original.resize(size, Image.ANTIALIAS)
        self.image = ImageTk.PhotoImage(resized)
        self.display.delete("IMG")
        self.display.create_image(0, 0, image=self.image, anchor=NW,
                                  tags="IMG")

    def quit(self):
        self.destroy()
        self.parent.destroy()

    def save(self):
        print("Save")

    def browse(self):
        print("Browse")


if __name__ == '__main__':
    """ Main entry point to module """
    # Path to temporary directory
    tempPath = os.path.dirname(os.path.realpath(__file__)) + '\\temporary'

    # Create temporary directory
    if not os.path.exists(tempPath):
        os.makedirs(tempPath)

    tempPath = tempPath + '\\screencap.png'

    # Take screen shot and save it in the temporary directory
    screen = ImageGrab.grab()
    screen.save(tempPath)

    # Create instance of UI
    root = Tk()
    root.title("Screen Crop")
    # root.state('zoomed')

    app = ScreenCap(root, tempPath)

    app.mainloop()
