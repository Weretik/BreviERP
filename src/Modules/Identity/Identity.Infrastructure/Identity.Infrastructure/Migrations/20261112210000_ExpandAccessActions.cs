using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Migrations
{
    /// <summary>
    /// Data only: pages that gained «Створення» or «Видалення» on «Права доступу» (2026-11-12 audit) give those actions to
    /// every saved role that had «Редагування» there, because before the change «Редагування» covered them. Nobody gains
    /// or loses anything; from now on the director can untick them separately.
    /// </summary>
    public partial class ExpandAccessActions : Migration
    {
        private const int Create = 2;
        private const int Delete = 8;
        private const int Edit = 4;

        private static readonly (string Page, int Added)[] Pages =
        [
            ("embroidery", Create | Delete),
            ("tasks", Create | Delete),
            ("work-log", Create),
            ("workshop-staff", Create),
            ("payroll", Create),
            ("fabric-norms", Create | Delete),
            ("fabric-stock", Create | Delete),
            ("accounting", Create | Delete),
            ("reminders", Create | Delete)
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (page, added) in Pages)
            {
                migrationBuilder.Sql($$"""
                    UPDATE identity.role_access
                    SET grants = jsonb_set(grants, '{{{page}}}', to_jsonb((grants->>'{{page}}')::int | {{added}})),
                        version = version + 1
                    WHERE grants ? '{{page}}' AND ((grants->>'{{page}}')::int & {{Edit}}) = {{Edit}};
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to undo: with the old catalogue the extra actions are dropped when rights are read.
        }
    }
}
