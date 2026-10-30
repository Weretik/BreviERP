"""User-run local Desktop OAuth setup. Never sends mail or prints credentials."""
import argparse
import json
from pathlib import Path
import subprocess
import sys
from gmail_oauth.validation import SetupError, SCOPES, mailbox, desktop_client, validated_grant, callback_code
from gmail_oauth.transport import TOKEN_URL, PROFILE_URL, json_request
from gmail_oauth.consent import receive_code


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--client-json", type=Path, required=True, help="Downloaded Google Desktop client JSON; never pass secret values.")
    parser.add_argument("--sender", help="Expected Gmail account, must equal Google profile.")
    parser.add_argument("--test-recipient", help="Only approved sandbox recipient.")
    args = parser.parse_args()
    repo = Path(__file__).resolve().parents[1]
    local_path = repo / ".local/p6/gmail-settings.local.json"
    local = json.loads(local_path.read_text(encoding="utf-8-sig")).get("Crm", {}).get("Delivery", {}) if local_path.exists() else {}
    sender = mailbox(args.sender or local.get("SenderAddress") or input("Sender Gmail address: "))
    recipient = mailbox(args.test_recipient or local.get("TestRecipientAddress") or input("Approved test recipient address: "))
    client = desktop_client(args.client_json)
    code, verifier, redirect = receive_code(client["client_id"])
    grant = json_request(TOKEN_URL, {"client_id": client["client_id"], "client_secret": client["client_secret"],
        "grant_type": "authorization_code", "code": code, "redirect_uri": redirect, "code_verifier": verifier})
    access, refresh = validated_grant(grant)
    profile = json_request(PROFILE_URL, access_token=access)
    if not isinstance(profile, dict) or str(profile.get("emailAddress", "")).lower() != sender.lower():
        raise SetupError("sender_account_mismatch")
    settings = {"Crm:Delivery:Enabled": False, "Crm:Delivery:SandboxOnly": True,
        "Crm:Delivery:ProviderKey": "gmail", "Crm:Delivery:SenderAddress": sender,
        "Crm:Delivery:TestRecipientAddress": recipient, "Crm:Delivery:Gmail:OAuth:ClientId": client["client_id"],
        "Crm:Delivery:Gmail:OAuth:ClientSecret": client["client_secret"], "Crm:Delivery:Gmail:OAuth:RefreshToken": refresh}
    # JSON stdin avoids secrets in argv/shell history. Secret Manager merges these keys into the existing store.
    saved = subprocess.run(["dotnet", "user-secrets", "set", "--project", str(repo / "src/Bootstrapper/Host.Api/Host.Api.csproj")],
        input=json.dumps(settings), text=True, encoding="utf-8", capture_output=True, timeout=30)
    if saved.returncode != 0:
        raise SetupError("user_secrets_write_failed")
    print("Verified account and saved OAuth settings to Host.Api development user-secrets. Sending remains disabled; sandbox remains enabled.")


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("Setup cancelled; no mail sent.", file=sys.stderr)
        sys.exit(1)
    except Exception as error:
        # Never display exception text: provider URLs/responses, callback codes or credential paths may be sensitive.
        code = str(error) if isinstance(error, SetupError) else "setup_failed"
        print("OAuth setup stopped: " + code + ". No mail sent.", file=sys.stderr)
        sys.exit(1)
