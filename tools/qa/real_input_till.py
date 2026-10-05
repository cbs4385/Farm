"""The playtester's scenario with REAL input in the built Windows player: walk onto grass with the keyboard, then (without walking again) aim the mouse
at the cell under the avatar and the cells around it and click: the hoe should till them. Screenshots show the result. Windows only.
Usage: python real_click.py <exe> <outdir>"""
import ctypes, ctypes.wintypes as wt, subprocess, sys, time, os
import numpy as np
from PIL import ImageGrab

EXE, OUT = sys.argv[1], sys.argv[2]
os.makedirs(OUT, exist_ok=True)
user32 = ctypes.windll.user32
user32.SetProcessDPIAware()
SW, SH = user32.GetSystemMetrics(0), user32.GetSystemMetrics(1)
proc = subprocess.Popen([EXE, "-screen-width", "1280", "-screen-height", "720", "-screen-fullscreen", "0", "-farmScene", "Farm"])
pid = proc.pid

def find_window():
    found = []
    @ctypes.WINFUNCTYPE(wt.BOOL, wt.HWND, wt.LPARAM)
    def cb(h, l):
        p = wt.DWORD(); user32.GetWindowThreadProcessId(h, ctypes.byref(p))
        if p.value == pid and user32.IsWindowVisible(h): found.append(h)
        return True
    user32.EnumWindows(cb, 0)
    return found[0] if found else None

hwnd = None
end = time.time() + 60
while time.time() < end and hwnd is None:
    time.sleep(0.5); hwnd = find_window()
if hwnd is None: proc.kill(); sys.exit("no window")
time.sleep(10)
for _ in range(5):
    user32.ShowWindow(hwnd, 9); user32.keybd_event(0x12, 0, 0, 0); user32.SetForegroundWindow(hwnd); user32.keybd_event(0x12, 0, 2, 0)
    time.sleep(0.8)
    if user32.GetForegroundWindow() == hwnd: break
print("window has focus:", user32.GetForegroundWindow() == hwnd)

pt = wt.POINT(0, 0); user32.ClientToScreen(hwnd, ctypes.byref(pt))
rect = wt.RECT(); user32.GetClientRect(hwnd, ctypes.byref(rect))
W, H, ox, oy = rect.right, rect.bottom, pt.x, pt.y
MOVE, ABS, LDOWN, LUP = 0x0001, 0x8000, 0x0002, 0x0004

def hw_move(x, y): user32.mouse_event(MOVE | ABS, int(x * 65535 / (SW - 1)), int(y * 65535 / (SH - 1)), 0, 0)
def move_to(cx, cy):
    x, y = ox + cx, oy + cy
    for dx, dy in ((-30, -20), (-10, -5), (0, 0), (2, 1), (0, 0)):
        hw_move(x + dx, y + dy); time.sleep(0.1)
    time.sleep(0.4)
def click(): user32.mouse_event(LDOWN, 0, 0, 0, 0); time.sleep(0.08); user32.mouse_event(LUP, 0, 0, 0, 0); time.sleep(0.5)
def shot(name):
    img = ImageGrab.grab(bbox=(ox, oy, ox + W, oy + H)); img.save(os.path.join(OUT, name + ".png")); return img
def yellow_box(img):
    a = np.asarray(img.convert("RGB")).astype(int)
    m = (a[:, :, 0] > 225) & (a[:, :, 1] > 185) & (a[:, :, 2] < 120)
    m[:150, :] = False; m[560:, :] = False
    ys, xs = np.where(m)
    if len(xs) < 20: return None
    return ((xs.min() + xs.max()) / 2, (ys.min() + ys.max()) / 2)

# 1. walk right with the real keyboard, off the narrow path onto grass
VK_D = 0x44
user32.keybd_event(VK_D, 0x20, 0, 0); time.sleep(0.9); user32.keybd_event(VK_D, 0x20, 2, 0)
time.sleep(0.6)

# 2. find the avatar: park the mouse far down-right of it, so the square sits one cell down-right of the avatar's cell
move_to(W - 80, H - 150)
sq = yellow_box(shot("00_calibrate"))
if sq is None: proc.kill(); sys.exit("no square found")
AV = (sq[0] - 32, sq[1] - 32)
print("avatar cell centre (client px):", AV)

# 3. aim and click each cell, never walking
for name, dx, dy in [("under_avatar", 0, 0), ("right", 1, 0), ("below", 0, 1), ("left", -1, 0), ("above", 0, -1), ("up_right", 1, -1)]:
    move_to(AV[0] + 32 * dx, AV[1] + 32 * dy)
    box = yellow_box(shot("10_aim_" + name))
    click()
    print(f"{name:13s} square at offset ({(box[0]-AV[0])/32:+.1f},{(box[1]-AV[1])/32:+.1f}) cells" if box else f"{name} no square")
move_to(W - 80, H - 60)
shot("20_result")
proc.kill()
