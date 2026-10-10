"""Check archive compatibility, package separation, and release version preservation."""

import argparse
from contextlib import redirect_stdout
import io
import json
from pathlib import Path
import tarfile
import tempfile
import unittest
import zipfile

import build_packages


class PackageBuilderTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.project = Path(self.temporary.name)
        self.output = self.project / "output"
        packages = (*build_packages.VPM_PACKAGES, build_packages.DEVELOPMENT_PACKAGE)
        for number, name in enumerate(packages, start=1):
            directory = self.project / "Packages" / name
            directory.mkdir(parents=True)
            contents = {
                "package.json": json.dumps({"name": name, "version": "0.1.0", "license": "MIT"}),
                "LICENSE.md": "MIT license fixture",
                "Runtime/Test.cs": f"// Package {number}\n",
            }
            for index, (relative, content) in enumerate(contents.items(), start=1):
                path = directory / relative
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text(content, encoding="utf-8")
                Path(str(path) + ".meta").write_text(
                    f"fileFormatVersion: 2\nguid: {number * 100 + index:032x}\n", encoding="utf-8"
                )
            (directory / "Runtime.meta").write_text(
                f"fileFormatVersion: 2\nguid: {number * 100:032x}\nfolderAsset: yes\n",
                encoding="utf-8",
            )

    def build(self, tag="v0.1.0", previous_index=None):
        args = argparse.Namespace(
            repository=build_packages.DEFAULT_REPOSITORY, tag=tag, project=self.project,
            output=self.output, previous_index=previous_index, previous_index_url=None,
        )
        with redirect_stdout(io.StringIO()):
            build_packages.build(args)

    def test_round_trip_preserves_assets_metadata_and_package_separation(self):
        self.build()
        self.assertEqual(len(list(self.output.glob("*.zip"))), 2)
        self.assertEqual(len(list(self.output.glob("*.unitypackage"))), 2)
        self.assertFalse(any(build_packages.DEVELOPMENT_PACKAGE in p.name for p in self.output.iterdir()))
        for name in build_packages.VPM_PACKAGES:
            install_path = build_packages.UNITY_INSTALL_PATHS[name]
            with zipfile.ZipFile(self.output / f"{name}-0.1.0.zip") as vpm:
                files = {relative: vpm.read(relative) for relative in vpm.namelist()}
            restored = {}
            with tarfile.open(self.output / f"{name}-0.1.0.unitypackage", "r:gz") as unity:
                for member in unity.getmembers():
                    if not member.name.endswith("/pathname"):
                        continue
                    guid = member.name.split("/")[0]
                    pathname = unity.extractfile(member).read().decode("utf-8")
                    self.assertTrue(pathname == install_path or pathname.startswith(install_path + "/"))
                    meta = unity.extractfile(f"{guid}/asset.meta").read()
                    self.assertIn(f"guid: {guid}".encode("ascii"), meta)
                    if pathname == install_path:
                        continue
                    relative = pathname[len(install_path) + 1:]
                    restored[relative + ".meta"] = meta
                    if f"{guid}/asset" in unity.getnames():
                        restored[relative] = unity.extractfile(f"{guid}/asset").read()
                    else:
                        self.assertIn(b"folderAsset: yes", meta)
            self.assertEqual(restored, files)

    def test_rebuild_is_deterministic_and_keeps_published_urls(self):
        self.build()
        original = {p.name: p.read_bytes() for p in self.output.iterdir()}
        previous = self.project / "previous-index.json"
        previous.write_bytes(original["index.json"])
        self.build(tag="v0.1.1", previous_index=previous)
        self.assertEqual(original, {p.name: p.read_bytes() for p in self.output.iterdir()})

    def test_modified_published_version_is_rejected(self):
        self.build()
        previous = self.project / "previous-index.json"
        previous.write_bytes((self.output / "index.json").read_bytes())
        directory = self.project / "Packages" / build_packages.VPM_PACKAGES[0]
        (directory / "Runtime/Test.cs").write_text("// Changed\n", encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "Published version is immutable"):
            self.build(previous_index=previous)

    def test_invalid_missing_or_duplicate_metadata_is_rejected(self):
        for files, message in (
            ({"Test.shader": b"shader"}, "Missing .meta"),
            ({"Test.meta": b"guid: invalid\n"}, "Invalid GUID"),
            ({"Test.meta": b"guid: " + b"1" * 32 + b"\n"}, "Orphan .meta"),
            ({"A": b"a", "B": b"b", "A.meta": b"guid: " + b"1" * 32 + b"\n",
              "B.meta": b"guid: " + b"1" * 32 + b"\n"}, "Duplicate GUID"),
        ):
            with self.subTest(message=message):
                with self.assertRaisesRegex(ValueError, message):
                    build_packages.write_unitypackage(self.project / "test.unitypackage", files, "Assets/Test")


if __name__ == "__main__":
    unittest.main()
