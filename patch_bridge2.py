with open('/root/openclaw_bridge.py', 'r') as f:
    content = f.read()

autonofw_method = '''
    def _run_autonofw(self, task: str) -> str:
        """POST task to AutonoFramework2026 at localhost:3030"""
        import json as _json
        url = "http://localhost:3030/task"
        payload = _json.dumps({"task": task}).encode("utf-8")
        req = urllib.request.Request(
            url,
            data=payload,
            headers={"Content-Type": "application/json"},
            method="POST"
        )
        try:
            with urllib.request.urlopen(req, timeout=120) as resp:
                body = resp.read().decode("utf-8")
                data = _json.loads(body)
                return data.get("result", body)[:4000]
        except Exception as e:
            return f"AutonoFramework error: {e}"

    def do_GET(self):'''

# Replace do_GET with the method + do_GET
content = content.replace('    def do_GET(self):', autonofw_method)

with open('/root/openclaw_bridge.py', 'w') as f:
    f.write(content)

print('Method added')
# Verify
with open('/root/openclaw_bridge.py', 'r') as f:
    c = f.read()
print('_run_autonofw def present:', 'def _run_autonofw' in c)
