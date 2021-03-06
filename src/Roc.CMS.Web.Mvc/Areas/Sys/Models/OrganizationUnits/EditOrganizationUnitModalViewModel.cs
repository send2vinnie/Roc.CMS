﻿using Abp.AutoMapper;
using Abp.Organizations;

namespace Roc.CMS.Web.Areas.Sys.Models.OrganizationUnits
{
    [AutoMapFrom(typeof(OrganizationUnit))]
    public class EditOrganizationUnitModalViewModel
    {
        public long? Id { get; set; }

        public string DisplayName { get; set; }
    }
}