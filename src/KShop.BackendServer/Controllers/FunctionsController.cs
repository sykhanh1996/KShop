using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KShop.BackendServer.Authorization;
using KShop.BackendServer.Constants;
using KShop.BackendServer.Data;
using KShop.BackendServer.Data.Entities;
using KShop.BackendServer.Helpers;
using KShop.ViewModels;
using KShop.ViewModels.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KShop.BackendServer.Controllers
{
    public class FunctionsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FunctionsController> _logger;
        private readonly IMapper _mapper;

        public FunctionsController(ApplicationDbContext context,
            ILogger<FunctionsController> logger,
            IMapper mapper
          )
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }
        [HttpPost]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.CREATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PostFunction([FromBody] FunctionCreateRequest request)
        {
            _logger.LogInformation("Begin PostFunction API");


            var dbFunction = await _context.Functions.FindAsync(request.Id);
            if (dbFunction != null)
                //return BadRequest(new ApiBadRequestResponse("Function with ID {0} is existed."));//dung dependecy
                return BadRequest();//dung dependecy

            var function = new Function()
            {
                Id = request.Id,
                Name = request.Name,
                ParentId = request.ParentId,
                SortOrder = request.SortOrder,
                Url = request.Url,
                Icon = request.Icon
            };
            _context.Functions.Add(function);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                _logger.LogInformation("End PostFunction API - Success");

                return CreatedAtAction(nameof(GetById), new { id = function.Id }, request);
            }

            _logger.LogInformation("End PostFunction API - Failed");

            //return BadRequest(new ApiBadRequestResponse(_localizer["CreateFunctionFail"]));
            return BadRequest();
        }

        [HttpGet]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetFunctions()
        {
            var functions = _context.Functions;

            var functionvms = await _mapper.ProjectTo<FunctionVm>(functions).ToListAsync();

            return Ok(functionvms);
        }

        [HttpGet("{functionId}/parents")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetFunctionsByParentId(string functionId)
        {
            var functions = _context.Functions.Where(x => x.ParentId == functionId);

            var functionvms = await _mapper.ProjectTo<FunctionVm>(functions).ToListAsync();

            return Ok(functionvms);
        }

        [HttpGet("filter")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetFunctionsPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _context.Functions.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Name.Contains(filter)
                                         || x.Id.Contains(filter)
                                         || x.Url.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);

            var lstItem = await _mapper.ProjectTo<FunctionVm>(items).ToListAsync();
            var pagination = new Pagination<FunctionVm>
            {
                Items = lstItem,
                TotalRecords = totalRecords,
                PageSize = pageSize,
                PageIndex = pageIndex
            };
            return Ok(pagination);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var function = await _context.Functions.FindAsync(id);
            if (function == null)
                return NotFound();

            var functionVm = _mapper.Map<FunctionVm>(function);
            return Ok(functionVm);
        }

        [HttpPut("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.UPDATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PutFunction(string id, [FromBody] FunctionCreateRequest request)
        {
            var function = await _context.Functions.FindAsync(id);
            if (function == null)
                //return NotFound(new ApiNotFoundResponse(_returnString.ReturnString(_localizer["Cannotfoundfunction"], id)));
                return NotFound();

            function.Name = request.Name;
            function.ParentId = request.ParentId;
            function.SortOrder = request.SortOrder;
            function.Url = request.Url;
            function.Icon = request.Icon;

            _context.Functions.Update(function);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            //return BadRequest(new ApiBadRequestResponse(_localizer["CreateFunctionFail"]));
            return BadRequest();
        }

        [HttpDelete("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteFunction(string id)
        {
            var function = await _context.Functions.FindAsync(id);
            //var errMess = _returnString.ReturnString(_localizer["Cannotfoundfunction"], id);

            if (function == null)
                //return NotFound(new ApiNotFoundResponse(errMess));
                return NotFound();

            _context.Functions.Remove(function);

            var commands = _context.CommandInFunctions.Where(x => x.FunctionId == id);
            _context.CommandInFunctions.RemoveRange(commands);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var functionvm = _mapper.Map<FunctionVm>(function);
                return Ok(functionvm);
            }
            //return BadRequest(new ApiBadRequestResponse(_localizer["DeleteFunctionFail"]));
            return BadRequest();
        }
        [HttpGet("{functionId}/commands")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommandsInFunction(string functionId)
        {
            var query = from a in _context.Commands
                        join cif in _context.CommandInFunctions on a.Id equals cif.CommandId into result1
                        from commandInFunction in result1.DefaultIfEmpty()
                        join f in _context.Functions on commandInFunction.FunctionId equals f.Id into result2
                        from function in result2.DefaultIfEmpty()
                        select new
                        {
                            a.Id,
                            a.Name,
                            commandInFunction.FunctionId
                        };

            query = query.Where(x => x.FunctionId == functionId);

            var data = await query.Select(x => new CommandVm()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }
        [HttpPost("{functionId}/commands")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.CREATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PostCommandToFunction(string functionId, [FromBody] CommandAssignRequest request)
        {
            foreach (var commandId in request.CommandIds)
            {
                if (await _context.CommandInFunctions.FindAsync(commandId, functionId) != null)
                    //return BadRequest(new ApiBadRequestResponse(_localizer["CommandExisted"]));
                    return BadRequest();

                var entity = new CommandInFunction()
                {
                    CommandId = commandId,
                    FunctionId = functionId
                };

                _context.CommandInFunctions.Add(entity);
            }

            if (request.AddToAllFunctions)
            {
                var otherFunctions = _context.Functions.Where(x => x.Id != functionId);
                foreach (var function in otherFunctions)
                {
                    foreach (var commandId in request.CommandIds)
                    {
                        if (await _context.CommandInFunctions.FindAsync(request.CommandIds, function.Id) == null)
                        {
                            _context.CommandInFunctions.Add(new CommandInFunction()
                            {
                                CommandId = commandId,
                                FunctionId = function.Id
                            });
                        }
                    }
                }
            }
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return CreatedAtAction(nameof(GetById), new { request.CommandIds, functionId });
            }

            //return BadRequest(new ApiBadRequestResponse(_localizer["AddCommandFail"]));
            return BadRequest();
        }
        [HttpDelete("{functionId}/commands")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.UPDATE)]
        public async Task<IActionResult> DeleteCommandToFunction(string functionId, [FromQuery] CommandAssignRequest request)
        {
            foreach (var commandId in request.CommandIds)
            {
                var entity = await _context.CommandInFunctions.FindAsync(commandId, functionId);
                if (entity == null)
                    //return BadRequest(new ApiBadRequestResponse(_localizer["CommandExisted"]));
                    return BadRequest();

                _context.CommandInFunctions.Remove(entity);
            }

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return Ok();
            }

            //return BadRequest(new ApiBadRequestResponse(_localizer["DeleteCommandFail"]));
            return BadRequest();
        }
    }
}
