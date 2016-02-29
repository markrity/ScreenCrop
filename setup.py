import sys
import cx_Freeze

base = None
if sys.platform == 'win32':
    base = "Win32GUI"

executables = [
    cx_Freeze.Executable("ScreenCrop.pyw", base=base, icon="camicon.ico")]


cx_Freeze.setup(
    name="ScreenCrop",
    options={"build_exe": {"packages": packages,
                           "include_files": ["camicon.ico", "settings.json"]}},
    version="0.1",
    description="ScreenCrop",
    executables=executables
)
