"""Tests of the CI test selection: python3 -m unittest discover -s tools/ci"""
import unittest

from affected_tests import affected, referenced_by

USERS = {"Crm": {"Production"}, "Production": {"Crm", "Warehouse"}, "Warehouse": {"Production"},
         "Catalog": set(), "Reference": {"Catalog"}, "Media": {"Catalog"}}


class AffectedTests(unittest.TestCase):
    def test_documentation_runs_no_module_tests(self):
        self.assertEqual(affected(["docs/sdd/x.md", "README.md"], USERS), "none")

    def test_a_module_runs_its_tests_and_the_modules_that_reference_it(self):
        self.assertEqual(affected(["src/Modules/Warehouse/Warehouse.Domain/X.cs"], USERS),
                         "FullyQualifiedName~UnitTests.Production.|FullyQualifiedName~IntegrationTests.Production."
                         "|FullyQualifiedName~UnitTests.Warehouse.|FullyQualifiedName~IntegrationTests.Warehouse.")

    def test_a_module_test_file_counts_as_its_module(self):
        self.assertIn("UnitTests.Crm.", affected(["tests/UnitTests/Crm/QuoteTests.cs"], USERS))

    def test_shared_code_build_files_and_test_support_run_everything(self):
        for path in ["src/BuildingBlocks/X.cs", "src/Bootstrapper/Host.Api/Program.cs", "Directory.Packages.props",
                     "BreviERP.sln", ".github/workflows/deploy-cloudrun.yml", "tests/UnitTests/TestSupport/X.cs",
                     "tests/UnitTests/RootTests.cs", "tools/ci/affected_tests.py"]:
            self.assertEqual(affected([path], USERS), "all", path)

    def test_modules_with_ungrouped_tests_run_everything(self):
        self.assertEqual(affected(["src/Modules/Media/Media.Domain/X.cs"], USERS), "all")

    def test_architecture_tests_alone_need_no_module_tests(self):
        self.assertEqual(affected(["tests/ArchitectureTests/X.cs"], USERS), "none")

    def test_the_real_reference_graph_is_read_from_project_files(self):
        users = referenced_by()
        self.assertIn("Warehouse", users["Production"])
        self.assertIn("Crm", users["Catalog"])


if __name__ == "__main__":
    unittest.main()
