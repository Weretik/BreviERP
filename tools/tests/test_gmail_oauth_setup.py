"""Offline tests of security-relevant consent callback/grant validation; no browser or network."""
import importlib.util
from pathlib import Path
import sys
import unittest
from unittest import mock
import io
import urllib.parse

sys.dont_write_bytecode = True
source = Path(__file__).resolve().parents[1] / "gmail-oauth-setup.py"
sys.path.insert(0, str(source.parent))
spec = importlib.util.spec_from_file_location("gmail_oauth_setup", source)
oauth = importlib.util.module_from_spec(spec)
spec.loader.exec_module(oauth)


class GmailOAuthSetupTests(unittest.TestCase):
    def test_callback_accepts_only_expected_state_and_single_code(self):
        self.assertEqual(oauth.callback_code("/callback?state=fixture-state&code=fixture-code", "fixture-state"), "fixture-code")
        with self.assertRaises(oauth.SetupError):
            oauth.callback_code("/callback?state=attacker&code=fixture-code", "fixture-state")

    def test_duplicate_state_or_code_is_rejected(self):
        for query in ("state=s&state=s&code=c", "state=s&code=c&code=d"):
            with self.subTest(query=query), self.assertRaises(oauth.SetupError):
                oauth.callback_code("/callback?" + query, "s")

    def test_arbitrary_url_and_path_are_rejected(self):
        for path in ("https://other.invalid/callback?state=s&code=c", "//other.invalid/callback?state=s&code=c", "/wrong?state=s&code=c"):
            with self.subTest(path=path), self.assertRaises(oauth.SetupError):
                oauth.callback_code(path, "s")

    def test_initial_grant_requires_both_scopes_and_offline_token(self):
        grant = dict(access_token="fixture-access", refresh_token="fixture-refresh", token_type="Bearer", scope=" ".join(oauth.SCOPES))
        self.assertEqual(oauth.validated_grant(grant), ("fixture-access", "fixture-refresh"))
        for patch in ({"scope": "https://www.googleapis.com/auth/gmail.send"}, {"refresh_token": ""}, {"token_type": "Basic"}):
            with self.subTest(patch=patch), self.assertRaises(oauth.SetupError):
                oauth.validated_grant(dict(grant, **patch))

    def test_refresh_token_matches_runtime_credential_bound(self):
        grant = dict(access_token="fixture-access", refresh_token="x" * 4097, token_type="Bearer", scope=" ".join(oauth.SCOPES))
        with self.assertRaises(oauth.SetupError):
            oauth.validated_grant(grant)

    def test_mailbox_rejects_headers_display_names_and_extra_addresses(self):
        self.assertEqual(oauth.mailbox("sandbox@example.test"), "sandbox@example.test")
        for mailbox in ("sandbox@example.test\r\nBcc: other@example.test", "Name <sandbox@example.test>", "one@example.test,two@example.test"):
            with self.subTest(mailbox=mailbox), self.assertRaises(oauth.SetupError):
                oauth.mailbox(mailbox)


class ConsentFlowTests(unittest.TestCase):
    def simulate(self, responses):
        state = {}
        calls = []
        module = sys.modules.get(oauth.receive_code.__module__, oauth)

        class FakeServer:
            server_port = 12345
            def __init__(self, address, handler):
                self.handler = handler
            def __enter__(self):
                return self
            def __exit__(self, *args):
                return False
            def handle_request(self):
                if len(calls) >= len(responses):
                    raise AssertionError("Consent callback did not terminate after the user decision.")
                path = responses[len(calls)](state["value"])
                calls.append(path)
                handler = object.__new__(self.handler)
                handler.server = self
                handler.connection = mock.Mock()
                handler.headers = {"Host": "127.0.0.1:12345"}
                handler.path = path
                handler.wfile = io.BytesIO()
                handler.send_response = mock.Mock()
                handler.send_header = mock.Mock()
                handler.end_headers = mock.Mock()
                handler.do_GET()

        def browser(url):
            state["value"] = urllib.parse.parse_qs(urllib.parse.urlsplit(url).query)["state"][0]
            return True

        with mock.patch.object(module.http.server, "HTTPServer", FakeServer), \
             mock.patch.object(module.webbrowser, "open", side_effect=browser), \
             mock.patch.object(module.time, "monotonic", return_value=0), \
             mock.patch("builtins.print"):
            return oauth.receive_code("fixture-client")

    def test_user_decline_stops_consent_immediately(self):
        with self.assertRaisesRegex(oauth.SetupError, "^consent_declined$"):
            self.simulate([lambda state: "/callback?state=" + state + "&error=access_denied"])

    def test_foreign_state_cannot_abort_consent(self):
        result = self.simulate([
            lambda state: "/callback?state=foreign&error=access_denied",
            lambda state: "/callback?state=" + state + "&code=fixture-code",
        ])
        self.assertEqual(result[0], "fixture-code")


if __name__ == "__main__":
    unittest.main()
