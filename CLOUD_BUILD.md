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
First build ~30–60 min. When done: open the run → Artifacts → download `AlgerBusSim-APK` → `AlgerBusSim.apk`.

### 7. Install on Samsung Galaxy S23+
1. Copy the APK to the phone.
2. Settings → Apps → Special access → Install unknown apps → allow your file manager/browser.
3. Tap the APK → Install → Open.

---

## ⚠️ Troubleshooting: build killed with exit code 137 (OOM)

If the build fails with `Build failed with exit code 137` (the Unity process is killed by the runner's OOM killer), the standard GitHub runner (16 GB RAM / 14 GB disk) is **not enough** for this Unity 6 Android IL2CPP build. The swap added to the workflow cannot help because the container's memory limit is enforced by cgroups.

### Free solution: Oracle Cloud Always Free (recommended)
Oracle Cloud offers a **free-forever** VM with 4 OCPUs, 24 GB RAM, 200 GB disk. Use it as a self-hosted GitHub Actions runner.

**Setup (~30 min, one-time):**
1. Create a free account: https://www.oracle.com/cloud/free/
2. Create a VM: Compute → Instances → Create. Choose:
   - Image: Ubuntu 22.04 (or 24.04).
   - Shape: VM.Standard.A1.Flex (ARM) with 4 OCPUs / 24 GB RAM — **Always Free eligible**.
   - Boot volume: 200 GB.
   - Add your SSH public key.
3. SSH into the VM and install Docker + the self-hosted runner:
   ```
   sudo apt update && sudo apt install -y docker.io
   sudo usermod -aG docker $USER
   # then log out and back in
   ```
4. On GitHub: repo → Settings → Actions → Runners → **New self-hosted runner** → choose Linux → copy the registration commands (they include a token).
5. Run those commands on the VM (in a folder like `~/actions-runner`). The runner registers with GitHub.
6. Start the runner: `./run.sh` (or install as a service: `sudo ./svc.sh install && sudo ./svc.sh start`).
7. Update the workflow: change `runs-on: ubuntu-latest` to `runs-on: self-hosted` in `.github/workflows/build-android.yml`, commit and push.
8. Re-run the build — it now runs on your 24 GB VM and should complete.

### Alternative: a friend's PC
If a friend has a PC with 16+ GB RAM and ~20 GB free disk, they can install Unity 6000.6.8f1 + Android module (free Personal license) and run the build locally:
```
Unity.exe -quit -batchmode -projectPath . -executeMethod AlgerBusSim.EditorTools.BuildScript.BuildAndroid
```
The APK lands in `Builds/AlgerBusSim.apk`.

### Notes
- Unity 6000.6.8f1 (matches `ProjectSettings/ProjectVersion.txt`).
- Build method: `AlgerBusSim.EditorTools.BuildScript.BuildAndroid`.
- Output: `Builds/AlgerBusSim.apk` (ARM64, IL2CPP, ASTC) — a real APK with all game features.
- Never commit the `.ulf` or your password. They live only in GitHub Secrets.
- Public repos get unlimited Actions minutes; private repos get 2000 min/month.
