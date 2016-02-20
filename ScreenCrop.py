
import os
from tkinter import *
from PIL import Image, ImageTk
from PIL import ImageGrab


class ScreenCrop(Frame):
    """docstring for ClassName"""

    def __init__(self, parent, path):
        Frame.__init__(self, parent)

        self.trace = 0
        self.drawn = None

        self.parent = parent

        self.frame_width = root.winfo_screenwidth() / 2
        self.frame_height = root.winfo_screenheight() / 2

        self.parent.minsize(int(self.frame_width), int(self.frame_height))

        self.main_frame = Frame(self)
        self.main_frame.pack(side="top", fill="both", expand=True)

        self.display = Canvas(self.main_frame, highlightthickness=0)
        self.display.pack(side="top", fill="both", expand=True)

        self.original = Image.open(path)
        self.image = ImageTk.PhotoImage(self.original)

        self.display.create_image(0, 0, image=self.image, anchor="nw",
                                  tags="IMG")

        # Event bindings
        self.parent.bind("<Escape>", self.quit)
        self.display.bind("<ButtonPress-1>", self.onStart)
        self.display.bind("<B1-Motion>", self.onGrow)
        self.display.bind('<Double-1>', self.onClear)
        self.display.bind('<ButtonPress-3>', self.onMove)

        self.pack(fill="both", expand=True)

    def onStart(self, event):
        self.shape = self.display.create_rectangle
        self.start = event
        self.drawn = None
        event.widget.delete('DRAWN')

    def onGrow(self, event):
        self.display = event.widget
        if self.drawn:
            self.display.delete(self.drawn)
        objectId = self.shape(self.start.x, self.start.y, event.x, event.y,
                              outline='red', width=3, tags="DRAWN")
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
    root.attributes("-fullscreen", True)
    root.title("Screen Crop")
    # root.state('zoomed')

    app = ScreenCrop(root, tempPath)

    app.mainloop()
