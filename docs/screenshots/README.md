# Screenshots

This folder contains screenshots of the Desktop Terrarium application for documentation purposes.

## Available Screenshots

*(Add screenshots as PNG files with descriptive names)*

| Screenshot | Description |
|------------|-------------|
| `terrarium-main.png` | Main application view with ecosystem |
| `terrarium-settings.png` | Settings panel (F2) |
| `terrarium-minimap.png` | Mini-map view (M key) |
| `terrarium-daynight.png` | Day/night cycle comparison |
| `terrarium-notifications.png` | Toast notifications |
| `terrarium-achievements.png` | Achievement system |

## How to Capture Screenshots

1. **Run the application**:
   ```bash
   cd Terrarium.Desktop
   dotnet run
   ```

2. **Set up a good scene**:
   - Press `P` several times to spawn plants
   - Press `H` to spawn herbivores
   - Press `C` to spawn a carnivore
   - Wait for the ecosystem to stabilize

3. **Capture with Windows Snipping Tool**:
   - Press `Win + Shift + S`
   - Select the terrarium area
   - Save to this folder

4. **Alternative: Full window capture**:
   - Press `Alt + Print Screen` to capture active window
   - Paste into Paint and save

## Screenshot Guidelines

- **Resolution**: Capture at 1920x1080 or higher if possible
- **Format**: PNG (lossless compression)
- **Content**: Show active ecosystem with multiple entity types
- **UI State**: Consider capturing with different UI states:
  - F1 to toggle status display
  - F2 for settings panel
  - M for mini-map
  - G for population graph

## For Website

To use screenshots on the website:

1. Place the image in `docs/screenshots/`
2. Reference in HTML:
   ```html
   <img src="screenshots/terrarium-main.png" alt="Desktop Terrarium" />
   ```

3. Or in markdown documentation:
   ```markdown
   ![Desktop Terrarium](screenshots/terrarium-main.png)
   ```

---

*Last updated: January 2026*
