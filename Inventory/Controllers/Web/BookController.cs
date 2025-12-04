using Inventory.Database.Entities;
using Inventory.Database.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers.Web;

public class BookController : Controller
{
    private readonly IBookService _bookService;

    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }

    // GET: Book
    public async Task<IActionResult> Index()
    {
        var books = await _bookService.GetAllAsync();
        return View(books);
    }

    // GET: Book/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book == null) return NotFound();
        return View(book);
    }

    // GET: Book/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Book/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book)
    {
        if (ModelState.IsValid)
        {
            await _bookService.CreateAsync(book);
            return RedirectToAction(nameof(Index));
        }
        return View(book);
    }

    // GET: Book/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book == null) return NotFound();
        return View(book);
    }

    // POST: Book/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Book book)
    {
        if (id != book.Id) return NotFound();

        if (ModelState.IsValid)
        {
            await _bookService.UpdateAsync(book);
            return RedirectToAction(nameof(Index));
        }
        return View(book);
    }

    // GET: Book/Delete/5
    public async Task<IActionResult> Delete(Guid id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book == null) return NotFound();
        return View(book);
    }

    // POST: Book/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _bookService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
