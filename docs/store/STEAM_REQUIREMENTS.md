# Steam store page: system requirements and platforms

Values for the Steamworks "Supported Platforms" screen (Store Presence, Basic Info), worked out on 2026-10-08. The store page text itself is in `STORE_PAGE.md`.

## What was measured
- **Memory:** the Windows release build used about 520 to 560 MB of RAM (about 900 MB committed) at the main menu, in the village and on the farm, 25 seconds after start.
- **Graphics API:** the Windows build chose Direct3D 12 and falls back to Direct3D 11 (Auto Graphics API: D3D12 then D3D11). Linux tries Vulkan, then OpenGL Core (an OpenGL 4.2 window was used on Ubuntu 22.04 under WSLg). macOS uses Metal.
- **Disk:** the Steam depots are 134 MB (Windows), 132 MB (Linux) and 112 MB (macOS).
- **Unity 6 player requirements** (docs.unity3d.com, system requirements): Windows 10 (21H1 or later, 64-bit) with a DX10/11/12 or Vulkan GPU; macOS 11 (Big Sur) or later with a Metal GPU; Ubuntu 22.04 or later with Vulkan or OpenGL 3.2.
- Not measured: CPU and video memory on slow hardware. The processor and graphics lines below are conservative estimates for a small 2D game.

## Windows
| Field | Minimum | Recommended |
|---|---|---|
| OS Version | Windows 10 (64-bit) | Windows 10 or 11 (64-bit) |
| Processor | Dual-core 2 GHz, x86-64 with SSE2 | Quad-core 3 GHz or faster |
| Memory | 2048 MB | 4096 MB |
| Graphics | DirectX 11 capable GPU, 512 MB video memory (integrated is fine) | DirectX 11 or 12 GPU, 1 GB video memory |
| Network | not ticked (single-player, runs offline) | not ticked |
| DirectX Version | 11 | 12 |
| Disk Space | 300 MB | 500 MB |
| Sound Card | blank | blank |
| VR | blank | blank |
| Additional Notes | Keyboard and mouse. Xbox-style controller optional. | same |

## macOS
| Field | Minimum | Recommended |
|---|---|---|
| OS Version | macOS 11 (Big Sur) or later | macOS 12 or later |
| Processor | Intel Core i5 or better. Intel Only | same |
| Memory | 2048 MB | 4096 MB |
| Graphics | Metal capable GPU (Intel integrated is fine) | Metal capable GPU, 1 GB video memory |
| Disk Space | 300 MB | 500 MB |
| Additional Notes | Intel build. Not yet tested on Apple Silicon (runs through Rosetta 2). | same |

Steam's hint on that screen asks for "Intel Only" when the build is Intel only. State that it runs well through Rosetta only after a tester has confirmed it. **The macOS build has never been run on a Mac.**

## Linux + SteamOS
| Field | Minimum | Recommended |
|---|---|---|
| OS Version | Ubuntu 22.04 or later, or SteamOS (64-bit) | same |
| Processor | Dual-core 2 GHz, x86-64 with SSE2 | Quad-core 3 GHz or faster |
| Memory | 2048 MB | 4096 MB |
| Graphics | Vulkan or OpenGL 3.2 capable GPU with current Mesa or NVIDIA drivers | same, 1 GB video memory |
| Disk Space | 300 MB | 500 MB |
| Additional Notes | Tested on Ubuntu 22.04. Controller optional. | same |

## Before release
- Tick macOS only if a tester has started the Mac build; tick Linux knowing it has run only on Ubuntu 22.04 (WSL2) so far.
- The release date on the page is Dec 1, 2026 ("Coming soon"). Check Valve's lead times for the Coming Soon page and the build review in the Steamworks checklist.
