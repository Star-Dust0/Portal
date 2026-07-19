#!/usr/bin/env python3
"""Test the Microsoft Translator endpoint used by YMCL.Avalonia.

Usage:
    python test_ymcl_translate.py "Hello, Minecraft!" zh-Hans
"""

import json
import sys
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from urllib.request import Request, urlopen


AUTH_URL = "https://edge.microsoft.com/translate/auth"
TRANSLATE_URL = "https://api.cognitive.microsofttranslator.com/translate"


def get_token() -> str:
    request = Request(
        AUTH_URL,
        headers={
            "User-Agent": "Apifox/1.0.0 (https://apifox.com)",
            "Accept": "*/*",
        },
    )
    with urlopen(request, timeout=15) as response:
        token = response.read().decode("utf-8").strip()

    if not token:
        raise RuntimeError("The authentication endpoint returned an empty token.")
    return token


def translate(text: str, target_language: str) -> str:
    token = get_token()
    query = urlencode({"api-version": "3.0", "to": target_language, "textType": "plain"})
    body = json.dumps([{"Text": text}], ensure_ascii=False).encode("utf-8")
    request = Request(
        f"{TRANSLATE_URL}?{query}",
        data=body,
        method="POST",
        headers={
            "Authorization": token,
            "Content-Type": "application/json; charset=utf-8",
            "Accept": "application/json",
        },
    )
    with urlopen(request, timeout=15) as response:
        payload = json.loads(response.read().decode("utf-8"))

    return payload[0]["translations"][0]["text"]


def main() -> int:
    text = sys.argv[1] if len(sys.argv) > 1 else "Hello, Minecraft!"
    target_language = sys.argv[2] if len(sys.argv) > 2 else "zh-Hans"

    try:
        print(translate(text, target_language))
    except HTTPError as error:
        detail = error.read().decode("utf-8", errors="replace")
        print(f"HTTP {error.code}: {detail}", file=sys.stderr)
        return 1
    except (URLError, TimeoutError, RuntimeError, KeyError, IndexError, json.JSONDecodeError) as error:
        print(f"Translation request failed: {error}", file=sys.stderr)
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
