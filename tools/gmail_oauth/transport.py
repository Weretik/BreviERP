"""Fixed Google endpoints and bounded JSON transport; redirects are prohibited."""
import json
import urllib.parse
import urllib.request
from .validation import SetupError

TOKEN_URL = "https://oauth2.googleapis.com/token"
PROFILE_URL = "https://gmail.googleapis.com/gmail/v1/users/me/profile"

class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, request, fp, code, message, headers, new_url):
        return None

def json_request(url, fields=None, access_token=None):
    # Endpoints are fixed constants. Never follow a redirect carrying the refresh/client secret form.
    if url not in (TOKEN_URL, PROFILE_URL):
        raise SetupError("endpoint_invalid")
    headers = {"Accept": "application/json"}
    if access_token:
        headers["Authorization"] = "Bearer " + access_token
    body = urllib.parse.urlencode(fields).encode("utf-8") if fields is not None else None
    request = urllib.request.Request(url, data=body, headers=headers)
    with urllib.request.build_opener(NoRedirect()).open(request, timeout=20) as response:
        payload = response.read(16385)
        if len(payload) > 16384:
            raise SetupError("response_too_large")
    return json.loads(payload)
