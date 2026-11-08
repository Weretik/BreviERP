using FluentValidation.TestHelper;
using Identity.Api.Contracts.Access;
using Identity.Application.Features.Access.CreateRole;
using Identity.Application.Features.Access.DeleteRole;
using Identity.Application.Features.Access.SaveRoleAccess;
using Identity.Application.Security.Access;
using Identity.Domain.Authorization;

namespace UnitTests.Identity;

/// <summary>«Права доступу» (client corrections item 11): pages, defaults, policy names and save rules.</summary>
public sealed class AccessRightsTests
{
    [Test]
    public void Defaults_keep_the_old_built_in_rules()
    {
        Assert.Multiple(() =>
        {
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.Quotes],
                Is.EqualTo(AccessAction.View | AccessAction.Create | AccessAction.Edit));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager), Does.Not.ContainKey(AccessCatalog.Payroll));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.WorkshopChief), Does.Not.ContainKey(AccessCatalog.Quotes));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Accountant)[AccessCatalog.Products], Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin), Does.Not.ContainKey(AccessCatalog.Accounting));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin), Does.Not.ContainKey(AccessCatalog.PurchaseInvoices));
            Assert.That(AccessCatalog.DefaultsOf("custom-new"), Is.Empty);
        });
    }

    [Test]
    public void Every_page_has_view_and_its_defaults_stay_within_its_actions()
    {
        Assert.That(AccessCatalog.Areas.Select(x => x.Key), Is.Unique);
        foreach (var area in AccessCatalog.Areas)
        {
            Assert.That(area.Actions.HasFlag(AccessAction.View), area.Key);
            foreach (var (role, actions) in area.Defaults)
            {
                Assert.That(actions & ~area.Actions, Is.EqualTo(AccessAction.None), $"{area.Key}/{role}");
                Assert.That(actions.HasFlag(AccessAction.View), $"{area.Key}/{role}");
            }
        }
    }

    [Test]
    public void Combined_roles_get_every_action_any_of_them_has()
    {
        var combined = AccessCatalog.Combine([AccessCatalog.DefaultsOf(RoleNames.Manager), AccessCatalog.DefaultsOf(RoleNames.WorkshopChief)]);

        Assert.Multiple(() =>
        {
            Assert.That(combined, Does.ContainKey(AccessCatalog.Quotes));
            Assert.That(combined, Does.ContainKey(AccessCatalog.Payroll));
        });
    }

    [TestCase("GET", null, AccessAction.View)]
    [TestCase("POST", null, AccessAction.Create)]
    [TestCase("PUT", null, AccessAction.Edit)]
    [TestCase("PATCH", null, AccessAction.Edit)]
    [TestCase("DELETE", null, AccessAction.Delete)]
    [TestCase("POST", AccessAction.Edit, AccessAction.Edit)]
    [TestCase("GET", AccessAction.Delete, AccessAction.Delete)]
    public void A_page_with_every_action_asks_for_the_methods_action(string method, AccessAction? named, AccessAction expected) =>
        Assert.That(AccessCatalog.Required(AccessCatalog.Find(AccessCatalog.Quotes)!, method, named), Is.EqualTo(expected));

    [TestCase("POST", AccessAction.Edit)]
    [TestCase("DELETE", AccessAction.Edit)]
    [TestCase("GET", AccessAction.View)]
    public void A_page_without_create_or_delete_asks_for_edit(string method, AccessAction expected) =>
        Assert.That(AccessCatalog.Required(AccessCatalog.Find(AccessCatalog.FabricStock)!, method, null), Is.EqualTo(expected));

    [Test]
    public void A_view_only_page_asks_for_view_whatever_the_method() =>
        Assert.That(AccessCatalog.Required(AccessCatalog.Find(AccessCatalog.WorkshopReports)!, "POST", null), Is.EqualTo(AccessAction.View));

    [Test]
    public void Policy_names_round_trip()
    {
        Assert.Multiple(() =>
        {
            Assert.That(AccessPolicies.Parse("access:quotes")?.Area.Key, Is.EqualTo(AccessCatalog.Quotes));
            Assert.That(AccessPolicies.Parse("access:quotes")?.Action, Is.Null);
            Assert.That(AccessPolicies.Parse(AccessPolicies.For(AccessCatalog.Quotes, AccessAction.Delete))?.Action,
                Is.EqualTo(AccessAction.Delete));
            Assert.That(AccessPolicies.Parse("access:nope"), Is.Null);
            Assert.That(AccessPolicies.Parse("access:quotes:fly"), Is.Null);
            Assert.That(AccessPolicies.Parse("access:quotes:view:x"), Is.Null);
            Assert.That(AccessPolicies.Parse("CatalogRead"), Is.Null);
        });
    }

    [Test]
    public void The_director_cannot_be_edited()
    {
        var result = new SaveRoleAccessCommandValidator().TestValidate(
            new SaveRoleAccessCommand(RoleNames.Director, 0, new Dictionary<string, IReadOnlyList<AccessAction>>()));

        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Test]
    public void A_save_needs_known_pages_their_own_actions_and_view_first()
    {
        var validator = new SaveRoleAccessCommandValidator();
        SaveRoleAccessCommand Save(string area, params AccessAction[] actions) =>
            new(RoleNames.Manager, 1, new Dictionary<string, IReadOnlyList<AccessAction>> { [area] = actions });

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(Save(AccessCatalog.Quotes, AccessAction.View, AccessAction.Delete)).IsValid, Is.True);
            Assert.That(validator.Validate(Save(AccessCatalog.Quotes)).IsValid, Is.True);
            Assert.That(validator.Validate(Save("nope", AccessAction.View)).IsValid, Is.False);
            Assert.That(validator.Validate(Save(AccessCatalog.FabricStock, AccessAction.View, AccessAction.Delete)).IsValid, Is.False);
            Assert.That(validator.Validate(Save(AccessCatalog.Quotes, AccessAction.Edit)).IsValid, Is.False);
        });
    }

    [Test]
    public void Built_in_roles_are_never_deleted_and_new_ones_need_a_name()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new DeleteRoleCommandValidator().Validate(new DeleteRoleCommand(RoleNames.Manager)).IsValid, Is.False);
            Assert.That(new DeleteRoleCommandValidator().Validate(new DeleteRoleCommand("custom-1a2b")).IsValid, Is.True);
            Assert.That(new CreateRoleCommandValidator().Validate(new CreateRoleCommand("  ")).IsValid, Is.False);
            Assert.That(new CreateRoleCommandValidator().Validate(new CreateRoleCommand(new string('а', 101))).IsValid, Is.False);
            Assert.That(new CreateRoleCommandValidator().Validate(new CreateRoleCommand("Технолог")).IsValid, Is.True);
        });
    }

    [Test]
    public void Api_action_names_are_lowercase_and_unknown_ones_are_refused()
    {
        var parsed = AccessActionNames.Parse(new Dictionary<string, IReadOnlyList<string>> { ["quotes"] = ["view", "edit", "view"] });

        Assert.Multiple(() =>
        {
            Assert.That(parsed!["quotes"], Is.EqualTo(new[] { AccessAction.View, AccessAction.Edit }));
            Assert.That(AccessActionNames.Parse(new Dictionary<string, IReadOnlyList<string>> { ["quotes"] = ["View"] }), Is.Null);
            Assert.That(AccessActionNames.Of(AccessAction.Delete), Is.EqualTo("delete"));
        });
    }
}
