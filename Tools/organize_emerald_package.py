#!/usr/bin/env python3
"""Repath a Unity-produced runtime package without changing source assets or GUIDs."""
import io
import json
from pathlib import Path, PurePosixPath
import re
import sys
import tarfile

ROOT = "Assets/EmeraldMoves/"


def destination(path):
    special = {
        "Assets/BotwVFX/EmeraldPackageREADME.md": "README.md",
        "Assets/EmeraldMoveVFX/UPSTREAM-LICENSE.txt": "Licenses/Emerald-UPSTREAM-LICENSE.txt",
        "Assets/EmeraldMoveVFX/README.md": "Documentation/UpstreamREADME.md",
        "Assets/BotwVFX/Models/Substitute/LICENSE.txt": "Licenses/Substitute-LICENSE.txt",
        "Assets/BotwVFX/Models/Substitute/Source/license.txt": "Licenses/Substitute-original-license.txt",
    }
    if path in special:
        return ROOT + special[path]
    for old, new in (
        ("Assets/BotwVFX/Prefabs/Emerald/", "Prefabs/"),
        ("Assets/EmeraldMoveVFX/Runtime/", "Scripts/Runtime/"),
        ("Assets/EmeraldMoveVFX/Data/", "Data/"),
        ("Assets/BotwVFX/", ""),
    ):
        if path.startswith(old):
            return ROOT + new + path[len(old):]
    raise ValueError("Unexpected asset path: " + path)


def organize(source, output):
    output = Path(output)
    if output.exists():
        raise FileExistsError(output)
    entries = {}
    with tarfile.open(source, "r|gz") as archive:
        for member in archive:
            parts = PurePosixPath(member.name).parts
            if member.name.startswith("/") or ".." in parts or member.issym() or member.islnk():
                raise ValueError("Unsafe archive member")
            if member.isfile():
                if member.name in entries:
                    raise ValueError("Duplicate archive entry")
                entries[member.name] = archive.extractfile(member).read()
    mapping, guids, paths = {}, set(), set()
    for name, data in entries.items():
        if not name.endswith("/pathname"):
            continue
        old = data.decode().strip()
        new = destination(old)
        guid = name.split("/")[0]
        if not re.fullmatch(r"[0-9a-f]{32}", guid) or guid in guids or new in paths:
            raise ValueError("Duplicate GUID/path")
        guids.add(guid)
        paths.add(new)
        mapping[old] = new
        entries[name] = new.encode()
        asset = guid + "/asset"
        # Only textual docs require project-root substitutions. Shader includes
        # remain siblings or refer to declared Unity/UPM libraries.
        if old.endswith(".md") and asset in entries:
            text = entries[asset].decode()
            if old == "Assets/EmeraldMoveVFX/README.md":
                text = ("# Upstream provenance\n\n"
                        "Baked move data and EmeraldVfxPlayer originate from pokemon-emerald-unity-vfx, "
                        "derived from NicoRuedaA/pokemon-emerald-arena at commit "
                        "97d9f960321e686b597d462bd5f7c632e81e61eb.\n\n"
                        "Retain [upstream license](../Licenses/Emerald-UPSTREAM-LICENSE.txt); "
                        "it does not relicense Pokemon material.\n\n"
                        "The original project's generation/gallery tooling is not included. "
                        "See [package README](../README.md) for usage.\n")
            for previous, current in sorted(mapping.items(), key=lambda item: -len(item[0])):
                text = text.replace(previous, current)
            entries[asset] = text.encode()
    for name, data in entries.items():
        if name.endswith("/asset.meta"):
            actual = re.search(rb"^guid: ([0-9a-f]{32})", data, re.M)
            if not actual or actual.group(1).decode() != name.split("/")[0]:
                raise ValueError("Metadata GUID mismatch")
    if sum(p.endswith(".prefab") for p in paths) != 165:
        raise ValueError("Expected 165 prefabs")
    output.parent.mkdir(parents=True, exist_ok=True)
    # Exclusive creation protects existing user packages even under a race.
    with output.open("xb") as target:
        with tarfile.open(fileobj=target, mode="w:gz") as archive:
            for name, data in sorted(entries.items()):
                item = tarfile.TarInfo(name)
                item.size, item.mode = len(data), 0o644
                archive.addfile(item, io.BytesIO(data))
    Path(str(output) + ".contents.txt").write_text("\n".join(sorted(paths)) + "\n")
    Path(str(output) + ".path-map.json").write_text(json.dumps(mapping, indent=2) + "\n")
    print(f"Organized {len(paths)} assets under {ROOT}")


if __name__ == "__main__":
    organize(*sys.argv[1:])
