#!/usr/bin/env python3
"""
push_unity_to_github.py - Push a Unity project to a NEW GitHub repo, fully automatic.

Put this file in your Unity project folder (next to Assets/ and ProjectSettings/) and run:

    python push_unity_to_github.py
or point it at a project:
    python push_unity_to_github.py "C:\\path\\to\\MyUnityProject"

What it does:
    1. Finds your Unity project and checks it looks like one
    2. Installs Git, Git LFS and GitHub CLI if missing
    3. Sets git identity if missing
    4. Initializes the repo (if needed), checks .gitignore / .gitattributes
    5. Sets up Git LFS (adds default Unity LFS rules only if you have none)
    6. Logs in to GitHub through your browser (once) and CREATES the repo
    7. Commits, makes sure no large file sneaks in outside LFS, and pushes
"""

import os
import platform
import re
import shutil
import subprocess
import sys
from pathlib import Path

SYSTEM = platform.system()  # Windows / Darwin / Linux
GITHUB_FILE_LIMIT = 95 * 1024 * 1024  # GitHub rejects normal files > 100 MB

DEFAULT_LFS_PATTERNS = [
    "*.psd", "*.tga", "*.tif", "*.tiff", "*.exr", "*.hdr",
    "*.fbx", "*.blend", "*.obj", "*.max", "*.ma", "*.mb",
    "*.wav", "*.mp3", "*.ogg", "*.aif", "*.aiff",
    "*.mp4", "*.mov", "*.avi",
    "*.zip", "*.7z", "*.rar", "*.unitypackage",
]

MINIMAL_UNITY_GITIGNORE = """\
# Minimal Unity .gitignore (created by push_unity_to_github.py)
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/
[Mm]emoryCaptures/
.vs/
.idea/
.vscode/
*.csproj
*.sln
*.suo
*.user
*.pidb.meta
*.pdb.meta
*.mdb.meta
sysinfo.txt
*.apk
*.aab
*.unitypackage
.DS_Store
"""


# ------------------------------------------------------------------ helpers
def run(cmd, check=True, capture=False, cwd=None):
    """Run a command. Long-running ones stream straight to the terminal."""
    print("  $", " ".join(cmd))
    kwargs = dict(check=check, cwd=cwd)
    if capture:
        kwargs.update(capture_output=True, text=True, encoding="utf-8", errors="replace")
    return subprocess.run(cmd, **kwargs)


def out(cmd, cwd=None):
    """Run quietly and return stripped stdout ('' on failure)."""
    r = subprocess.run(
        cmd, cwd=cwd, capture_output=True, text=True, encoding="utf-8", errors="replace"
    )
    return r.stdout.strip() if r.returncode == 0 else ""


def have(binary):
    return shutil.which(binary) is not None


def refresh_path():
    """Pick up PATH changes made by installers without reopening the terminal."""
    if SYSTEM == "Windows":
        try:
            import winreg

            parts = []
            for root, sub in (
                (winreg.HKEY_LOCAL_MACHINE, r"SYSTEM\CurrentControlSet\Control\Session Manager\Environment"),
                (winreg.HKEY_CURRENT_USER, "Environment"),
            ):
                try:
                    with winreg.OpenKey(root, sub) as key:
                        value, _ = winreg.QueryValueEx(key, "Path")
                        parts.append(os.path.expandvars(value))
                except OSError:
                    pass
            os.environ["PATH"] = os.pathsep.join(parts + [os.environ.get("PATH", "")])
        except Exception:
            pass
        extras = [
            r"C:\Program Files\Git\cmd",
            r"C:\Program Files\Git LFS",
            r"C:\Program Files\GitHub CLI",
        ]
    else:
        extras = ["/usr/local/bin", "/opt/homebrew/bin", "/usr/bin"]
    for p in extras:
        if os.path.isdir(p) and p not in os.environ["PATH"]:
            os.environ["PATH"] += os.pathsep + p


def git_lfs_installed():
    return subprocess.run(
        ["git", "lfs", "version"], capture_output=True, text=True
    ).returncode == 0


def die(msg):
    print(f"\n[X] {msg}")
    sys.exit(1)


def ask(prompt, default=""):
    suffix = f" [{default}]" if default else ""
    value = input(f"{prompt}{suffix}: ").strip()
    return value or default


# ------------------------------------------------------------------ 1. project
def is_unity_project(p: Path):
    return (p / "Assets").is_dir() and (p / "ProjectSettings").is_dir()


