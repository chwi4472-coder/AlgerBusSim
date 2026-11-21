# Cloud build (no local Unity required)

This project builds a real Android APK via **GitHub Actions + game-ci/unity-builder**.
No Unity Editor, no Android SDK/NDK/JDK, and no 20 GB of disk space are needed on your PC.

## One-time setup

### 1. Create accounts (free)
- GitHub: https://github.com/signup
- Unity: https://id.unity.com (free Personal license)

### 2. Create an empty GitHub repository
- GitHub → New repository → name `AlgerBusSim` → Public → Create (no README/.gitignore).

### 3. Push this project to GitHub
In PowerShell, in the `AlgerBusSim` folder:
```
$env:PATH = "C:\Program Files\Git\cmd;" + $env:PATH
git remote add origin https://github.com/<YOUR-USERNAME>/AlgerBusSim.git
git push -u origin main
```
Use a Personal Access Token (GitHub → Settings → Developer settings → Tokens → classic, scope `repo`) if prompted.

### 4. Obtain a Unity Personal license (smallest possible way)
Licenses are **not tied to a machine or OS** — you can generate the `.ulf` on any computer (a friend's PC, a free cloud VM, a library PC) and use it in CI.

On **any** machine with ~2 GB free:
1. Download Unity Hub: https://unity.com/download (small installer).
2. Install + open Unity Hub → sign in with your Unity account.
3. Unity Hub → Preferences → Licenses → **Add** → **Get a free personal license**. (Clicking "Add" is required to actually generate the `.ulf` file — don't skip it.)
4. Locate the license file:
   - Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
   - macOS: `/Library/Application Support/Unity/Unity_lic.ulf`
   - Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`
   (Enable "show hidden files" if needed.)
5. Open `Unity_lic.ulf` in a text editor → copy its entire contents.
6. **Sign out of Unity Hub** (Preferences → Account → Sign out) so your local machine releases the Personal seat and CI can use it.
7. You may now uninstall Unity Hub from that machine — the `.ulf` contents are all CI needs.

### 5. Add GitHub Secrets
GitHub repo → Settings → Secrets and variables → Actions → New repository secret, three times:
- `UNITY_LICENSE` → paste the `.ulf` file contents.
- `UNITY_EMAIL` → your Unity account email.
- `UNITY_PASSWORD` → your Unity account password.

### 6. Build the APK
GitHub → Actions → "Build Android APK" → **Run workflow**.
First build ~30–60 min; later builds are faster (cached Library).
When finished: open the run → Artifacts → download `AlgerBusSim-APK` → `AlgerBusSim.apk`.

### 7. Install on Samsung Galaxy S23+
1. Copy the APK to the phone.
2. Settings → Apps → Special access → Install unknown apps → allow your file manager/browser.
3. Tap the APK → Install → Open.

## Notes
- Unity 6000.6.8f1 (matches `ProjectSettings/ProjectVersion.txt`).
- Build method: `AlgerBusSim.EditorTools.BuildScript.BuildAndroid` (configures + creates scene + builds).
- Output: `Builds/AlgerBusSim.apk` (ARM64, IL2CPP, ASTC) — a real APK with all game features.
- Never commit the `.ulf` or your password. They live only in GitHub Secrets.
- Public repos get unlimited Actions minutes; private repos get 2000 min/month.
