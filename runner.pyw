#!python2

import ScreenCrop
import pythoncom
from pyHook import HookManager, HookConstants


def OnKeyboardEvent(event):
    # in case you want to debug: uncomment next line
    if event.KeyID == HookConstants.VKeyToID('VK_SNAPSHOT'):
        ScreenCrop.main()

    return True

# create a hook manager
hm = HookManager()
# watch for all keyboard events
hm.KeyDown = OnKeyboardEvent
# set the hook
hm.HookKeyboard()
# wait forever
pythoncom.PumpMessages()
