"""Render an isolated validate-only Cloud Run v1 Job; never call cloud APIs."""
import argparse
import copy
import json
import re
from pathlib import Path


def render_job(seed_config, *, image, bucket, source_sha256, secret_version, report_id):
    require_pattern(image, r"[^\s,]+/host-seed(?::[a-fA-F0-9]{7,64}|@sha256:[a-fA-F0-9]{64})", "image")
    require_pattern(bucket, r"[a-z0-9][a-z0-9._-]{1,220}[a-z0-9]", "bucket")
    require_pattern(source_sha256, r"[a-fA-F0-9]{64}", "source hash")
    require_pattern(secret_version, r"[1-9][0-9]*", "secret version")
    require_pattern(report_id, r"[a-fA-F0-9]{7,64}", "report id")
    if seed_config.get("kind") != "Job" or seed_config.get("apiVersion") != "run.googleapis.com/v1":
        raise ValueError("Expected a Cloud Run v1 Job configuration.")
    source_sha256 = source_sha256.lower()
    spec = copy.deepcopy(seed_config["spec"])
    execution = spec["template"]["spec"]
    task = execution["template"]["spec"]
    containers = task.get("containers")
    if not isinstance(containers, list) or len(containers) != 1:
        raise ValueError("Exactly one seed container is required.")
    container = containers[0]
    default_connections = [item for item in container.get("env", []) if item.get("name") == "ConnectionStrings__Default"]
    if len(default_connections) != 1 or "value" in default_connections[0]:
        raise ValueError("Exactly one secret-backed default connection is required.")
    secret = default_connections[0].get("valueFrom", {}).get("secretKeyRef", {})
    require_pattern(secret.get("name"), r"[A-Za-z0-9_/-]+", "connection secret name")
    require_pattern(secret.get("key"), r"(?:latest|[1-9][0-9]*)", "connection secret key")
    secret = {"name": secret["name"], "key": secret["key"]}
    container["image"] = image
    container["command"] = ["dotnet"]
    container.setdefault("resources", {})["limits"] = {"cpu": "1000m", "memory": "1Gi"}
    container["args"] = ["Host.Seed.dll", f"--catalog-import=/import-source/catalog-import/{source_sha256}/source.json",
                         "--mode=validate", f"--report=/import-output/{report_id.lower()}-{{execution}}.json"]
    container["env"] = [
        {"name": "ConnectionStrings__Default", "valueFrom": {"secretKeyRef": secret}},
        {"name": "CatalogImport__ApprovedConnection", "valueFrom": {"secretKeyRef": {"name": secret["name"], "key": secret_version}}},
        {"name": "CatalogImport__SourceSha256", "value": source_sha256},
        {"name": "Crm__Delivery__Enabled", "value": "false"},
    ]
    container.pop("envFrom", None)
    volumes = task.get("volumes", [])
    reserved = {"catalog-import-source", "catalog-import-output"}
    if any(volume.get("name") in reserved for volume in volumes):
        raise ValueError("Reserved import volume name collides with the seed configuration.")
    # Retain SQL connectivity, never unrelated secret mounts from the seed job.
    sql_volumes = [volume for volume in volumes if "cloudSqlInstance" in volume or
                   (volume.get("name") == "cloudsql" and "secret" not in volume)]
    sql_names = {volume["name"] for volume in sql_volumes}
    container["volumeMounts"] = [mount for mount in container.get("volumeMounts", []) if mount.get("name") in sql_names]
    container["volumeMounts"] += [{"name": "catalog-import-source", "mountPath": "/import-source"},
                                  {"name": "catalog-import-output", "mountPath": "/import-output"}]
    task["volumes"] = sql_volumes + [
        {"name": "catalog-import-source", "csi": {"driver": "gcsfuse.run.googleapis.com", "readOnly": True,
                                                  "volumeAttributes": {"bucketName": bucket}}},
        {"name": "catalog-import-output", "csi": {"driver": "gcsfuse.run.googleapis.com", "readOnly": False,
                                                  "volumeAttributes": {"bucketName": bucket, "mountOptions": f"only-dir=catalog-import/{source_sha256}/reports"}}},
    ]
    execution["taskCount"] = 1
    execution["parallelism"] = 1
    task["maxRetries"] = 0
    task["timeoutSeconds"] = 3600
    spec["template"].setdefault("metadata", {}).setdefault("annotations", {})["run.googleapis.com/execution-environment"] = "gen2"
    keep_connection_secret_alias(spec, secret["name"])
    metadata = {"name": "brevierp-catalog-import", "labels": {"managed-by": "brevi-catalog-import"}}
    namespace = seed_config.get("metadata", {}).get("namespace")
    if namespace is not None:
        require_pattern(str(namespace), r"[A-Za-z0-9_-]+", "namespace")
        metadata["namespace"] = str(namespace)
    return {"apiVersion": "run.googleapis.com/v1", "kind": "Job", "metadata": metadata, "spec": spec}


def require_pattern(value, pattern, label):
    if not isinstance(value, str) or re.fullmatch(pattern, value) is None:
        raise ValueError("Invalid " + label + ".")


def keep_connection_secret_alias(node, secret_name):
    """Keep only the connection alias, if v1 exports map aliases to full secret resources."""
    if isinstance(node, dict):
        annotations = node.get("annotations")
        if isinstance(annotations, dict) and "run.googleapis.com/secrets" in annotations:
            aliases = annotations["run.googleapis.com/secrets"]
            if not isinstance(aliases, str):
                raise ValueError("Invalid secret alias annotation.")
            kept = [alias for alias in aliases.split(",") if alias.partition(":")[0].strip() == secret_name]
            if kept:
                annotations["run.googleapis.com/secrets"] = ",".join(kept)
            else:
                annotations.pop("run.googleapis.com/secrets")
        for child in node.values():
            keep_connection_secret_alias(child, secret_name)
    elif isinstance(node, list):
        for child in node:
            keep_connection_secret_alias(child, secret_name)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("seed-config", "image", "bucket", "source-sha256", "secret-version", "report-id", "output"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args()
    try:
        if Path(args.seed_config).resolve() == Path(args.output).resolve():
            raise ValueError("Output must differ from the seed configuration.")
        config = json.loads(Path(args.seed_config).read_text(encoding="utf-8-sig"))
        job = render_job(config, image=args.image, bucket=args.bucket, source_sha256=args.source_sha256,
                         secret_version=args.secret_version, report_id=args.report_id)
        Path(args.output).write_text(json.dumps(job, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    except (ValueError, KeyError, TypeError, OSError):
        parser.exit(2, "Cannot render catalogue import job: invalid configuration or output path.\n")


if __name__ == "__main__":
    main()
