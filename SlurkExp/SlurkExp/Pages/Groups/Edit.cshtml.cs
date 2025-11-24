using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;

namespace SlurkExp.Pages.Groups
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly SlurkExpDbContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            SlurkExpDbContext context,
            ILogger<EditModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Group Group { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            Group = await _context.Groups.FirstOrDefaultAsync(x => x.GroupId.Equals(id));

            if (Group == null)
            {
                return RedirectToPage("./index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Group).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Groups.AnyAsync(e => e.GroupId == Group.GroupId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("../groups/edit", new { id = Group.GroupId });
        }

        public async Task<IActionResult> OnPostCopyAsync()
        {
            Group newGroup = new Group();

            var group = await _context.Groups.FirstOrDefaultAsync(x => x.GroupId.Equals(Group.GroupId));
            if(group != null)
            {
                newGroup = group;
                newGroup.GroupId = 0;
                newGroup.SortGroup = 0;
                newGroup.SortOrder = 0;
                newGroup.Status = 0;
                newGroup.Checkin = 0;
                newGroup.Checkout = 0;
                newGroup.WaitingRoomId = 0;
                newGroup.WaitingRoomTime = 0;
                newGroup.ChatRoomId = 0;
                newGroup.ChatRoomTime = 0;
                newGroup.Created = DateTime.UtcNow;
                newGroup.Updated = DateTime.UtcNow;
                _context.Groups.Add(newGroup);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("../groups/edit", new { id = newGroup.GroupId });
        }
    }
}
