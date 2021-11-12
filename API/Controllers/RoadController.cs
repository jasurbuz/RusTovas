﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yol.API.Extensions;
using Yol.Data.Models;
using Yol.Services;
using Yol.Services.DTOs;
using Yol.Services.DTOs.RoadDtos;
using Yol.Services.IRepository;

namespace Yol.API.Controllers
{
    [Route("/api/[controller]/[action]")]
    [ApiController]
    public class RoadController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public RoadController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoad([FromBody] RoadForCreationDTO creationDto)
        {
            var road = _mapper.Map<Road>(creationDto);
            
            if(creationDto.Images != null)
                foreach(var image in creationDto.Images)
                {
                    Image image1 = new Image() { Id = Guid.NewGuid(), RoadId = road.Id, FileName = await _unitOfWork.SaveFileAsync(image)};
                    await _unitOfWork.Images.Insert(image1);
                }
            await _unitOfWork.Roads.Insert(road);
            await _unitOfWork.Save();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetRoads([FromForm] RequestParams requestParams)
        {
            if (requestParams.OrderBy is null)
            {
                requestParams.OrderBy = "StartedAt";
            }
            var roads = await _unitOfWork.Roads.GetPagedList(requestParams, order => order.OrderBy(requestParams.OrderBy), 
                new List<string>() { "Admin", "Company" });
            var response = new ResponseDto
            {
                PageCount = roads.PageCount,
                Total = roads.TotalItemCount,
                Current = roads.PageNumber,
                PageSize = roads.PageSize,
                HasPreviousPage = roads.HasPreviousPage,
                HasNextPage = roads.HasNextPage,
                FirstItemOnPage = roads.FirstItemOnPage,
                LastItemOnPage = roads.LastItemOnPage,
                Data = _mapper.Map<IEnumerable<RoadDTO>>(roads)
            };
            return Ok(response);
        }
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetRoad(Guid Id)
        {
            var road = await _unitOfWork.Roads.Get(p => p.Id == Id, new List<string>() { "Admin", "Company", "Images" });
            if(road is null)
                return NotFound("Road doesn't found");
            var images = new List<string>();
            
            foreach (var image in road.Images)
                images.Add($"{CustomServices.GetBaseUrl()}/Images/{image.FileName}");
            
            var responce = _mapper.Map<RoadDTO>(road);
            responce.Images = images;

            if(road.Coordinate is not null)
                responce.Cordinates = JsonConvert.DeserializeObject<List<decimal[]>>(road.Coordinate);

            return Ok(responce);

        }
    }
}
