# Astrovisio Client

**Astrovisio Client** is a Unity-based VR application for immersive visualization of astrophysical datasets. It provides interactive tools to explore scientific data using colormaps and advanced data mapping techniques.

## Requirements

- Windows 10/11 (64-bit)
- GPU with VR support and updated drivers
- VR headset compatible with OpenXR (e.g., Meta Quest 2 or 3 via Link – *optional, see below*)

## Prerequisites

Before launching Astrovisio Client, make sure the following components are installed:

- **Docker Desktop**  
  The application relies on a backend API server for data access and processing.  
  This server is provided by [AstroAPI](https://github.com/Astrovisio/AstroAPI), which runs as a Docker container.
  You must first install **Docker** and run AstroAPI using the instructions in its repository.

> ⚠️ Docker and AstroAPI must be running before starting the client, or the application will not be able to load datasets.

- **Meta Quest Link** (optional)  
  Required **only** if you intend to use the application in **VR mode**.  
  You can download Meta Quest Link here:  
  👉 [https://www.meta.com/quest/setup/](https://www.meta.com/quest/setup/)

## VR Setup

Astrovisio currently supports:

- **Meta Quest 2**
- **Meta Quest 3**

To enable VR functionality:

1. Install **Meta Quest Link** on your PC.
2. Connect your headset using:
   - **Link Cable** (recommended for best performance)
   - Or **Air Link** (for wireless streaming)
3. Ensure your headset is **set to PC VR mode** via the Meta Link interface.

> ❗ Without Meta Quest Link properly configured, the application will not enter VR mode.

## How to Run

1. Download the latest version from the [Releases](https://github.com/Astrovisio/astrovisio-unity/releases) section.
2. Extract the downloaded `.zip` archive.
3. Open the extracted folder.
4. **Double-click on `Astrovisio.exe`** to launch the application.

> ⚠️ Ensure that Docker + AstroAPI are running before launching.  
> 🎮 VR headset is optional – required only for immersive data visualization.
