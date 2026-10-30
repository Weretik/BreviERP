"""Local validation of OAuth inputs, grants and callback state; no IO."""
import json
import secrets
import urllib.parse

SCOPES = {"https://www.googleapis.com/auth/gmail.send", "https://www.googleapis.com/auth/gmail.readonly"}

class SetupError(Exception):
    """Bounded public error code only; never include a provider response or secret."""

def mailbox(value):
    if not isinstance(value, str) or not 3 <= len(value) <= 254 or not value.isascii() or any(c.isspace() or ord(c) < 32 for c in value):
        raise SetupError("invalid_mailbox")
    if value.count("@") != 1 or any(c in value for c in '<>,;:"()\\'):
        raise SetupError("invalid_mailbox")
    local, domain = value.split("@")
    if not local or "." not in domain or domain.startswith(".") or domain.endswith("."):
        raise SetupError("invalid_mailbox")
    return value


def desktop_client(path):
    if path.stat().st_size > 32768:
        raise SetupError("client_file_invalid")
    data = json.loads(path.read_text(encoding="utf-8-sig"))
    client = data.get("installed") if isinstance(data, dict) else None
    if not isinstance(client, dict):
        raise SetupError("desktop_client_required")
    for field in ("client_id", "client_secret"):
        value = client.get(field)
        if not isinstance(value, str) or not value or len(value) > 4096 or any(c.isspace() or ord(c) < 32 for c in value):
            raise SetupError("client_file_invalid")
    return client

def validated_grant(grant):
    if not isinstance(grant, dict) or str(grant.get("token_type", "")).lower() != "bearer":
        raise SetupError("oauth_grant_invalid")
    for field in ("access_token", "refresh_token"):
        token = grant.get(field)
        if not isinstance(token, str) or not token or len(token) > (8192 if field == "access_token" else 4096) or any(c.isspace() or ord(c) < 32 for c in token):
            raise SetupError("offline_grant_required")
    scope = grant.get("scope")
    if not isinstance(scope, str) or not SCOPES.issubset(set(scope.split())):
        raise SetupError("required_scopes_not_granted")
    return grant["access_token"], grant["refresh_token"]


def callback_code(path, state):
    parsed = urllib.parse.urlsplit(path)
    if parsed.scheme or parsed.netloc or parsed.path != "/callback" or len(path) > 8192:
        raise SetupError("callback_invalid")
    params = urllib.parse.parse_qs(parsed.query, max_num_fields=12)
    states = params.get("state", [])
    if len(states) != 1 or not secrets.compare_digest(states[0].encode("utf-8"), state.encode("utf-8")):
        raise SetupError("callback_state_invalid")
    if "error" in params:
        raise SetupError("consent_declined")
    codes = params.get("code", [])
    if len(codes) != 1 or not codes[0] or len(codes[0]) > 4096:
        raise SetupError("callback_code_invalid")
    return codes[0]
