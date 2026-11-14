import copy
import importlib.util
from pathlib import Path
import unittest

MODULE = Path(__file__).resolve().parents[1] / "render-catalog-import-job.py"
SPEC = importlib.util.spec_from_file_location("catalog_job_renderer", MODULE)
renderer = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(renderer)


def seed():
    return {
        "apiVersion": "run.googleapis.com/v1", "kind": "Job",
        "metadata": {"name": "brevierp-seed", "namespace": "123456", "resourceVersion": "discard"},
        "status": {"must": "discard"},
        "spec": {"template": {
            "metadata": {"annotations": {"run.googleapis.com/vpc-access-connector": "network", "run.googleapis.com/cloudsql-instances": "project:region:instance"}},
            "spec": {"taskCount": 4, "parallelism": 4, "template": {"spec": {
                "serviceAccountName": "seed@example.iam.gserviceaccount.com", "maxRetries": 3, "timeoutSeconds": 600,
                "containers": [{"name": "worker", "image": "old", "command": ["old"], "args": ["--normal-seed"],
                    "env": [{"name": "ConnectionStrings__Default", "valueFrom": {"secretKeyRef": {"name": "db-connection", "key": "latest"}}},
                            {"name": "ADMIN_DEFAULT_PASSWORD", "value": "SYNTHETIC-DO-NOT-COPY"},
                            {"name": "Reminders__Telegram__BotToken", "valueFrom": {"secretKeyRef": {"name": "bot", "key": "latest"}}}],
                    "envFrom": [{"secretRef": {"name": "other"}}],
                    "volumeMounts": [{"name": "cloudsql", "mountPath": "/cloudsql"}, {"name": "admin-secret", "mountPath": "/admin"}],
                    "resources": {"limits": {"cpu": "1", "memory": "1Gi"}}}],
                "volumes": [{"name": "cloudsql", "cloudSqlInstance": {"instances": ["project:region:instance"]}},
                            {"name": "admin-secret", "secret": {"secretName": "admin"}}]
            }}}
        }}
    }


class RenderJobTests(unittest.TestCase):
    def setUp(self):
        self.seed = seed()
        self.args = dict(image="europe-central2-docker.pkg.dev/project/api/host-seed:" + "b" * 40,
                         bucket="private-import-bucket", source_sha256="a" * 64,
                         secret_version="12", report_id="c" * 40)

    def render(self):
        return renderer.render_job(self.seed, **self.args)

    def test_isolated_validate_job_preserves_network_and_identity(self):
        before = copy.deepcopy(self.seed)
        job = self.render()
        self.assertEqual("brevierp-catalog-import", job["metadata"]["name"])
        self.assertEqual("brevi-catalog-import", job["metadata"]["labels"]["managed-by"])
        self.assertNotIn("status", job)
        self.assertNotIn("resourceVersion", job["metadata"])
        execution = job["spec"]["template"]["spec"]
        task = execution["template"]["spec"]
        self.assertEqual((1, 1, 0, 3600), (execution["taskCount"], execution["parallelism"], task["maxRetries"], task["timeoutSeconds"]))
        self.assertEqual(before["spec"]["template"]["metadata"]["annotations"]["run.googleapis.com/vpc-access-connector"], job["spec"]["template"]["metadata"]["annotations"]["run.googleapis.com/vpc-access-connector"])
        self.assertEqual("seed@example.iam.gserviceaccount.com", task["serviceAccountName"])
        container = task["containers"][0]
        self.assertEqual(self.args["image"], container["image"])
        self.assertEqual(["dotnet"], container["command"])
        self.assertEqual("Host.Seed.dll", container["args"][0])
        self.assertIn("--mode=validate", container["args"])
        self.assertTrue(any(a.startswith("--catalog-import=/import-source/catalog-import/") for a in container["args"]))
        self.assertEqual(before, self.seed)

    def test_import_has_dedicated_memory_even_when_seed_is_smaller(self):
        source = self.seed["spec"]["template"]["spec"]["template"]["spec"]["containers"][0]
        source["resources"]["limits"]["memory"] = "512Mi"
        rendered = self.render()["spec"]["template"]["spec"]["template"]["spec"]["containers"][0]
        self.assertEqual({"cpu": "1000m", "memory": "1Gi"}, rendered["resources"]["limits"])

    def test_only_connection_secret_and_safe_environment_are_copied(self):
        task = self.render()["spec"]["template"]["spec"]["template"]["spec"]
        container = task["containers"][0]
        env = {e["name"]: e for e in container["env"]}
        self.assertEqual({"ConnectionStrings__Default", "CatalogImport__ApprovedConnection", "CatalogImport__SourceSha256", "Crm__Delivery__Enabled"}, set(env))
        self.assertEqual({"name": "db-connection", "key": "12"}, env["CatalogImport__ApprovedConnection"]["valueFrom"]["secretKeyRef"])
        self.assertEqual("latest", env["ConnectionStrings__Default"]["valueFrom"]["secretKeyRef"]["key"])
        self.assertEqual("false", env["Crm__Delivery__Enabled"]["value"])
        self.assertNotIn("envFrom", container)
        self.assertNotIn("SYNTHETIC-DO-NOT-COPY", json_text(self.render()))

    def test_source_readonly_report_prefix_and_sql_volumes(self):
        task = self.render()["spec"]["template"]["spec"]["template"]["spec"]
        volumes = {v["name"]: v for v in task["volumes"]}
        self.assertIn("cloudsql", volumes)
        self.assertNotIn("admin-secret", volumes)
        self.assertTrue(volumes["catalog-import-source"]["csi"]["readOnly"])
        output = volumes["catalog-import-output"]["csi"]
        self.assertFalse(output["readOnly"])
        self.assertEqual("only-dir=catalog-import/" + "a" * 64 + "/reports", output["volumeAttributes"]["mountOptions"])

    def test_missing_duplicate_or_plain_connection_is_rejected(self):
        task = self.seed["spec"]["template"]["spec"]["template"]["spec"]
        original = copy.deepcopy(task["containers"][0]["env"])
        for env in ([], original + [original[0]], [{"name": "ConnectionStrings__Default", "value": "SYNTHETIC"}]):
            with self.subTest(env_shape=len(env)):
                task["containers"][0]["env"] = env
                with self.assertRaises(ValueError):
                    self.render()

    def test_multiple_containers_are_rejected(self):
        task = self.seed["spec"]["template"]["spec"]["template"]["spec"]
        task["containers"].append(copy.deepcopy(task["containers"][0]))
        with self.assertRaises(ValueError):
            self.render()

    def test_reserved_volume_collision_is_rejected(self):
        self.seed["spec"]["template"]["spec"]["template"]["spec"]["volumes"].append({"name": "catalog-import-source", "emptyDir": {}})
        with self.assertRaises(ValueError):
            self.render()

    def test_invalid_frozen_inputs_are_rejected(self):
        for key, value in (("source_sha256", "bad"), ("secret_version", "latest"), ("report_id", "../unsafe"), ("bucket", "public bucket"), ("image", "image:latest")):
            with self.subTest(key=key):
                args = {**self.args, key: value}
                with self.assertRaises(ValueError):
                    renderer.render_job(self.seed, **args)


def json_text(value):
    import json
    return json.dumps(value)


if __name__ == "__main__":
    unittest.main()
