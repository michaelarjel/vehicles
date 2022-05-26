﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vehicles.API.Data;
using Vehicles.API.Data.Entities;
using Vehicles.API.Helpers;
using Vehicles.API.Models.Requests;

namespace Vehicles.API.Controllers.API
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class HistoriesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public HistoriesController(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<History>> GetHistory(int id)
        {
            History history = await _context.Histories
                .Include(x => x.Details)
                .ThenInclude(x => x.Procedure)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (history == null)
            {
                return NotFound();
            }

            return history;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutHistory(int id, HistoryRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            Vehicle vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
            if (vehicle == null)
            {
                return BadRequest("Vehículo no existe.");
            }

            User user = await _userHelper.GetUserAsync(Guid.Parse(request.UserId));
            if (user == null)
            {
                return BadRequest("Ese usuario no existe.");
            }

            History history = await _context.Histories.FindAsync(id);
            if (history == null)
            {
                return BadRequest("Historia no existe.");
            }

            history.Mileage = request.Mileage;
            history.Remarks = request.Remarks;
            _context.Entry(history).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<History>> PostHistory(HistoryRequest request)
        {
            Vehicle vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
            if (vehicle == null)
            {
                return BadRequest("Vehículo no existe.");
            }

            User user = await _userHelper.GetUserAsync(Guid.Parse(request.UserId));
            if (user == null)
            {
                return BadRequest("Ese usuario no existe.");
            }

            History history = new History
            {
                Date = DateTime.UtcNow,
                Mileage = request.Mileage,
                Remarks = request.Remarks,
                User = user,
                Vehicle = vehicle,
                Details = new List<Detail>(),
            };

            _context.Histories.Add(history);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetHistory", new { id = history.Id }, history);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            History history = await _context.Histories
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (history == null)
            {
                return NotFound();
            }

            _context.Histories.Remove(history);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
