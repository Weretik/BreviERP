"""Explicit local browser consent with a temporary loopback receiver."""
import base64
import hashlib
import http.server
import secrets
import time
import urllib.parse
import webbrowser
from .validation import SetupError, SCOPES, callback_code

def receive_code(client_id):
    state = secrets.token_urlsafe(32)
    verifier = secrets.token_urlsafe(64)
    challenge = base64.urlsafe_b64encode(hashlib.sha256(verifier.encode("ascii")).digest()).rstrip(b"=").decode("ascii")
    result = {}

    class Callback(http.server.BaseHTTPRequestHandler):
        def log_message(self, format, *args):
            pass  # Default logging would expose the authorization code in the request URL.

        def do_GET(self):
            self.connection.settimeout(3)
            try:
                if self.headers.get("Host") != "127.0.0.1:" + str(self.server.server_port) or self.headers.get("Content-Length", "0") != "0" or self.headers.get("Transfer-Encoding"):
                    raise SetupError("callback_invalid")
                result["code"] = callback_code(self.path, state)
                body = b"Google consent received. You may close this tab and return to the terminal."
                self.send_response(200)
            except SetupError as error:
                # Only a state-verified denial is terminal; a forged callback cannot abort consent.
                if str(error) == "consent_declined":
                    result["error"] = "consent_declined"
                body = b"Consent was declined or callback was invalid. Return to the terminal."
                self.send_response(400)
            except ValueError:
                body = b"Invalid callback. Return to the consent flow."
                self.send_response(400)
            self.send_header("Content-Type", "text/plain; charset=utf-8")
            self.send_header("Cache-Control", "no-store")
            self.send_header("Content-Length", str(len(body)))
            self.end_headers()
            self.wfile.write(body)

    class LoopbackServer(http.server.HTTPServer):
        def get_request(self):
            connection, address = super().get_request()
            connection.settimeout(3)
            return connection, address

    with LoopbackServer(("127.0.0.1", 0), Callback) as server:
        server.timeout = 1
        redirect = "http://127.0.0.1:" + str(server.server_port) + "/callback"
        query = urllib.parse.urlencode({"client_id": client_id, "redirect_uri": redirect, "response_type": "code",
            "scope": " ".join(sorted(SCOPES)), "access_type": "offline", "prompt": "consent", "state": state,
            "code_challenge": challenge, "code_challenge_method": "S256"})
        if not webbrowser.open("https://accounts.google.com/o/oauth2/v2/auth?" + query):
            raise SetupError("system_browser_unavailable")
        print("Complete Google consent in your system browser. No mail will be sent.")
        deadline = time.monotonic() + 300
        while not result and time.monotonic() < deadline:
            server.handle_request()
        if "error" in result:
            raise SetupError(result["error"])
        if "code" not in result:
            raise SetupError("consent_timeout")
    return result["code"], verifier, redirect
