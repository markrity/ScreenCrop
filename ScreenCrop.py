
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
        wscale = float(event.width) / self.width
        hscale = float(event.height) / self.height
        self.width = event.width
        self.height = event.height
        if (wscale / hscale == self.width / self.height):
            # resize the canvas
            self.config(width=self.width, height=self.height)
            # rescale all the objects tagged with the "all" tag
            self.scale("IMG", 0, 0, wscale, hscale)


class ScreenCap(Frame):
    """docstring for ClassName"""

    def __init__(self, parent, path):
        Frame.__init__(self, parent)
        self.parent = parent

        self.frame_width = root.winfo_screenwidth() / 2
        self.frame_height = root.winfo_screenheight() / 2

        # Frame initialtion
        self.canvas_frame = Frame(self, width=self.frame_width,
                                  height=self.frame_height)
        self.buttons_frame = Frame(self, width=self.frame_width)

        # Buttons initiation
        self.buttonSave = Button(self.buttons_frame, text='Save')
        self.buttonCancel = Button(
            self.buttons_frame, text='Cancel', command=self.quit)

        # Packing frames, canvas and buttons
        self.canvas_frame.pack(fill=BOTH, expand=YES)
        self.buttons_frame.pack(side=BOTTOM)
        self.buttonSave.pack(side=LEFT, padx=5, pady=5)
        self.buttonCancel.pack(side=LEFT, padx=5, pady=5)

        ''' Set screen capture in canvas '''
        # Use the image we took at the main function of the module
        self.original = Image.open(path)
        self.image = ImageTk.PhotoImage(self.original)
        # Initiat a resizeable canvas that would scale the image to the
        # canvas size and ratio
        self.display = ResizingCanvas(
            self.canvas_frame, bd=0, highlightthickness=0,
            borderwidth=2,
            relief=RIDGE)
        # Set the screen capture on the canvas
        self.display.create_image(
            0, 0, image=self.image, anchor=NW, tags="IMG")
        # Set a grid and pack the display canvas
        self.display.grid(row=0, sticky=W + E + N + S)
        self.display.pack(fill=BOTH, expand=YES)

        # Event bindings
        self.bind("<Configure>", self.resize)
        self.display.bind('<Motion>', self.motion)

        # Pack frame
        self.pack(fill=BOTH, expand=1, padx=20, pady=20)

    def motion(self, event):
        x, y = event.x, event.y
        print('{}, {}'.format(x, y))

    def resize(self, event):
        size = (event.width, event.height)
        resized = self.original.resize(size, Image.ANTIALIAS)
        self.image = ImageTk.PhotoImage(resized)
        self.display.delete("IMG")
        self.display.create_image(
            0, 0, image=self.image, anchor=NW, tags="IMG")

    def quit(self):
        self.destroy()
        self.parent.destroy()


if __name__ == '__main__':
    # Create temporary path and directory
    tempPath = os.path.dirname(os.path.realpath(__file__)) + '\\temporary'

    if not os.path.exists(tempPath):
        os.makedirs(tempPath)

    tempPath = tempPath + '\\screencap.png'

    screen = ImageGrab.grab()
    screen.save(tempPath)

    root = Tk()
    root.title("Screen Crop")
    root.state('zoomed')

    app = ScreenCap(root, tempPath)

    app.mainloop()