def find_project():
    candidates = []
    if len(sys.argv) > 1:
        candidates.append(Path(sys.argv[1]).expanduser())
    candidates += [Path(__file__).resolve().parent, Path.cwd()]
    for c in candidates:
        if is_unity_project(c):
            return c.resolve()
    print("Couldn't find a Unity project (a folder with Assets/ and ProjectSettings/).")
    while True:
        p = Path(ask("Paste the full path to your Unity project").strip('"')).expanduser()
        if is_unity_project(p):
            return p.resolve()
        print("  That doesn't look like a Unity project. Try again.")


# ------------------------------------------------------------------ 2. tools
def install(label, binary, winget_id, brew, apt, dnf=None, pacman=None):
    if binary == "git-lfs":
        if have("git") and git_lfs_installed():
            return
    elif have(binary):
        return

    print(f"[..] {label} not found. Installing...")
    if SYSTEM == "Windows":
        if not have("winget"):
            die(f"winget not found. Install {label} manually, then run this script again.")
        run(["winget", "install", "--id", winget_id, "-e", "--source", "winget",
             "--accept-package-agreements", "--accept-source-agreements"], check=False)
    elif SYSTEM == "Darwin":
        if not have("brew"):
            die(f"Homebrew not found. Install {label} manually, then run this script again.")
        run(["brew", "install", brew])
    else:
        sudo = [] if os.geteuid() == 0 else ["sudo"]
        if have("apt-get"):
            run(sudo + ["apt-get", "install", "-y", apt])
        elif have("dnf"):
            run(sudo + ["dnf", "install", "-y", dnf or apt])
        elif have("pacman"):
            run(sudo + ["pacman", "-S", "--noconfirm", pacman or apt])
        else:
            die(f"Install {label} manually, then run this script again.")

    refresh_path()
    ok = (have("git") and git_lfs_installed()) if binary == "git-lfs" else have(binary)
    if not ok:
        die(f"{label} was installed but isn't visible yet. Close this terminal, open a new one and run the script again.")
    print(f"[OK] {label} installed.")


def ensure_tools():
    refresh_path()
    install("Git", "git", "Git.Git", "git", "git")
    install("Git LFS", "git-lfs", "GitHub.GitLFS", "git-lfs", "git-lfs")
    install("GitHub CLI", "gh", "GitHub.cli", "gh", "gh", "gh", "github-cli")


# ------------------------------------------------------------------ 3. identity
def ensure_identity(project):
    name = out(["git", "config", "--global", "user.name"])
    email = out(["git", "config", "--global", "user.email"])
    if not name:
        name = ask("Your name (shown on commits)")
        run(["git", "config", "--global", "user.name", name])
    if not email:
        email = ask("Your GitHub email")
        run(["git", "config", "--global", "user.email", email])
    print(f"[OK] Committing as {name} <{email}>")
    if SYSTEM == "Windows":
        run(["git", "config", "--global", "core.longpaths", "true"], check=False)


# ------------------------------------------------------------------ 4. repo files
def ensure_repo(project):
    if not (project / ".git").exists():
        print("[..] Initializing git repo...")
        run(["git", "init"], cwd=project)
        run(["git", "symbolic-ref", "HEAD", "refs/heads/main"], cwd=project)
    else:
        print("[OK] Git repo already initialized.")

    gi = project / ".gitignore"
    if not gi.exists():
        print("[!] No .gitignore found - creating a minimal Unity one.")
        gi.write_text(MINIMAL_UNITY_GITIGNORE, encoding="utf-8")
    else:
        text = gi.read_text(encoding="utf-8", errors="replace")
        if "ibrary" not in text:
            print("[!] Your .gitignore doesn't seem to ignore Library/ - the push could be huge.")
            if ask("Continue anyway? (y/n)", "n").lower() != "y":
                die("Stopped. Add Library/ to .gitignore and run again.")
        if re.search(r"^\*\.meta\s*$", text, re.M):
            print("[!] Your .gitignore ignores *.meta files. Unity needs .meta files in version control.")
            if ask("Continue anyway? (y/n)", "n").lower() != "y":
                die("Stopped. Remove '*.meta' from .gitignore and run again.")


def ensure_lfs(project):
    run(["git", "lfs", "install", "--local"], cwd=project)

    ga = project / ".gitattributes"
    existing = ga.read_text(encoding="utf-8", errors="replace") if ga.exists() else ""
    if "filter=lfs" in existing:
        print("[OK] Found existing Git LFS rules in .gitattributes.")
        return
    print("[..] No LFS rules found - adding default Unity large-file patterns.")
    for pattern in DEFAULT_LFS_PATTERNS:
        run(["git", "lfs", "track", pattern], cwd=project, capture=True)


