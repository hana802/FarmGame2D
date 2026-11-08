"""Compatibility wrapper for the project's bundled Node Playwright check."""

import os
import subprocess


NODE = r"C:\Users\User\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe"
NODE_MODULES = r"C:\Users\User\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules"
environment = os.environ.copy()
environment["NODE_PATH"] = NODE_MODULES
raise SystemExit(
    subprocess.call(
        [NODE, "Tools/verify-webgl-ui.cjs", "http://127.0.0.1:8766/"],
        env=environment,
    )
)
