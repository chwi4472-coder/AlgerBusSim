# Cloud build (no local Unity required)

This project builds a real Android APK via **GitHub Actions + game-ci/unity-builder**.
No Unity Editor, no Android SDK/NDK/JDK, and no 20 GB of disk space are needed on your PC.

## One-time setup (you do this once)

### 1. Create a GitHub account + Unity account
- GitHub: https://github.com (free).
- Unity: https://id.unity.com (free Personal license).

### 2. Create an empty GitHub repository
- On GitHub: New repository → name it `AlgerBusSim` → Public (free Actions minutes) → Create.
- Do NOT add a README/.gitignore (we already have them).

### 3. Push this project to GitHub
Open PowerShell in the project folder (`AlgerBusSim/`) and run:
```
git remote add origin https://github.com/<YOUR-USERNAME>/AlgerBusSim.git
git push -u origin main
```
If git asks for credentials, use a Personal Access Token (GitHub → Settings → Developer settings → Personal access tokens → classic → scope `repo`).

### 4. Get your Unity license (free, one-time)
1. On GitHub, go to your repo → **Actions** → select **"Request Unity Activation File"** → **Run workflow**.
2. Wait ~2 min. Then open the run → **Artifacts** → download `unity-activation-file` (a `.alf` file).
3. Go to https://license.unity3d.com/manual → upload the `.alf` file → download the `.ulf` file Unity returns.
4. Open the `.ulf` file in a text editor, copy its entire contents.
5. On GitHub: repo → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**:
   - Name: `UNITY_LICENSE`
   - Value: paste the `.ulf` contents → Save.

### 5. Trigger the APK build
- GitHub → **Actions** → **"Build Android APK"** → **Run workflow**.
- First build takes ~30–60 min (Unity download + build). Subsequent builds are faster (cached Library).
- When it finishes: open the run → **Artifacts** → download `AlgerBusSim-APK` → you get `AlgerBusSim.apk`.

### 6. Install on your Samsung Galaxy S23+
1. Transfer the APK to the phone (USB / cloud / email).
2. On the phone: Settings → Apps → Special access → Install unknown apps → allow your file manager/browser.
3. Tap the APK → Install → Open.

## Notes
- The build uses Unity 6000.6.8f1 (matches `ProjectSettings/ProjectVersion.txt`).
- Build method: `AlgerBusSim.EditorTools.BuildScript.BuildAndroid` (configures + creates scene + builds).
- Output: `Builds/AlgerBusSim.apk` (ARM64, IL2CPP, ASTC).
- Free tier: public repos get unlimited Actions minutes. Private repos get 2000 min/month (enough for ~2-3 builds).
- The `.ulf` license is tied to your Unity account; never commit it. It lives only in GitHub Secrets.
