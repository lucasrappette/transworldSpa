using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Migrations
{
    public static class InitialViews
    {
        public static void BuildInitialViews(this MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
if object_id('DealerStats','v') is not null
    drop view DealerStats;
");

            migrationBuilder.Sql(@"
create view DealerStats
as
	select
		c.Id as DealerId
	from Dealers c
");
        }
    }
}
