#!/usr/bin/env python3
"""Fetch and verify workshop maps used by LevelImposter.CoreTests."""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import shutil
import sys
import tempfile
import urllib.error
import urllib.request
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_MANIFEST = REPO_ROOT / "LevelImposter.CoreTests" / "Workshop" / "manifest.json"
DEFAULT_CACHE = REPO_ROOT / "LevelImposter.CoreTests" / ".cache" / "maps"
API_ROOT = "https://api.levelimposter.net"
USER_AGENT = "LevelImposter-CoreTests/1.0 (workshop map fetcher)"


def read_json(url: str) -> dict:
    request = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
    with urllib.request.urlopen(request, timeout=60) as response:
        return json.load(response)


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def identify_format(path: Path) -> str:
    with path.open("rb") as stream:
        prefix = stream.read(4)
        if prefix == b"PK\x03\x04":
            return "LIM2_ZIP"
        if prefix == b"LIM2":
            return "LIM2_Signature"
        if prefix[:1] == b"{":
            return "Legacy"
        return "LIM2"


def fetch_map_metadata(map_id: str) -> dict:
    response = read_json(f"{API_ROOT}/map/{map_id}")
    data = response.get("data")
    if not isinstance(data, dict):
        raise RuntimeError(f"Map API returned no data for {map_id}: {response.get('error', '')}")
    api_id = data.get("id")
    if not isinstance(api_id, str) or api_id.lower() != map_id.lower():
        raise RuntimeError(f"Map API returned unexpected ID for {map_id}: {api_id!r}")
    if not isinstance(data.get("downloadURL"), str):
        raise RuntimeError(f"Map {map_id} has no download URL")
    return data


def download_file(url: str, destination: Path) -> None:
    request = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
    with urllib.request.urlopen(request, timeout=180) as response, destination.open("wb") as output:
        shutil.copyfileobj(response, output, length=1024 * 1024)


def warn_stale(map_entry: dict, message: str) -> None:
    print(
        f"WARNING baseline-stale for {map_entry['id']}: {message}. "
        "Workshop structural checks will still run; use --refresh-baseline to accept a new baseline.",
        file=sys.stderr,
    )


def update_metadata(map_entry: dict, api_data: dict, path: Path) -> None:
    map_entry["name"] = api_data.get("name", map_entry["name"])
    map_entry["authorName"] = api_data.get("authorName", map_entry["authorName"])
    map_entry["mapVersion"] = api_data.get("v")
    map_entry["format"] = identify_format(path)
    map_entry["sizeBytes"] = path.stat().st_size
    map_entry["sha256"] = sha256_file(path)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--manifest", type=Path, default=DEFAULT_MANIFEST)
    parser.add_argument("--cache", type=Path, default=DEFAULT_CACHE)
    parser.add_argument(
        "--refresh-baseline",
        action="store_true",
        help="Download current map versions and update their metadata, SHA-256 hashes, and format baselines.",
    )
    args = parser.parse_args()

    manifest_path = args.manifest.resolve()
    cache_dir = args.cache.resolve()
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    maps = manifest.get("maps")
    if not isinstance(maps, list) or not maps:
        raise RuntimeError(f"Workshop manifest contains no maps: {manifest_path}")

    cache_dir.mkdir(parents=True, exist_ok=True)
    manifest_changed = False
    failures: list[str] = []

    for map_entry in maps:
        map_id = map_entry["id"]
        path = cache_dir / map_entry["fileName"]
        pinned_sha = map_entry.get("sha256")

        if path.exists() and pinned_sha and not args.refresh_baseline:
            actual_sha = sha256_file(path)
            if pinned_sha and actual_sha != pinned_sha:
                warn_stale(map_entry, f"cached file SHA-256 is {actual_sha}, expected {pinned_sha}")
            else:
                print(f"cached {map_id}: {path.stat().st_size:,} bytes")
            continue

        temporary_path: Path | None = None
        try:
            api_data = fetch_map_metadata(map_id)
            with tempfile.NamedTemporaryFile(
                prefix=f"{map_id}.",
                suffix=".part",
                dir=cache_dir,
                delete=False,
            ) as temporary:
                temporary_path = Path(temporary.name)

            download_file(api_data["downloadURL"], temporary_path)
            actual_sha = sha256_file(temporary_path)
            if pinned_sha and actual_sha != pinned_sha and not args.refresh_baseline:
                warn_stale(map_entry, f"download SHA-256 is {actual_sha}, expected {pinned_sha}")

            if args.refresh_baseline or not pinned_sha:
                update_metadata(map_entry, api_data, temporary_path)
                manifest_changed = True

            os.replace(temporary_path, path)
            temporary_path = None
            print(f"fetched {map_id}: {path.stat().st_size:,} bytes, sha256={actual_sha}")
        except (urllib.error.URLError, TimeoutError, OSError, RuntimeError) as error:
            failures.append(f"{map_id}: {error}")
            print(f"ERROR {map_id}: {error}", file=sys.stderr)
        finally:
            if temporary_path is not None:
                temporary_path.unlink(missing_ok=True)

    if manifest_changed:
        manifest_path.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
        print(f"updated baseline manifest: {manifest_path}")

    if failures:
        return 1
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (json.JSONDecodeError, KeyError, RuntimeError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        raise SystemExit(1)
