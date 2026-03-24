with open('/root/openclaw_bridge.py', 'r') as f:
    content = f.read()

# Add urllib.request import if not present
if 'import urllib.request' not in content:
    content = content.replace(
        'from http.server import HTTPServer, BaseHTTPRequestHandler',
        'from http.server import HTTPServer, BaseHTTPRequestHandler\nimport urllib.request'
    )

# Insert the autonofw routing at start of _run_task and add the new method
# Find the _run_task method and insert autonofw check at the top
autonofw_check = '        # Route to AutonoFramework2026 HTTP endpoint\n        if target == "autonofw":\n            return self._run_autonofw(task)\n\n'
run_task_start = '    def _run_task(self, task: str, target: str) -> str:\n        env = os.environ.copy()'

if autonofw_check not in content:
    content = content.replace(
        run_task_start,
        '    def _run_task(self, task: str, target: str) -> str:\n' + autonofw_check + '        env = os.environ.copy()'
    )

# Add the _run_autonofw method before the do_GET method
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

'''

if '_run_autonofw' not in content:
    content = content.replace('    def do_GET(self):', autonofw_method + '    def do_GET(self):')

with open('/root/openclaw_bridge.py', 'w') as f:
    f.write(content)

print('Bridge patched successfully')
print('autonofw check present:', 'autonofw' in content)
print('_run_autonofw present:', '_run_autonofw' in content)
