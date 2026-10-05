"""End-to-end check with REAL injected mouse input in the built Windows player (SendInput-style absolute moves, so the game sees them as a mouse
moving, not just a cursor teleport). Launches the game, focuses it, moves the mouse around the avatar and screenshots the screen each time.
Usage: python real_cursor2.py <exe> <outdir> [extra player args...]   Windows only."""
import ctypes, ctypes.wintypes as wt, subprocess, sys, time, os
import numpy as np
from PIL import ImageGrab

EXE, OUT = sys.argv[1], sys.argv[2]
os.makedirs(OUT, exist_ok=True)
user32 = ctypes.windll.user32
user32.SetProcessDPIAware()
SW, SH = user32.GetSystemMetrics(0), user32.GetSystemMetrics(1)

proc = subprocess.Popen([EXE] + sys.argv[3:] + ["-screen-width", "1280", "-screen-height", "720", "-screen-fullscreen", "0", "-farmScene", "Farm"])
pid = proc.pid

def find_window():
    found = []
    @ctypes.WINFUNCTYPE(wt.BOOL, wt.HWND, wt.LPARAM)
    def cb(h, l):
        p = wt.DWORD()
        user32.GetWindowThreadProcessId(h, ctypes.byref(p))
        if p.value == pid and user32.IsWindowVisible(h): found.append(h)
        return True
    user32.EnumWindows(cb, 0)
    return found[0] if found else None

hwnd = None
end = time.time() + 60
while time.time() < end and hwnd is None:
    time.sleep(0.5)
    hwnd = find_window()
if hwnd is None:
    proc.kill(); sys.exit("no window")
time.sleep(10)

def focus(h):
    user32.ShowWindow(h, 9)
    user32.keybd_event(0x12, 0, 0, 0)
    user32.SetForegroundWindow(h)
    user32.keybd_event(0x12, 0, 2, 0)
    user32.BringWindowToTop(h)
    time.sleep(0.8)
    return user32.GetForegroundWindow() == h
for _ in range(5):
    if focus(hwnd): break
print("window has focus:", user32.GetForegroundWindow() == hwnd)

pt = wt.POINT(0, 0)
user32.ClientToScreen(hwnd, ctypes.byref(pt))
rect = wt.RECT(); user32.GetClientRect(hwnd, ctypes.byref(rect))
W, H, ox, oy = rect.right, rect.bottom, pt.x, pt.y
print("client origin", ox, oy, "size", W, H, "screen", SW, SH)

MOVE, ABS = 0x0001, 0x8000
def hw_move(x, y):                                   # an absolute mouse move as a real device would send it
    user32.mouse_event(MOVE | ABS, int(x * 65535 / (SW - 1)), int(y * 65535 / (SH - 1)), 0, 0)

def move_to(cx, cy):
    x, y = ox + cx, oy + cy
    for dx, dy in ((-30, -20), (-10, -5), (0, 0), (2, 1), (0, 0)):
        hw_move(x + dx, y + dy)
        time.sleep(0.12)
    time.sleep(0.5)

def shot(name):
    img = ImageGrab.grab(bbox=(ox, oy, ox + W, oy + H))
    img.save(os.path.join(OUT, name + ".png"))
    return img

def yellow_box(img):
    a = np.asarray(img.convert("RGB")).astype(int)
    m = (a[:, :, 0] > 225) & (a[:, :, 1] > 185) & (a[:, :, 2] < 120)
    m[:230, :] = False; m[520:, :] = False; m[:, :120] = False; m[:, 560:] = False
    ys, xs = np.where(m)
    if len(xs) < 20: return None
    return (int(xs.min()), int(ys.min()), int(xs.max()), int(ys.max()))

AVATAR = (240, 376)       # the middle of the avatar's cell, in client pixels (the camera stops at the map's left edge, so it is not mid-screen)
hw_move(ox + 700, oy + 600)
time.sleep(1.0)
base = yellow_box(shot("00_baseline"))
print("baseline square (mouse idle):", base)

cases = [("own_cell", 0, 0), ("right", 32, 0), ("left", -32, 0), ("above", 0, -32), ("below", 0, 32),
         ("up_right", 32, -32), ("down_left", -32, 32), ("far_up_right", 220, -110)]
for name, dx, dy in cases:
    move_to(AVATAR[0] + dx, AVATAR[1] + dy)
    box = yellow_box(shot("01_" + name))
    if box:
        cx, cy = (box[0] + box[2]) / 2, (box[1] + box[3]) / 2
        print(f"{name:13s} square centre ({cx:.0f},{cy:.0f})  offset from avatar cell: ({(cx-AVATAR[0])/32:+.1f},{(cy-AVATAR[1])/32:+.1f}) cells")
    else:
        print(f"{name:13s} no square found")
proc.kill()
