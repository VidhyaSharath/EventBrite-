﻿using EventCatalogAPI.Data;
using EventCatalogAPI.Domain;
using EventCatalogAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace EventCatalogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EventController : ControllerBase
    {
        private readonly EventContext _context;
        private readonly IConfiguration _config;

        public EventController(EventContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // To get display of events with pagenumber, counts tec. on top.

        [HttpGet("[action]")]
        public async Task<IActionResult> EventItems(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 6)
        {
            var eventItemsCount = await _context.EventItems.LongCountAsync();

            var eventItems = await (from e in _context.EventItems
                                    join t in _context.EventTypes on e.EventTypeId equals t.Id
                                    join c in _context.EventCategories on e.EventCategoryId equals c.Id
                                    join sc in _context.EventSubCategories on e.EventSubCategoryId equals sc.Id
                                    join l in _context.Locations on e.LocationId equals l.Id
                                    join dt in _context.DatesAndTimes on e.DateAndTimeId equals dt.Id
                                    select new EventItemViewModel
                                    {
                                        Id = e.Id,
                                        Title = e.Title.ToUpper(),
                                        Description = e.Description.Trim(),
                                        PictureUrl = e.PictureUrl,
                                        Price = e.Price,
                                        Contact = e.Contact,
                                        EventTypeId = t.Id,
                                        EventType = t.Type,
                                        EventCategoryId = c.Id,
                                        EventCategory = c.Category,
                                        EventSubCategoryId = sc.Id,
                                        EventSubCategory = sc.SubCategory,
                                        LocationId = l.Id,
                                        Location = l.Address ?? "Online Event",
                                        DateAndTimeId = dt.Id,
                                        StartDateTime = dt.StartDateTime.ToString("f"),
                                        EndDateTime = dt.EndDateTime.ToString("f")
                                    }) 
                                .OrderBy(e => e.Title)
                                .Skip(pageIndex * pageSize)
                                .Take(pageSize)
                                .ToListAsync();     

            /*   var eventItems = await _context.EventItems
                               .OrderBy(c => c.Title)
                               .Skip(pageIndex * pageSize)
                               .Take(pageSize)
                               .ToListAsync();     */

            eventItems = ChangePictureUrl(eventItems);

            var model = new PaginatedItemsViewModel<EventItemViewModel>
            {
                PageIndex = pageIndex,
                PageSize = eventItems.Count,
                Count = eventItemsCount, 
                Data = eventItems
            };
            
            return Ok(model);
        }

        //To replace url
        private List<EventItemViewModel> ChangePictureUrl(List<EventItemViewModel> eventItems)
        {
            eventItems.ForEach(eventItem =>
                eventItem.PictureUrl = eventItem.PictureUrl.Replace(
                                    "http://externaleventbaseurltobereplaced",
                                    _config["ExternalCatalogBaseUrl"]));
            return eventItems;
        }

        //To filter based on Types
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> EventTypes()
        {
            var types = await _context.EventTypes.ToListAsync();
            return Ok(types);
        }

        //Filter based on Categories
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> EventCategories()
        {
            var categories = await _context.EventCategories.ToListAsync();
            return Ok(categories);
        }

        //Filter based on subcategories
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> EventSubCategories()
        {
            var subCategories = await _context.EventSubCategories.ToListAsync();
            return Ok(subCategories);
        }
        //Filter based on Location
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> EventLocations()
        {
            var locations = await _context.Locations.ToListAsync();
            return Ok(locations);
        }

        //Filter based on DatesAndTimes
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> EventDatesAndTimes()
        {
            var datesAndTimes = await _context.DatesAndTimes.ToListAsync();
            return Ok(datesAndTimes);
        }

        [HttpGet("[action]/type/{eventTypeId}/category/{eventCategoryId}/subCategory/{eventSubCategoryId}")]
        public async Task<IActionResult> EventItems(
            int? eventTypeId,
            int? eventCategoryId,
            int? eventSubCategoryId,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 6
            )
        {
            // var query = (IQueryable<EventItemViewModel>)_context.EventItems;

            var query =  from e in _context.EventItems
                               join t in _context.EventTypes on e.EventTypeId equals t.Id
                               join c in _context.EventCategories on e.EventCategoryId equals c.Id
                               join sc in _context.EventSubCategories on e.EventSubCategoryId equals sc.Id
                               join l in _context.Locations on e.LocationId equals l.Id
                               join dt in _context.DatesAndTimes on e.DateAndTimeId equals dt.Id
                               select new EventItemViewModel
                               {
                                   Id = e.Id,
                                   Title = e.Title.ToUpper(),
                                   Description = e.Description.Trim(),
                                   PictureUrl = e.PictureUrl,
                                   Price = e.Price,
                                   Contact = e.Contact,
                                   EventTypeId = t.Id,
                                   EventType = t.Type,
                                   EventCategoryId = c.Id,
                                   EventCategory = c.Category,
                                   EventSubCategoryId = sc.Id,
                                   EventSubCategory = sc.SubCategory,
                                   LocationId = l.Id,
                                   Location = l.Address ?? "Online Event",
                                   DateAndTimeId = dt.Id,
                                   StartDateTime = dt.StartDateTime.ToString("f"),
                                   EndDateTime = dt.EndDateTime.ToString("f")
                               };

            if (eventTypeId.HasValue)
            {
                query = query.Where(c => c.EventTypeId == eventTypeId);
            }

            if (eventCategoryId.HasValue)
            {
                query = query.Where(c => c.EventCategoryId == eventCategoryId);
            }

            if (eventSubCategoryId.HasValue)
            {
                query = query.Where(c => c.EventSubCategoryId == eventSubCategoryId);
            }

            var eventItemsCount = await query.LongCountAsync();

            var eventItems = await query
                            .OrderBy(c => c.Title)
                            .Skip(pageIndex * pageSize)
                            .Take(pageSize)
                            .ToListAsync(); 

            eventItems = ChangePictureUrl(eventItems);

            var model = new PaginatedItemsViewModel<EventItemViewModel>
            {
                PageIndex = pageIndex,
                PageSize = eventItems.Count,
                Count = eventItemsCount,
                Data = eventItems
            };

            return Ok(model);
        }
        [HttpGet("[action]/location/{locationId}/type/{eventTypeId}/category/{eventCategoryId}/subCategory/{eventSubCategoryId}")]
        public async Task<IActionResult> Items(
            int? locationId,
            int? eventTypeId,
            int? eventCategoryId,
            int? eventSubCategoryId,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 6
            )
        {

            var query = (IQueryable<EventItemViewModel>)_context.EventItems;
            if (locationId.HasValue)
            {
                query = query.Where(c => c.LocationId == locationId);
            }

            if (eventTypeId.HasValue)
            {
                query = query.Where(c => c.EventTypeId == eventTypeId);
            }

            if (eventCategoryId.HasValue)
            {
                query = query.Where(c => c.EventCategoryId == eventCategoryId);
            }

            if (eventSubCategoryId.HasValue)
            {
                query = query.Where(c => c.EventSubCategoryId == eventSubCategoryId);
            }

            var eventItemsCount = await query.LongCountAsync();

            var eventItems = await query
                            .OrderBy(c => c.Title)
                            .Skip(pageIndex * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            eventItems = ChangePictureUrl(eventItems);

            var model = new PaginatedItemsViewModel<EventItemViewModel>
            {
                PageIndex = pageIndex,
                PageSize = eventItems.Count,
                Count = eventItemsCount,
                Data = eventItems
            };

            return Ok(model);
        }

    }
}
