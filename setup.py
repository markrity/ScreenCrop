import sys
import cx_Freeze

base = None
if sys.platform == 'win32':
    base = "Win32GUI"

executables = [
    cx_Freeze.Executable("runner.pyw", base=base, icon="camicon.ico")]

packages = ["tkinter", "pythoncom", "pyHook", "ScreenCrop"]

cx_Freeze.setup(
    name="ScreenCrop",
    options={"build_exe": {"packages": packages,
                           "include_files": ["camicon.ico", "settings.json"]}},
    version="0.1",
    description="Screen shot cropping module",
    executables=executables
)
