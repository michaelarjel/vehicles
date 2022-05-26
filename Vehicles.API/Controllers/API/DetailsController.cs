﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Vehicles.API.Data;
using Vehicles.API.Data.Entities;
using Vehicles.API.Models.Requests;

namespace Vehicles.API.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetailsController : ControllerBase
    {
        private readonly DataContext _context;

        public DetailsController(DataContext context)
        {
            _context = context;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetail(int id, DetailRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            Procedure procedure = await _context.Procedures.FindAsync(request.ProcedureId);
            if (procedure == null)
            {
                return BadRequest("Procedimiento no existe.");
            }

            Detail detail = await _context.Details.FindAsync(request.Id);
            if (detail == null)
            {
                return BadRequest("Detalle no existe.");
            }

            detail.LaborPrice = request.LaborPrice;
            detail.Procedure = procedure;
            detail.Remarks = request.Remarks;
            detail.SparePartsPrice = request.SparePartsPrice;
            _context.Entry(detail).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Detail>> PostDetail(DetailRequest request)
        {
            History history = await _context.Histories.FindAsync(request.HistoryId);
            if (history == null)
            {
                return BadRequest("Historia no existe.");
            }

            Procedure procedure = await _context.Procedures.FindAsync(request.ProcedureId);
            if (history == null)
            {
                return BadRequest("Procedimiento no existe.");
            }

            Detail detail = new Detail
            {
                History = history,
                LaborPrice = request.LaborPrice,
                Procedure = procedure,
                Remarks = request.Remarks,
                SparePartsPrice = request.SparePartsPrice,
            };

            _context.Details.Add(detail);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetDetail", new { id = detail.Id }, detail);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetail(int id)
        {
            Detail detail = await _context.Details.FindAsync(id);
            if (detail == null)
            {
                return NotFound();
            }

            _context.Details.Remove(detail);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
