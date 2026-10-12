"""Check archive compatibility, package separation, and obsolete asset cleanup."""

import argparse
from contextlib import redirect_stdout
import io
import json
from pathlib import Path
import tarfile
import tempfile
import unittest

import build_packages


class PackageBuilderTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.project = Path(self.temporary.name)
        self.output = self.project / "output"
        packages = (*build_packages.PUBLIC_PACKAGES, build_packages.DEVELOPMENT_PACKAGE)
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

    def build(self):
        args = argparse.Namespace(project=self.project, output=self.output)
        with redirect_stdout(io.StringIO()):
            build_packages.build(args)

    def test_round_trip_preserves_assets_metadata_and_package_separation(self):
        self.build()
        self.assertEqual(len(list(self.output.glob("*.zip"))), 0)
        self.assertFalse((self.output / "index.json").exists())
        self.assertEqual(len(list(self.output.glob("*.unitypackage"))), 2)
        self.assertFalse(any(build_packages.DEVELOPMENT_PACKAGE in p.name for p in self.output.iterdir()))
        for name in build_packages.PUBLIC_PACKAGES:
            install_path = build_packages.UNITY_INSTALL_PATHS[name]
            files = dict(build_packages.package_files(self.project / "Packages" / name))
            files["package.json"] = build_packages.json_bytes(json.loads(files["package.json"]))
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

    def test_rebuild_is_deterministic(self):
        self.build()
        original = {p.name: p.read_bytes() for p in self.output.iterdir()}
        self.build()
        self.assertEqual(original, {p.name: p.read_bytes() for p in self.output.iterdir()})

    def test_obsolete_outputs_are_removed_and_other_files_are_preserved(self):
        self.output.mkdir()
        stale = ["index.json", f"{build_packages.DEVELOPMENT_PACKAGE}-0.1.0.tgz"]
        stale.extend(f"{name}-0.1.0.zip" for name in build_packages.PUBLIC_PACKAGES)
        for name in [*stale, "other-package.zip"]:
            (self.output / name).write_bytes(b"old archive")
        self.build()
        for name in stale:
            self.assertFalse((self.output / name).exists(), name)
        self.assertEqual((self.output / "other-package.zip").read_bytes(), b"old archive")

    def test_old_tag_metadata_is_removed_from_unitypackages(self):
        for name in build_packages.PUBLIC_PACKAGES:
            path = self.project / "Packages" / name / "package.json"
            manifest = json.loads(path.read_text(encoding="utf-8"))
            manifest.update(url="https://example.com/old.zip", vpmDependencies={"old.package": "1.x"}, zipSHA256="old")
            path.write_text(json.dumps(manifest), encoding="utf-8")
        self.build()
        for archive in self.output.glob("*.unitypackage"):
            with tarfile.open(archive, "r:gz") as unity:
                for member in unity.getmembers():
                    if member.name.endswith("/pathname") and unity.extractfile(member).read().endswith(b"/package.json"):
                        guid = member.name.split("/")[0]
                        manifest = json.load(unity.extractfile(f"{guid}/asset"))
                        for field in ("url", "vpmDependencies", "zipSHA256"):
                            self.assertNotIn(field, manifest)

    def test_invalid_version_cannot_escape_output_directory(self):
        path = self.project / "Packages" / build_packages.PUBLIC_PACKAGES[0] / "package.json"
        manifest = json.loads(path.read_text(encoding="utf-8"))
        manifest["version"] = "../../escape"
        path.write_text(json.dumps(manifest), encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "Invalid package version"):
            self.build()

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
