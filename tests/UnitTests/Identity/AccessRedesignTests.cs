using Identity.Application.Features.Access.Commands.RenameRole;
using Identity.Domain.Authorization;
using Identity.Domain.Authorization.Roles;
using Identity.Infrastructure.Integrations.AspNetIdentity.Access;

namespace UnitTests.Identity;

/// <summary>«Права доступу» redesign (ACC-002): pages that were Admin-only, action names, the Admin-only block, renaming.</summary>
public sealed class AccessRedesignTests
{
    [Test]
    public void My_tasks_only_sees_and_moves_tasks_and_managing_them_is_its_own_page()
    {
        var tasks = AccessCatalog.Find(AccessCatalog.Tasks)!;
        var manage = AccessCatalog.Find(AccessCatalog.ManagerTasks)!;

        Assert.Multiple(() =>
        {
            Assert.That(tasks.Actions, Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(manage.Group, Is.EqualTo("Адміністрування"));
            Assert.That(manage.Title, Is.EqualTo("Завдання менеджерам"));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ManagerTasks], Is.EqualTo(manage.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager), Does.Not.ContainKey(AccessCatalog.ManagerTasks));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.Tasks],
                Is.EqualTo(AccessAction.View | AccessAction.Edit));
        });
    }

    [Test]
    public void Embroidery_cost_and_settings_are_their_own_page_next_to_the_embroidery_price()
    {
        var settings = AccessCatalog.Find(AccessCatalog.EmbroiderySettings)!;
        var keys = AccessCatalog.Areas.Select(x => x.Key).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(settings.Group, Is.EqualTo("Менеджер сервісу"));
            Assert.That(settings.Actions, Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(keys.IndexOf(AccessCatalog.EmbroiderySettings), Is.EqualTo(keys.IndexOf(AccessCatalog.Embroidery) + 1));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.EmbroiderySettings], Is.EqualTo(settings.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager), Does.Not.ContainKey(AccessCatalog.EmbroiderySettings));
        });
    }

    [Test]
    public void A_role_saved_before_the_new_pages_keeps_managing_tasks_and_seeing_the_embroidery_cost()
    {
        var savedAdmin = AccessGrantSource.Parse("{\"quotes\":7,\"tasks\":15}", RoleNames.Admin);
        var savedManager = AccessGrantSource.Parse("{\"tasks\":15}", RoleNames.Manager);

        Assert.Multiple(() =>
        {
            Assert.That(savedAdmin[AccessCatalog.ManagerTasks], Is.EqualTo(AccessCatalog.Find(AccessCatalog.ManagerTasks)!.Actions));
            Assert.That(savedAdmin[AccessCatalog.EmbroiderySettings], Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(savedAdmin[AccessCatalog.Tasks], Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(savedManager, Does.Not.ContainKey(AccessCatalog.ManagerTasks));
        });
    }

    [Test]
    public void Every_action_of_every_page_has_a_ukrainian_name_and_a_page_names_only_its_own_actions()
    {
        foreach (var area in AccessCatalog.Areas)
        {
            foreach (var action in new[] { AccessAction.View, AccessAction.Create, AccessAction.Edit, AccessAction.Delete })
            {
                var label = area.LabelOf(action);
                if (area.Actions.HasFlag(action))
                    Assert.That(label, Is.Not.Null.And.Not.Empty, $"{area.Key}/{action}");
                else
                    Assert.That(label, Is.Null, $"{area.Key}/{action}");
            }
        }
    }

    [Test]
    public void Pages_whose_generic_action_hides_its_meaning_say_what_it_does()
    {
        Assert.Multiple(() =>
        {
            Assert.That(AccessCatalog.Find(AccessCatalog.BackgroundTasks)!.LabelOf(AccessAction.Create), Is.EqualTo("Запускати зараз"));
            Assert.That(AccessCatalog.Find(AccessCatalog.LeadStats)!.LabelOf(AccessAction.Create), Is.EqualTo("Синхронізувати зараз"));
            Assert.That(AccessCatalog.Find(AccessCatalog.Forecast)!.LabelOf(AccessAction.View), Is.EqualTo("Бачить"));
        });
    }

    [Test]
    public void The_admin_only_block_lists_system_pages_and_actions_that_no_page_grants()
    {
        var titles = AccessCatalog.AdminOnly.Select(x => x.Title).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(titles, Does.Contain("Користувачі"));
            Assert.That(titles, Does.Contain("Права доступу"));
            Assert.That(titles, Does.Contain("Налаштування ШІ"));
            Assert.That(titles, Is.Unique);
            Assert.That(titles.Intersect(AccessCatalog.Areas.Select(x => x.Title)), Is.Empty);
        });
    }

    [Test]
    public void Only_a_role_made_on_the_page_is_renamed_and_its_new_title_needs_one_to_a_hundred_characters()
    {
        var validator = new RenameRoleValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new RenameRoleCommand("custom-1a2b", "Логіст")).IsValid, Is.True);
            Assert.That(validator.Validate(new RenameRoleCommand(RoleNames.Manager, "Продавець")).IsValid, Is.False);
            Assert.That(validator.Validate(new RenameRoleCommand("custom-1a2b", "  ")).IsValid, Is.False);
            Assert.That(validator.Validate(new RenameRoleCommand("custom-1a2b", new string('а', 101))).IsValid, Is.False);
        });
    }
}
