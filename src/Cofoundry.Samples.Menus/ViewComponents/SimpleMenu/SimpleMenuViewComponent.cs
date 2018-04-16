﻿using Cofoundry.Core;
using Cofoundry.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cofoundry.Samples.Menus
{
    /// <summary>
    /// <param>
    /// Here we use a view component to load the menu data
    /// from Cofoundry repositories, map it into a view model
    /// and then return a view for rendering.
    /// </param>
    /// <para>
    /// If you wanted to support different styles of menu, you 
    /// could use the menuId parameter as the view name, e.g. "main"
    /// or "footer". Alternatively you could have the menu style
    /// as a property on the custom entity data model and then 
    /// pass that through as the view name.
    /// </para>
    /// </summary>
    public class SimpleMenuViewComponent : ViewComponent
    {
        private readonly ICustomEntityRepository _customEntityRepository;
        private readonly IPageRepository _pageRepository;

        public SimpleMenuViewComponent(
            ICustomEntityRepository customEntityRepository,
            IPageRepository pageRepository
            )
        {
            _customEntityRepository = customEntityRepository;
            _pageRepository = pageRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(string menuId)
        {
            var viewModel = new SimpleMenuViewModel();
            viewModel.MenuId = menuId;

            var menuEntity = await GetMenuByIdAsync(menuId);

            if (menuEntity == null) return View(viewModel);
            var dataModel = (SimpleMenuDataModel)menuEntity.Model;

            // Id range queries return a dictionary to allow easy lookups
            // but in this case, we simply need to order them correctly 
            // and return the collection
            var allPages = await _pageRepository.GetPageRoutesByIdRangeAsync(dataModel.PageIds);
            viewModel.Pages = allPages
                .FilterAndOrderByKeys(dataModel.PageIds)
                .ToList();

            return View(viewModel);
        }

        /// <summary>
        /// Note that this query isn't optimal and does client side filtering. This
        /// will be fixed in an upcoming release. See issue #81.
        /// </summary>
        private async Task<CustomEntityRenderSummary> GetMenuByIdAsync(string menuId)
        {
            // To be relaced by a get custom entity by url slug query
            var customEntityQuery = new GetCustomEntityRenderSummariesByDefinitionCodeQuery(SimpleMenuDefinition.DefinitionCode);
            var menus = await _customEntityRepository.GetCustomEntityRenderSummariesByDefinitionCodeAsync(customEntityQuery);

            return menus.FirstOrDefault(s => s.UrlSlug == menuId);
        }
    }
}