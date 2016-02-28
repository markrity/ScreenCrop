#!python34

import ScreenCrop
import pythoncom
from pyHook import HookManager, HookConstants


def OnKeyboardEvent(event):
    # in case you want to debug: uncomment next line
    if event.KeyID == HookConstants.VKeyToID('VK_SNAPSHOT'):
        ScreenCrop.main()
        print("snapshot")
    return False

# create a hook manager
hm = HookManager()
# watch for all keyboard events
hm.KeyDown = OnKeyboardEvent
# set the hook
hm.HookKeyboard()
# wait forever
while True:
    try:
        while True:
            pythoncom.PumpWaitingMessages()
    except:
        pass