# ------------------------------------------------------------------ 5. github
def ensure_github_login():
    if subprocess.run(["gh", "auth", "status"], capture_output=True).returncode != 0:
        print("\n[..] Logging in to GitHub. A code will be shown and your browser will open -")
        print("     since you're already logged in on the web, just paste the code and approve.\n")
        run(["gh", "auth", "login", "--hostname", "github.com",
             "--git-protocol", "https", "--web"])
    run(["gh", "auth", "setup-git"], check=False, capture=True)  # lets git push use gh's login
    user = out(["gh", "api", "user", "--jq", ".login"])
    if not user:
        die("Couldn't read your GitHub username. Run 'gh auth login' manually and try again.")
    print(f"[OK] Logged in to GitHub as {user}")
    return user


def sanitize_repo_name(name):
    name = re.sub(r"\s+", "-", name.strip())
    return re.sub(r"[^A-Za-z0-9._-]", "", name) or "unity-project"


def ensure_remote(project, user):
    existing = out(["git", "remote", "get-url", "origin"], cwd=project)
    if existing:
        print(f"[OK] Remote 'origin' already set: {existing}")
        return

    name = sanitize_repo_name(ask("Repository name on GitHub", sanitize_repo_name(project.name)))
    private = ask("Make it private? (y/n)", "y").lower() != "n"
    full = f"{user}/{name}"

    exists = subprocess.run(["gh", "repo", "view", full], capture_output=True).returncode == 0
    if exists:
        print(f"[!] {full} already exists on GitHub.")
        if ask("Push into the existing repo? (y/n)", "n").lower() != "y":
            die("Stopped. Run again and pick a different repository name.")
        run(["git", "remote", "add", "origin", f"https://github.com/{full}.git"], cwd=project)
    else:
        print(f"[..] Creating {'private' if private else 'public'} repo {full} ...")
        run(["gh", "repo", "create", full,
             "--private" if private else "--public",
             "--source", ".", "--remote", "origin"], cwd=project)
    print("[OK] Remote ready.")


# ------------------------------------------------------------------ 6. commit + push
def large_files_outside_lfs(project):
    lfs_files = set(out(["git", "lfs", "ls-files", "-n"], cwd=project).splitlines())
    tracked = out(["git", "ls-files"], cwd=project).splitlines()
    bad = []
    for rel in tracked:
        f = project / rel
        try:
            if f.is_file() and f.stat().st_size > GITHUB_FILE_LIMIT and rel not in lfs_files:
                bad.append(rel)
        except OSError:
            pass
    return bad


def stage_and_check(project):
    run(["git", "add", "-A"], cwd=project)

    bad = large_files_outside_lfs(project)
    if not bad:
        return
    print("\n[!] These files are over GitHub's 100 MB limit and aren't tracked by LFS:")
    for b in bad:
        print("    ", b)
    print("[..] Adding them to Git LFS automatically...")
    for rel in bad:
        ext = Path(rel).suffix
        run(["git", "lfs", "track", f"*{ext}" if ext else rel], cwd=project, capture=True)
    run(["git", "rm", "-r", "--cached", ".", "-q"], cwd=project)  # re-stage so LFS rules apply
    run(["git", "add", "-A"], cwd=project)

    bad = large_files_outside_lfs(project)
    if bad:
        die("Some files are still too large for a normal push:\n  " + "\n  ".join(bad))


def commit(project):
    has_commits = subprocess.run(
        ["git", "rev-parse", "HEAD"], cwd=project, capture_output=True
    ).returncode == 0
    pending = out(["git", "status", "--porcelain"], cwd=project)

    if pending:
        msg = "Initial commit" if not has_commits else ask("Commit message", "Update Unity project")
        run(["git", "commit", "-m", msg], cwd=project)
    elif not has_commits:
        die("Nothing to commit - is the project folder empty?")
    else:
        print("[OK] Nothing new to commit.")


def push(project):
    branch = out(["git", "rev-parse", "--abbrev-ref", "HEAD"], cwd=project) or "main"
    run(["git", "config", "http.postBuffer", "524288000"], cwd=project)
    print(f"\n[..] Pushing '{branch}' to GitHub (large LFS uploads can take a while)...\n")
    r = run(["git", "push", "-u", "origin", branch], cwd=project, check=False)
    if r.returncode != 0:
        die("Push failed. Fix the error above and run this script again - "
            "LFS uploads that already finished won't be re-sent.")
    url = out(["gh", "repo", "view", "--json", "url", "--jq", ".url"], cwd=project)
    print(f"\n[OK] Done! Your project is on GitHub: {url}")


def main():
    project = find_project()
    print(f"Unity project: {project}\n")

    ensure_tools()
    ensure_identity(project)
    ensure_repo(project)
    ensure_lfs(project)
    user = ensure_github_login()
    ensure_remote(project, user)
    stage_and_check(project)
    commit(project)
    push(project)


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\nCancelled.")
    except subprocess.CalledProcessError as e:
        die(f"A command failed (exit code {e.returncode}). See the output above.")
