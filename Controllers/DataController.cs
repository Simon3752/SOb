using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using SOb.Models;
using SOb.Data;
using System.Globalization;
using System.Data;

namespace SOb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly AppDB _context;

        public DataController(AppDB context) => _context = context;

        [HttpPost("upload")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File doesnt uploaded");
            if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase)) return BadRequest("Only .csv files allowed");

            var valuesList = new List<ValueEntry>();
            var fileName = file.FileName;

            using var transact = await _context.Database.BeginTransactionAsync();
            try
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" }))
                {
                    csv.Context.RegisterClassMap<ValueEntryMap>();

                    var recs = csv.GetRecords<dynamic>().ToList();

                    if (recs.Count < 1 || recs.Count > 10000) return BadRequest("Number of rows must be between 1 and 10000");

                    foreach (var rec in recs)
                    {
                        var dict = (IDictionary<string, object>)rec;

                        if (!dict.ContainsKey("Date") || !dict.ContainsKey("ExecutionTime") || !dict.ContainsKey("Value")) return BadRequest("One of the columms isnt exists (Date/ExecutionTime/Value).");

                        string rawDate = dict["Date"]?.ToString() ?? "";
                        string rawEx = dict["ExecutionTime"]?.ToString() ?? "";
                        string rawVal = dict["Value"]?.ToString() ?? "";

                        if (string.IsNullOrWhiteSpace(rawDate) || string.IsNullOrWhiteSpace(rawEx) || string.IsNullOrWhiteSpace(rawVal)) return BadRequest("Cant read empty values");
                        if (!DateTime.TryParseExact(rawDate, "yyyy-MM-ddTHH-mm-ss.ffffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime parsedDate)) return BadRequest($"Wrong date format {rawDate}, only yyyy-MM-ddTHH-mm-ss.ffffZ is allowed");
                        if (!double.TryParse(rawEx, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedExecTime)) return BadRequest($"Wrong format of executionTime: {rawEx}");
                        if (!double.TryParse(rawVal, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedValue)) return BadRequest($"Wrong format of value: {rawVal}");
                        if (parsedDate < new DateTime(2000, 1, 1) || parsedDate > DateTime.UtcNow) return BadRequest($"Date {rawDate} is out of range (from 2000-01-01 to nowadays).");
                        if (parsedExecTime < 0) return BadRequest("ExecutionTime must be >= 0.");
                        if (parsedValue < 0) return BadRequest("Value must be >= 0.");

                        valuesList.Add(new ValueEntry { date = parsedDate, value = parsedValue, fileName = fileName, executionTime = parsedExecTime });
                    }
                }

                var sortedVals = valuesList.OrderBy(v => v.value).ToList();
                var cnt = sortedVals.Count;

                var result = new ProcessingResult
                {
                    fileName = fileName,
                    startTime = valuesList.Min(v => v.date),
                    dTime = (valuesList.Max(v => v.date) - valuesList.Min(v => v.date)).TotalSeconds,
                    avgExecutionTime = valuesList.Average(v => v.executionTime),
                    avgValue = valuesList.Average(v => v.value),
                    maxValue = valuesList.Max(v => v.value),
                    minValue = valuesList.Min(v => v.value),
                    midValue = cnt % 2 == 0 ? (sortedVals[cnt / 2 - 1].value + sortedVals[cnt / 2].value) / 2.0 : sortedVals[cnt / 2].value
                };

                await _context.Values.Where(v => v.fileName == fileName).ExecuteDeleteAsync();
                await _context.results.Where(r => r.fileName == fileName).ExecuteDeleteAsync();

                _context.Values.AddRange(valuesList);
                _context.results.Add(result);

                await _context.SaveChangesAsync();
                await transact.CommitAsync();

                return Ok(new { message = "Date processing completed", fileName = fileName });
            } catch (Exception ex)
            {
                await transact.RollbackAsync();
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("results")]
        public async Task<IActionResult> GetResults(
            [FromQuery] string? fileName,
            [FromQuery] DateTime? startFrom,
            [FromQuery] DateTime? startTo,
            [FromQuery] double? minAvgValue,
            [FromQuery] double? maxAvgValue,
            [FromQuery] double? minAvgExec,
            [FromQuery] double? maxAvgExec)
        {
            var query = _context.results.AsQueryable();

            if (!string.IsNullOrEmpty(fileName)) query = query.Where(r => r.fileName.Contains(fileName));
            if (startFrom.HasValue) query = query.Where(r => r.startTime >= startFrom.Value);
            if (startTo.HasValue) query = query.Where(r => r.startTime <= startTo.Value);
            if (minAvgValue.HasValue) query = query.Where(r => r.avgValue >= minAvgValue.Value);
            if (maxAvgValue.HasValue) query = query.Where(r => r.avgValue <= maxAvgValue.Value);
            if (minAvgExec.HasValue) query = query.Where(r => r.avgExecutionTime >= minAvgExec.Value);
            if (maxAvgExec.HasValue) query = query.Where(r => r.avgExecutionTime <= maxAvgExec.Value);

            return Ok(await query.ToListAsync());
        }

        [HttpGet("values/{fileName}/last10")]
        public async Task<IActionResult> GetLast10(string fileName)
        {
            var data = await _context.Values
                .Where(v => v.fileName == fileName)
                .OrderByDescending(v => v.date)
                .Take(10)
                .ToListAsync();

            if (!data.Any()) return NotFound("Failed to find data for this file");

            return Ok(data);
        }
    }
}
