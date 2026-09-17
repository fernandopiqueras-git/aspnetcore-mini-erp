using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;using MiniErp.Data;using MiniErp.Security;
namespace MiniErp.Controllers;
[Authorize(Roles=AppRoles.Administrator)] public class AuditController(AppDbContext db):Controller { public async Task<IActionResult> Index()=>View(await db.AuditEntries.AsNoTracking().OrderByDescending(x=>x.Timestamp).Take(500).ToListAsync()); }
