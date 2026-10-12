"""Build VPM ZIPs and unitypackages for the two public packages without Unity."""

import argparse
import gzip
import io
import json
from pathlib import Path
import re
import tarfile
import uuid
import zipfile
from urllib.parse import quote


PROJECT = Path(__file__).resolve().parents[1]
PUBLIC_PACKAGES = (
    "com.kinankomoti.takenokoshader",
    "com.kinankomoti.takenokoshaderarealight",
)
DEVELOPMENT_PACKAGE = "com.kinankomoti.shaderbindingexporter"
DEFAULT_REPOSITORY = "kinakomoti-321/TakenokoShader"
UNITY_INSTALL_PATHS = {
    "com.kinankomoti.takenokoshader": "Assets/TakenokoShader",
    "com.kinankomoti.takenokoshaderarealight": "Assets/TakenokoShaderAreaLight",
}


def json_bytes(value):
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")


def package_files(directory):
    for path in sorted(directory.rglob("*")):
        relative = path.relative_to(directory)
        if any(part.startswith(".") or part.endswith("~") for part in relative.parts):
            continue
        if path.is_file():
            yield relative.as_posix(), path.read_bytes()


def write_zip(archive, files):
    with zipfile.ZipFile(archive, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=9) as output:
        for relative, data in sorted(files.items()):
            entry = zipfile.ZipInfo(relative)
            entry.compress_type = zipfile.ZIP_DEFLATED
            entry.external_attr = 0o100644 << 16
            output.writestr(entry, data)


def write_unitypackage(archive, files, install_path):
    """Write assets and GUIDs in Unity's GUID/asset, meta, pathname format."""
    root_guid = uuid.uuid5(uuid.NAMESPACE_URL, install_path).hex
    root_meta = (
        f"fileFormatVersion: 2\nguid: {root_guid}\nfolderAsset: yes\n"
        "DefaultImporter:\n  externalObjects: {}\n  userData: \n"
        "  assetBundleName: \n  assetBundleVariant: \n"
    ).encode("utf-8")
    assets = [(root_guid, install_path, root_meta, None)]
    guids = {root_guid}
    for relative, data in sorted(files.items()):
        if not relative.endswith(".meta") and relative + ".meta" not in files:
            raise ValueError(f"Missing .meta: {install_path}/{relative}")
        if not relative.endswith(".meta"):
            continue
        match = re.search(rb"^guid: ([0-9a-fA-F]{32})\s*$", data, re.MULTILINE)
        if not match:
            raise ValueError(f"Invalid GUID: {install_path}/{relative}")
        guid = match[1].decode("ascii").lower()
        if guid in guids:
            raise ValueError(f"Duplicate GUID: {install_path}/{relative}")
        guids.add(guid)
        asset_path = relative[:-5]
        asset = files.get(asset_path)
        if asset is None and not re.search(rb"^folderAsset: yes\s*$", data, re.MULTILINE):
            raise ValueError(f"Orphan .meta: {install_path}/{relative}")
        if asset is not None and Path(asset_path).suffix in (".shader", ".hlsl", ".cginc", ".compute"):
            for package_name, asset_directory in UNITY_INSTALL_PATHS.items():
                pattern = rb'(?m)(^[ \t]*#[ \t]*include[ \t]+")Packages/' + re.escape(package_name.encode("utf-8")) + rb'/'
                asset = re.sub(pattern, rb'\g<1>' + asset_directory.encode("utf-8") + b'/', asset)
        assets.append((guid, f"{install_path}/{asset_path}", data, asset))

    # Fixed headers make repeated builds byte-for-byte identical.
    with archive.open("wb") as raw:
        with gzip.GzipFile(filename="", mode="wb", fileobj=raw, mtime=0) as compressed:
            with tarfile.open(fileobj=compressed, mode="w", format=tarfile.USTAR_FORMAT) as output:
                for guid, pathname, meta, asset in assets:
                    directory = tarfile.TarInfo(guid)
                    directory.type = tarfile.DIRTYPE
                    directory.mode = 0o755
                    output.addfile(directory)
                    contents = {"pathname": pathname.encode("utf-8"), "asset.meta": meta}
                    if asset is not None:
                        contents["asset"] = asset
                    for filename, data in contents.items():
                        entry = tarfile.TarInfo(f"{guid}/{filename}")
                        entry.size = len(data)
                        entry.mode = 0o644
                        output.addfile(entry, io.BytesIO(data))


def build(args):
    args.output.mkdir(parents=True, exist_ok=True)
    # Remove obsolete distribution files left by earlier builder versions.
    stale_files = [args.output / "index.json"]
    stale_files.extend(args.output.glob(f"{DEVELOPMENT_PACKAGE}-*.tgz"))
    for stale_archive in stale_files:
        if stale_archive.is_file():
            stale_archive.unlink()

    for name in PUBLIC_PACKAGES:
        directory = args.project / "Packages" / name
        manifest = json.loads((directory / "package.json").read_text(encoding="utf-8"))
        if manifest["name"] != name or manifest.get("license") != "MIT":
            raise ValueError(f"Invalid package name or license: {name}")
        if not (directory / "LICENSE.md").is_file():
            raise ValueError(f"Missing LICENSE.md: {name}")
        version = manifest["version"]
        if not re.fullmatch(r"[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?", version):
            raise ValueError(f"Invalid package version: {name} {version}")
        files = dict(package_files(directory))
        release_tag = args.tag or f"v{version}"
        zip_archive = args.output / f"{name}-{version}.zip"
        vpm_manifest = dict(manifest)
        vpm_manifest.pop("zipSHA256", None)
        vpm_manifest["url"] = f"https://github.com/{args.repository}/releases/download/{quote(release_tag, safe='')}/{zip_archive.name}"
        files["package.json"] = json_bytes(vpm_manifest)
        write_zip(zip_archive, files)
        print(f"Built {zip_archive}")
        # Older release tags may still contain obsolete distribution metadata.
        for field in ("url", "vpmDependencies", "zipSHA256"):
            manifest.pop(field, None)
        files["package.json"] = json_bytes(manifest)
        unity_archive = args.output / f"{name}-{version}.unitypackage"
        write_unitypackage(unity_archive, files, UNITY_INSTALL_PATHS[name])
        print(f"Built {unity_archive}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project", type=Path, default=PROJECT)
    parser.add_argument("--output", type=Path, default=PROJECT / "Build" / "Packages")
    parser.add_argument("--tag")
    parser.add_argument("--repository", default=DEFAULT_REPOSITORY)
    build(parser.parse_args())


if __name__ == "__main__":
    main()
