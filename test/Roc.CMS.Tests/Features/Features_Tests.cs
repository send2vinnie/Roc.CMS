﻿using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.MultiTenancy;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Roc.CMS.Authorization.Users;
using Roc.CMS.Authorization.Users.Dto;
using Roc.CMS.Editions;
using Roc.CMS.Editions.Dto;
using Roc.CMS.Features;
using Shouldly;
using Xunit;

namespace Roc.CMS.Tests.Features
{
    public class Features_Tests : AppTestBase
    {
        private readonly IEditionAppService _editionAppService;
        private readonly IUserAppService _userAppService;

        public Features_Tests()
        {
            LoginAsHostAdmin();
            _editionAppService = Resolve<IEditionAppService>();
            _userAppService = Resolve<IUserAppService>();
        }

        [MultiTenantFact]
        public async Task Should_Not_Create_User_More_Than_Allowed_Count()
        {
            //Getting edition for edit
            var output = await _editionAppService.GetEditionForEdit(new NullableIdDto(null));

            //Changing a sample feature value
            var maxUserCountFeature = output.FeatureValues.FirstOrDefault(f => f.Name == AppFeatures.MaxUserCount);
            if (maxUserCountFeature != null)
            {
                maxUserCountFeature.Value = "2";
            }

            await _editionAppService.CreateOrUpdateEdition(
                new CreateOrUpdateEditionDto
                {
                    Edition = new EditionEditDto
                    {
                        DisplayName = "Premium Edition"
                    },
                    FeatureValues = output.FeatureValues
                });


            var premiumEditon = (await _editionAppService.GetEditions()).Items.FirstOrDefault(e => e.DisplayName == "Premium Edition");
            premiumEditon.ShouldNotBeNull();

            await UsingDbContextAsync(async context =>
            {
                var tenant = await context.Tenants.SingleAsync(t => t.TenancyName == AbpTenantBase.DefaultTenantName);
                tenant.EditionId = premiumEditon.Id;

                context.SaveChanges();
            });

            LoginAsDefaultTenantAdmin();

            // This is second user (first is tenant admin)
            await _userAppService.CreateOrUpdateUser(
                new CreateOrUpdateUserInput
                {
                    User = new UserEditDto
                    {
                        EmailAddress = "test@mail.com",
                        Name = "John",
                        Surname = "Nash",
                        UserName = "johnnash",
                        Password = "123qwE*"
                    },
                    AssignedRoleNames = new string[] { }
                });

            //Act
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(
                async () =>
                    await _userAppService.CreateOrUpdateUser(
                        new CreateOrUpdateUserInput
                        {
                            User = new UserEditDto
                            {
                                EmailAddress = "test2@mail.com",
                                Name = "Ali Rıza",
                                Surname = "Adıyahşi",
                                UserName = "alirizaadiyahsi",
                                Password = "123qwE*"
                            },
                            AssignedRoleNames = new string[] { }
                        })
            );

            exception.Message.ShouldContain("Reached to maximum allowed user count!");
        }
    }
}
