using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ContactPro.Data;
using ContactPro.Models;
using ContactPro.Models.ViewModels;
using ContactPro.Enums;
using ContactPro.Services;
using ContactPro.Services.Interfaces;
using ContactPro.Models.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ContactPro.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager; //the underscore is a naming convention that helps identify a variable as being private.
        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;
        private readonly IEmailSender _emailService;

        public ContactsController(ApplicationDbContext context, 
                                  UserManager<AppUser> userManager,
                                  IImageService imageService,
                                  IAddressBookService addressBookService,
                                  IEmailSender emailService)//injection. Where we push information into the controller. it allows access to objects established anywhere inside the properties.
        {
            _context = context; // allows access to database.
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;
            _emailService = emailService;
        }

        // GET: Contacts
        [Authorize]
        public IActionResult Index(int categoryId, string swalMessage = null) //int categoryId links to the Contacts index.cshtml
        {
            ViewData["SwalMessage"] = swalMessage; //sweet alert message is represented by swal.

            var contacts = new List<Contact>();
            /*List<Contact> contacts = new List<Contact>();*/ //explicit decoration. Shortens syntax.
            string appUserId = _userManager.GetUserId(User);

            //return userId and its associated contacts and categories.
            AppUser appUser = _context.Users
                                      .Include(c => c.Contacts)
                                      .ThenInclude(c => c.Categories)
                                      .FirstOrDefault(u => u.Id == appUserId)!; // c represents AppUser class in this instance. u represents user. ! added at the end to avoid green null warning. Because a user will be logged in to see the values being pulled it is safe to ignore this warning.

            var categories = appUser.Categories;

            if(categoryId == 0) // This is for if "All Contacts" is selection. If zero "All Contacts". Else anything other than zero in the array.
            {
            contacts = appUser.Contacts.OrderBy(c => c.LastName)
                                       .ThenBy(c => c.FirstName)
                                       .ToList();
            } 
            else
            {
                contacts = appUser.Categories.FirstOrDefault(c => c.Id == categoryId)
                                   .Contacts
                                   .OrderBy(c => c.LastName)
                                   .ThenBy(c => c.FirstName)
                                   .ToList();
            }


            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", categoryId);

            return View(contacts);

        }

        [Authorize]
        public IActionResult SearchContacts(string searchString)
        {
            string appUserId = _userManager.GetUserId(User);
            var contacts = new List<Contact>();

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            AppUser appUser = _context.Users
                                      .Include(c => c.Contacts)
                                      .ThenInclude(c => c.Categories)
                                      .FirstOrDefault(u => u.Id == appUserId);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (String.IsNullOrEmpty(searchString)) //if is default search. Else is with a search string in place.
            {
                contacts = appUser.Contacts
                                  .OrderBy(c => c.LastName)
                                  .ThenBy(c => c.FirstName)
                                  .ToList();
            }
            else
            {
                contacts = appUser.Contacts.Where(c => c.FullName!.ToLower().Contains(searchString.ToLower())) // convert both FullName and searchString to lower in order to make search easier for the user with more success.
                                  .OrderBy(c => c.LastName)
                                  .ThenBy(c => c.FirstName)
                                  .ToList();
            }
            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name", 0); //0 is present to select all values and not filter.

            return View(nameof(Index), contacts); //make sure that we are routing to Index view but with contacts result being inserted.
        }

        //Email
        [Authorize]
        public async Task<IActionResult> EmailContact(int id) //When making an action result use a method, you must wrap the phrase action result within a task. Especially if async is present at the beginning. Be sure to follow routing map as listing in Program.cs. This is why we use lowercase id as anything else will route the page into an error page.
        {
            string appUserId = _userManager.GetUserId(User);
            Contact contact = await _context.Contacts.Where(c => c.Id == id && c.AppUserId == appUserId)
                                                     .FirstOrDefaultAsync();

            if(contact == null)
            {
                return NotFound();
            }

            EmailData emailData = new EmailData()
            {
                EmailAddress = contact.Email,
                FirstName = contact.FirstName,
                LastName = contact.LastName
            };

            EmailContactViewModel model = new EmailContactViewModel()
            {
                Contact = contact,
                EmailData = emailData
            };

            return View(model);
        }

        //Email: Post
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EmailContact(EmailContactViewModel ecvm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _emailService.SendEmailAsync(ecvm.EmailData.EmailAddress, ecvm.EmailData.Subject, ecvm.EmailData.Body);
                    return RedirectToAction("Index", "Contacts", new {swalMessage = "Success: Email Sent!"}); //Success has to be present

                }
                catch
                {
                    return RedirectToAction("Index", "Contacts", new { swalMessage = "Error: Email Send Failed!" }); //Error has to be present.
                    throw;
                }
            }
            return View(ecvm);
        }



        // GET: Contacts/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        [Authorize]
        public async Task<IActionResult> Create() //Needs to be written as a task because there is an async/await function being performed.
        {
            string appUserId = _userManager.GetUserId(User);
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            //Cast simply takes the result<States> and tells the States that they will be converted into a list. This is where our result/states will be stored.
            //A smarter explanation: The Cast<TResult>(IEnumerable) method enables the standard query operators to be invoked on non-generic collections by supplying the necessary type information. 
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name");
            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,ImageFile")] Contact contact, List<int> CategoryList) //Bind references all inputs by name. List<int> categoryList was added to ensure that categories could be read and saved. It could not be added to the bind traditionally because it is a multiselect list.
        {
            ModelState.Remove("AppUserId");
            if (ModelState.IsValid)
            {
                contact.AppUserId = _userManager.GetUserId(User);
                contact.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc); //When is Now. UTC is universal time code.

                if (contact.BirthDate != null)
                {
                    contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc); //SpecifyKind... first is what we want to convert. the 2nd is the information we are using. We are using the DateTimeKind.Utc taken in when created to create a Birthdate Value. This is all shared through the contact itself as the DateTimeKind.Utc was previously passed through to it when created with the AppUserId behind the scenes.
                }

                if (contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType; // looks at file and determines what type it is. Doc, Png, jpeg ect.
                    // with this final bit. we have saved images to the database.
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();

                //loop over all of the selected categories.
                foreach (int categoryId in CategoryList)
                {
                    await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id); //called method from the service. Is a Task the same as a Function?
                }
                //save each category selected to the contact categories table.

                return RedirectToAction(nameof(Index));
            }
            
            return RedirectToAction(nameof(Index)); //Takes us back to the list page instead of keeping us on the edit page.
     
        }

        // GET: Contacts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            string appUserId = _userManager.GetUserId(User);

            //var contact = await _context.Contacts.FindAsync(id);  Can't use this for security reasons. Possible to still find another user's contacts.
            var contact = await _context.Contacts.Where(c => c.Id == id && c.AppUserId == appUserId) 
                                                 .FirstOrDefaultAsync(); //Ensures that it has to have the user id and the appUserId in order for the contact to be found.
            if (contact == null)
            {
                return NotFound();
            }
            //ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserId);

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name", await _addressBookService.GetContactCategoryIdsAsync(contact.Id));
                                    

            return View(contact);
        }


        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, AppUserId, FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageFile,ImageData, ImageType")] Contact contact, List<int> CategoryList) //This was edited to reflect what was being taken in from the form.
        {
            if (id != contact.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    contact.Created = DateTime.SpecifyKind(contact.Created, DateTimeKind.Utc); //DateTime will always be available.
                    
                    if(contact.BirthDate != null) //Possible for this value to not exist if null.
                    {
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                    }

                    if(contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType; //IFormFile allows us to see what type.
                    }

                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    //save categories
                    //first step is to remove current categories. This will be followed by adding selected current categories.

                    List<Category> oldCategories = (await _addressBookService.GetContactCategoriesAsync(contact.Id)).ToList();
                    foreach (var category in oldCategories)
                    {
                        await _addressBookService.RemoveContactFromCategoryAsync(category.Id, contact.Id); //removes category by identifying the id.
                    }
                    foreach(int categoryid in CategoryList)
                    {
                        await _addressBookService.AddContactToCategoryAsync(categoryid, contact.Id);
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserId);
            return View(contact);
        }

        // GET: Contacts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            string appUserId = _userManager.GetUserId(User);

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);

            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string appUserId = _userManager.GetUserId(User);



            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);

            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }
            
            
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
          return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
